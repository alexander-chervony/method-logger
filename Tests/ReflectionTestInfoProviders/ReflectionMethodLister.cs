using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Stringification;
using TestLib.TestTypes;

namespace Tests.ReflectionTestInfoProviders
{
    public class ReflectionMethodLister : IMethodLister
    {
        private readonly Type[] _types;

        private ReflectionMethodLister(params Type[] types)
        {
            _types = types;
        }

        public static IEnumerable<string> GetTestMethods()
        {
            return CreateTestTypesLister().GetAllMethods(methodName => methodName.StartsWith("Object::", StringComparison.InvariantCultureIgnoreCase) || methodName.StartsWith("System.Object::", StringComparison.InvariantCultureIgnoreCase));
        }

        public static ReflectionMethodLister CreateTestTypesLister()
        {
            return new ReflectionMethodLister(
                typeof(GenericTestType<>),
                typeof(SomeTestType),
                typeof(StaticClass),
                typeof(StaticClass.FilesUtilityNativeMethods),
                typeof(PartialClass),
                typeof(GenericTestType<>.InnerClassB),
                typeof(GenericTestType<>.InnerClass));
        }

        public IEnumerable<IMethodInfoProvider> ListAllMethods()
        {
            return GetAllMethods().Select(m => new ReflectionMethodInfoProvider(m));
        }

        public IEnumerable<MethodInfo> GetAllMethods()
        {
            return _types
                .SelectMany(
                    t => t.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance | BindingFlags.GetProperty | BindingFlags.SetProperty)
                        .Select(m => m));
        }

        public IMethodInfoProvider GetMethodUnsafe(string nameSubstr)
        {
            MethodInfo found = GetAllMethods().FirstOrDefault(mi => mi.Name.Contains(nameSubstr));
            return found == null ? null : new ReflectionMethodInfoProvider(found);
        }
    }
}