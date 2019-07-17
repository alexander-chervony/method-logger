using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Mono.Cecil;
using Stringification;

namespace LibWeaver
{
    public class CecilMethodLister : IMethodLister
    {
        private readonly IEnumerable<string> _assemblyPath;
        private readonly Func<TypeDefinition, bool> _typeConstrint;

        public CecilMethodLister(IEnumerable<string> assemblyPath, Func<TypeDefinition, bool> typeConstrint = null)
        {
            _assemblyPath = assemblyPath;
            _typeConstrint = typeConstrint;
        }

        public void ProcessAssembies(
            Func<string, bool> shouldProcessAssembly,
            Action<AssemblyDefinition> preProcessAssemblyAction,
            Action<MethodDefinition, string> processMethod,
            Action<AssemblyDefinition, string> postProcessAssemblyAction)
        {
            foreach (var assemblyPath in _assemblyPath)
            {
                if (!shouldProcessAssembly(assemblyPath))
                {
                    continue;
                }

                AssemblyDefinition assembly = ReadAssembly(assemblyPath);

                if (preProcessAssemblyAction != null)
                {
                    preProcessAssemblyAction(assembly);
                }

                var typeDefinitions = assembly.MainModule.GetTypes();

                if (_typeConstrint != null)
                {
                    typeDefinitions = typeDefinitions.Where(_typeConstrint);
                }

                // method body can be null for example in delegates
                foreach (var md in typeDefinitions.SelectMany(type => type.Methods).Where(method => method.Body != null))
                {
                    string name = Stringifier.GetMethodDeclarationString(new CecilMethodInfoProvider(md));

                    if (name == null)
                    {
                        continue;
                    }

                    processMethod(md, name);
                }

                if (postProcessAssemblyAction != null)
                {
                    postProcessAssemblyAction(assembly, assemblyPath);
                }
            }
        }

        public IMethodInfoProvider GetMethodUnsafe(string nameSubstr)
        {
            foreach (var assemblyPath in _assemblyPath)
            {
                AssemblyDefinition assembly = ReadAssembly(assemblyPath);
                var typeDefinitions = assembly.MainModule.GetTypes();
                foreach (var md in typeDefinitions.SelectMany(type => type.Methods).Where(md => md.Name.Contains(nameSubstr)))
                {
                    return new CecilMethodInfoProvider(md);
                }
            }
            return null;
        }

        private static AssemblyDefinition ReadAssembly(string assemblyPath)
        {
            return AssemblyDefinition.ReadAssembly(assemblyPath, new ReaderParameters { AssemblyResolver = LibWeaver.GetCurrentDirectoryAssemblyResolver(assemblyPath) });
        }

        public IEnumerable<IMethodInfoProvider> ListAllMethods()
        {
            var methodInfoProviders = new List<IMethodInfoProvider>();
            var processedAssemblies = new HashSet<AsmFingerprint>();

            ProcessAssembies(
                assemblyPath =>
                {
                    var asmFingerprint = GetAsmFingerprint(assemblyPath);
                    if (!processedAssemblies.Contains(asmFingerprint))
                    {
                        processedAssemblies.Add(asmFingerprint);
                        return true;
                    }
                    return false;
                },
                null,
                (methodDefinition, name) =>
                {
                    methodInfoProviders.Add(new CecilMethodInfoProvider(methodDefinition));
                },
                null);

            return methodInfoProviders;
        }

        private AsmFingerprint GetAsmFingerprint(string assemblyPath)
        {
            var fileInfo = new FileInfo(assemblyPath);
            return new AsmFingerprint{ShortName = fileInfo.Name, Size = fileInfo.Length};
        }

        struct AsmFingerprint
        {
            public string ShortName;
            public long Size;
        }
    }
}