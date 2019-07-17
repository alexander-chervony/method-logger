using System;

namespace TestLib.Olololo
{
    public static class WeavingClassOpCodes
    {
        private static bool _weaveEnterFlag1;
        private static int _i;

        public static void WeavedOpCodesContainer()
        {
            //LogHelper.LogOnce("OLOLO", ref _weaveEnterFlag1);
            LogHelper.LogOnce("OLOLO", int.MaxValue);
        }

        public static int Property { get { return _i; } }

        public static int AutoProperty { get; set; }
    }

    public static class LogHelper
    {
        public static void LogOnce(string methodName, ref bool logged)
        {
            if (!logged)
            {
                Console.WriteLine(methodName);
                logged = true;
            }
        }
        
        public static void LogOnce(string methodName, int methodIndex)
        {
        }
    }

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


    /*
     * 
        private static bool _weaveEnterFlag1;

        public static void WeavedOpCodesContainer()
        {
            LogHelper.LogOnce("OLOLO", ref _weaveEnterFlag1);
        }
     
IL_0000: nop
IL_0001: ldstr "OLOLO"
IL_0006: ldsflda System.Boolean TestLib.TestTypes.WeavingClassOpCodes::_weaveEnterFlag1
IL_000b: call System.Void TestLib.TestTypes.LogHelper::LogOnce(System.String,System.Boolean&)
IL_0010: nop
IL_0011: ret    
     */
}