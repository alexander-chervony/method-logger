using System;
using System.Linq;

namespace Stringification
{
    public static class MethodNameNormalizer
    {
        private static readonly TypeNameNormalizer TypeNameNormalizer = new TypeNameNormalizer();

        public static string Normalize(string methodName)
        {
            // this is the case for reflection and cecil for explicit interface implementation methods (see ShouldStringify_ExplicitInterfaceImplementationMethod)
            // another sample: System.Collections.Generic.IEnumerable<System.String>.GetEnumerator()
            // the algorithm assumes that method parameters doesn't include dots (parameter types processed by TypeNameNormalizer and those unqualified/shortened)
            var dotSeparated = methodName.Split('.');
            if (dotSeparated.Length > 1 && !methodName.Contains(".ctor") && !methodName.Contains(".cctor") && !IsLambdaMethod(methodName)) // skip ".ctor"
            {
                var exceptMethodName = string.Join(".", dotSeparated.Take(dotSeparated.Length - 1));
                var parts = exceptMethodName.Split(new []{'<', '>', ':', ','}, StringSplitOptions.RemoveEmptyEntries);
                foreach (var part in parts)
                {
                    var typeName = TypeNameNormalizer.Normalize(part);
                    if (typeName != part)
                    {
                        exceptMethodName = exceptMethodName.Replace(part, typeName);
                    }
                }
                methodName = exceptMethodName + "." + dotSeparated[dotSeparated.Length - 1];
            }

            return methodName;
        }

        // for compiler-generated lambda methods like "SomeTestType::<MethodWithLambdasInside>b__0(object actParam)"
        public static bool IsLambdaMethod(string method)
        {
            return method.StartsWith("<") || method.Contains("::<");
        }

        public static string TrimNamespace(this string actualString, string ns)
        {
            if (actualString.StartsWith(ns))
            {
                actualString = actualString.Substring(ns.Length);
            }
            return actualString;
        }

        /// <summary>
        /// Skip lambda. Dispose/finalize methods skipped intentionally. Finalize invocation may not be logged during app execution.
        /// </summary>
        public static bool SkipProcessingMethod(string name)
        {
            // don't weave Lambdas, Props, Events, Operations
            // '<%' , '%::<%>%' , '%::[gs]et[_]%' , '%::%.[gs]et[_]%' , '%::add[_]%' , '%::remove[_]%' , '%::op[_]%'
            if (
                name.StartsWith("<") ||
                name.Contains("::<") ||
                name.Contains("::get_Item(") || // indexers are out of scope for now
                name.Contains("::set_Item(") ||
                //name.Contains("::get_") ||
                //name.Contains(".get_") ||
                //name.Contains("::set_") ||
                //name.Contains(".set_") ||
                name.Contains("::add_") ||
                name.Contains(".add_") ||
                name.Contains("::remove_") ||
                name.Contains(".remove_") ||
                name.Contains("::op_") ||
                name.Contains("::Finalize()") ||
                name.Contains("::Dispose()")
                )
            {
                return true;
            }
            return false;
        }
    }
}