using System;
using System.Collections.Generic;
using System.Linq;

namespace Stringification
{
    public static class IMethodListerExtensions
    {
        /// <summary>
        /// Returns all method declaration strings except Lambdas.
        /// </summary>
        /// <param name="methodLister"></param>
        /// <param name="except"></param>
        /// <returns></returns>
        public static IEnumerable<string> GetAllMethods(this IMethodLister methodLister, Func<string, bool> except = null)
        {
            return GetAllMethodsEx(methodLister, except).OrderBy(s => s).ToArray();
        }
        
        private static IEnumerable<string> GetAllMethodsEx(this IMethodLister methodLister, Func<string, bool> except = null)
        {
            foreach (var info in methodLister.ListAllMethods())
            {
                string method = Stringifier.GetMethodDeclarationString(info);
                // exclude compiler-generated lambda methods like "SomeTestType::<MethodWithLambdasInside>b__0(object actParam)"
                if (method == null)
                {
                    continue;
                }
                if (except != null && except(method))
                {
                    continue;
                }
                yield return method;
            }
        }

        public static IMethodInfoProvider GetByName(this IMethodLister methodLister, string name)
        {
            var all = methodLister.ListAllMethods().Where(
                m =>
                {
                    string methodNamePart = Stringifier.GetMethodDeclarationString(m);
                    return methodNamePart != null && methodNamePart.Contains(name);
                }).ToArray();

            if (all.Length > 1)
            {
                throw new InvalidOperationException("More than one method with this name defined. Create unique method for your test case.");
            }

            return all.FirstOrDefault();
        }

        public static void PrintAllMethods(this IMethodLister methodLister, Func<string, bool> except = null)
        {
            foreach (var methodName in GetAllMethods(methodLister, except))
            {
                Console.WriteLine(methodName);
                Console.WriteLine("\r\n");
            }
        }

        public static void Print(this IEnumerable<string> methods)
        {
            foreach (var methodName in methods)
            {
                Console.WriteLine(methodName);
                Console.WriteLine("\r\n");
            }
        }
    }
}