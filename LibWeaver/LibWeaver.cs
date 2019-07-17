using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using MethodLogger;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Collections.Generic;

namespace LibWeaver
{
    public class LibWeaver
    {
        private static readonly Type _notifierType = typeof(WeavedNotifier);
        private static readonly MethodInfo _enterMethod = _notifierType.GetMethod("NotifyEnter", BindingFlags.Public | BindingFlags.Static, null, new[] { typeof(string), typeof(int) }, null);
        private static readonly MethodInfo _exitMethod = _notifierType.GetMethod("NotifyExit", BindingFlags.Public | BindingFlags.Static, null, new[] { typeof(string) }, null);
        private static readonly MethodInfo _jumpFromMethod = _notifierType.GetMethod("NotifyJumpOut", BindingFlags.Public | BindingFlags.Static, null, new[] { typeof(string) }, null);
        private static readonly MethodInfo _jumpBackMethod = _notifierType.GetMethod("NotifyJumpBack", BindingFlags.Public | BindingFlags.Static, null, new[] { typeof(string) }, null);

        public int WeaveList(IEnumerable<string> asmPath, Notify notify, Action<string> log)
        {
            MethodReference enterReference = null;
            MethodReference exitReference = null;
            MethodReference jumpFromReference = null;
            MethodReference jumpBackReference = null;

            new CecilMethodLister(asmPath).

                ProcessAssembies(
                    assemblyPath =>
                    {
                        log(string.Format("Weaving {0}", assemblyPath));
                        return true;
                    },
                    assembly =>
                    {
                        enterReference = (notify & Notify.Enter) == Notify.Enter ? assembly.MainModule.Import(_enterMethod) : null;
                        exitReference = (notify & Notify.Exit) == Notify.Exit ? assembly.MainModule.Import(_exitMethod) : null;
                        jumpFromReference = (notify & Notify.Jump) == Notify.Jump ? assembly.MainModule.Import(_jumpFromMethod) : null;
                        jumpBackReference = (notify & Notify.Jump) == Notify.Jump ? assembly.MainModule.Import(_jumpBackMethod) : null;
                    },
                    (methodDefinition, name) =>
                    {
                        CleanMethodCallOfPreviousWeaving(methodDefinition);

                        if ((notify & Notify.Enter) == Notify.Enter)
                            WeaveEnter(methodDefinition, enterReference, name, GetEnteredMethodIndex(name));

                        if ((notify & Notify.Jump) == Notify.Jump)
                            throw new NotImplementedException();
                            //WeaveJump(methodDefinition, jumpFromReference, jumpBackReference, name);

                        if ((notify & Notify.Exit) == Notify.Exit)
                            throw new NotImplementedException();
                            //WeaveExit(methodDefinition, exitReference, name);

                    },
                    (assembly, assemblyPath) =>
                    {

                        // todo: copy and write only if modified
                        CopyLoggerToWeavedAssemblyDir(assemblyPath);
                        assembly.Write(assemblyPath);
                    });

            return _weaveEnterMethodIndexes.Count;
        }

        private Dictionary<string, int> _weaveEnterMethodIndexes = new Dictionary<string, int>(); 
        private int GetEnteredMethodIndex(string name)
        {
            if (!_weaveEnterMethodIndexes.ContainsKey(name))
            {
                _weaveEnterMethodIndexes.Add(name, _weaveEnterMethodIndexes.Count);
            }
            int methodIndex = _weaveEnterMethodIndexes[name];
            return methodIndex;
        }

        public static DefaultAssemblyResolver GetCurrentDirectoryAssemblyResolver(string assemblyPath)
        {
            var dir = new FileInfo(assemblyPath).Directory;
            var resolver = new DefaultAssemblyResolver();
            resolver.AddSearchDirectory(dir.FullName);
            return resolver;
        }

        /// <summary>
        /// This needs to be done to get the type resolved in runtime. In other case every target project should reference MethodLogger.
        /// Copies all the dlls from notifier project output.
        /// </summary>
        private static void CopyLoggerToWeavedAssemblyDir(string assemblyPath)
        {
            string destinationDir = new FileInfo(assemblyPath).Directory.FullName;
            var notifierDir = new FileInfo(_notifierType.Assembly.Location).Directory;
            foreach (var info in notifierDir.GetFiles("*.dll").Where(i => !i.FullName.EndsWith("Mono.Cecil.dll")))
            {
                string destinationAssemblyPath = Path.Combine(destinationDir, info.Name);
                File.Copy(info.FullName, destinationAssemblyPath, true);
            }
        }

        private static void CleanMethodCallOfPreviousWeaving(MethodDefinition method)
        {
            ILProcessor ilProcessor = method.Body.GetILProcessor();

            Collection<Instruction> instructions = method.Body.Instructions;
            for (int i = instructions.Count - 1; i > 0; i--)
            {
                if (instructions[i].OpCode == OpCodes.Call && ((MethodReference)instructions[i].Operand).DeclaringType.FullName == _notifierType.FullName &&
                    (i - 2) >= 0 &&
                    instructions[i - 1].OpCode == OpCodes.Ldc_I4 &&
                    instructions[i - 2].OpCode == OpCodes.Ldstr)
                {
                    ilProcessor.Remove(instructions[i]);
                    ilProcessor.Remove(instructions[i - 1]);
                    ilProcessor.Remove(instructions[i - 2]);
                    i--;
                    i--;
                }
            }
            
            // old dlls support (when there was only one string parameter)
            for (int i = instructions.Count - 1; i > 0; i--)
            {
                if (instructions[i].OpCode == OpCodes.Call && ((MethodReference)instructions[i].Operand).DeclaringType.FullName == _notifierType.FullName &&
                    instructions[i - 1].OpCode == OpCodes.Ldstr)
                {
                    ilProcessor.Remove(instructions[i]);
                    ilProcessor.Remove(instructions[i - 1]);
                    i--;
                }
            }
        }

        private void WeaveEnter(MethodDefinition method, MethodReference methodReference, string name, int methodIndex)
        {
            var ilProcessor = method.Body.GetILProcessor();

            Instruction firstInstruction = method.Body.Instructions.First();

            Instruction loadNameInstruction = ilProcessor.Create(OpCodes.Ldstr, name);
            Instruction loadIndexInstruction = ilProcessor.Create(OpCodes.Ldc_I4, methodIndex);
            Instruction callEnterInstruction = ilProcessor.Create(OpCodes.Call, methodReference);

            ilProcessor.InsertBefore(firstInstruction, loadNameInstruction);
            ilProcessor.InsertAfter(loadNameInstruction, loadIndexInstruction);
            ilProcessor.InsertAfter(loadIndexInstruction, callEnterInstruction);
        }

        //private static void WeaveExit(MethodDefinition method, MethodReference exitReference, string name)
        //{
        //    ILProcessor ilProcessor = method.Body.GetILProcessor();

        //    IEnumerable<Instruction> returnInstructions = method.Body.Instructions.Where(instruction => instruction.OpCode == OpCodes.Ret).ToArray();
        //    foreach (var returnInstruction in returnInstructions)
        //    {
        //        Instruction loadNameInstruction = ilProcessor.Create(OpCodes.Ldstr, name);
        //        Instruction callExitReference = ilProcessor.Create(OpCodes.Call, exitReference);

        //        ilProcessor.InsertBefore(returnInstruction, loadNameInstruction);
        //        ilProcessor.InsertAfter(loadNameInstruction, callExitReference);
        //    }
        //}

        //private static void WeaveJump(MethodDefinition method, MethodReference jumpFromReference, MethodReference jumpBackReference, string name)
        //{
        //    ILProcessor ilProcessor = method.Body.GetILProcessor();

        //    IEnumerable<Instruction> callInstructions = method.Body.Instructions.Where(instruction => instruction.OpCode == OpCodes.Call).ToArray();
        //    foreach (var callInstruction in callInstructions)
        //    {
        //        Instruction loadNameForFromInstruction = ilProcessor.Create(OpCodes.Ldstr, name);
        //        Instruction callJumpFromInstruction = ilProcessor.Create(OpCodes.Call, jumpFromReference);

        //        ilProcessor.InsertBefore(callInstruction, loadNameForFromInstruction);
        //        ilProcessor.InsertAfter(loadNameForFromInstruction, callJumpFromInstruction);

        //        Instruction loadNameForBackInstruction = ilProcessor.Create(OpCodes.Ldstr, name);
        //        Instruction callJumpBackInstruction = ilProcessor.Create(OpCodes.Call, jumpBackReference);

        //        ilProcessor.InsertAfter(callInstruction, loadNameForBackInstruction);
        //        ilProcessor.InsertAfter(loadNameForBackInstruction, callJumpBackInstruction);
        //    }
        //}

        #region Alternate weawing - without method invocation - right inplace, however it's not allways weaved correctly - in autoprops for example

        /*
         * 
            private static bool _weaveEnterFlag1;

            public static void WeavedOpCodesContainer()
            {
                if (!_weaveEnterFlag1)
                {
                    _invocationLogger.Log(methodName);
                    _weaveEnterFlag1 = true;
                }
            }
     
    IL_0000: nop
    IL_0001: ldsfld System.Boolean TestLib.TestTypes.WeavingClassOpCodes::_weaveEnterFlag1  -- Push the value of field on the stack.
    IL_0006: stloc.0                                                                        -- Pop a value from stack into local variable 0.
    IL_0007: ldloc.0                                                                        -- Load local variable 0 onto stack.
    IL_0008: brtrue.s IL_0012                                                               -- Branch to target if value is non-zero (true), short form.
    IL_000a: nop
    IL_000b: ldc.i4.1                                                                       -- Push 1 onto the stack as int32.
    IL_000c: stsfld System.Boolean TestLib.TestTypes.WeavingClassOpCodes::_weaveEnterFlag1  -- Replace the value of field with val.
    IL_0011: nop
    IL_0012: ret
    
         */

        //private void WeaveEnter(MethodDefinition method, MethodReference methodReference, string name, TypeReference boolReference)
        //{
        //    var ilProcessor = method.Body.GetILProcessor();

        //    Instruction firstInstruction = method.Body.Instructions.First();

        //    var fieldRef = GetFlagFieldRef(method.DeclaringType, "Enter", boolReference);

        //    // compare flag and skip to next instruction if flag set (see WeavingClassOpCodes)
        //    // Push the value of field on the stack.
        //    var Ldsfld = ilProcessor.Create(OpCodes.Ldsfld, fieldRef);
        //    ilProcessor.InsertBefore(firstInstruction, Ldsfld);

        //    // Pop a value from stack into local variable 0
        //    var Stloc_0 = ilProcessor.Create(OpCodes.Stloc_0);
        //    ilProcessor.InsertAfter(Ldsfld, Stloc_0);

        //    // Load local variable 0 onto stack
        //    var Ldloc_0 = ilProcessor.Create(OpCodes.Ldloc_0);
        //    ilProcessor.InsertAfter(Stloc_0, Ldloc_0);

        //    // Branch to target if value is non-zero (true), short form
        //    var Brtrue_S = ilProcessor.Create(OpCodes.Brtrue, firstInstruction);
        //    ilProcessor.InsertAfter(Ldloc_0, Brtrue_S);

        //    // invoke method logging
        //    Instruction loadNameInstruction = ilProcessor.Create(OpCodes.Ldstr, name);
        //    Instruction callEnterInstruction = ilProcessor.Create(OpCodes.Call, methodReference);
        //    ilProcessor.InsertAfter(Brtrue_S, loadNameInstruction);
        //    ilProcessor.InsertAfter(loadNameInstruction, callEnterInstruction);

        //    // set flag to true
        //    // Push 1 onto the stack as int32
        //    var Ldc_I4_1 = ilProcessor.Create(OpCodes.Ldc_I4_1);
        //    ilProcessor.InsertAfter(callEnterInstruction, Ldc_I4_1);

        //    // Replace the value of field with val
        //    var Stsfld = ilProcessor.Create(OpCodes.Stsfld, fieldRef);
        //    ilProcessor.InsertAfter(Ldc_I4_1, Stsfld);
        //}

        #endregion

    }

    [Flags]
    public enum Notify
    {
        None = 0, 
        Enter = 1, 
        Exit = 2, 
        Jump = 4
    }
}