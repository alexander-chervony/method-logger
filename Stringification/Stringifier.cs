using System;
using System.Linq;
using System.Text;

namespace Stringification
{
    public static class Stringifier
    {
        private const string TypeFromMethodDelimiter = "::";
        private static readonly TypeNameNormalizer Normalizer = new TypeNameNormalizer();

        public static string GetTypePart(string stringifiedMethod)
        {
            return stringifiedMethod.Substring(0, stringifiedMethod.IndexOf(TypeFromMethodDelimiter, StringComparison.Ordinal));
        }

        /// <summary>
        /// Returns ONLY method declaration string. Not intended to work for lambda methods so returns NULL for them.
        /// Todo: refactor: actually lambda identification must be part of method lister. Bu it would require to make MethodLister generic from underlying source type (e.g. MethodLister(Of MethodDefinitionSyntax))
        /// </summary>
        /// <param name="method"></param>
        /// <returns></returns>
        public static string GetMethodDeclarationString(IMethodInfoProvider method)
        {
            try
            {
                if (method.IsPartialMethodWithoutBody)
                {
                    return null;
                }

                if (method.IsAbstractClassOrInterfaceMember)
                {
                    return null;
                }

                if (method.IsExternal)
                {
                    return null;
                }

                string methodNamePart = GetMethodNamePart(method);
                if (methodNamePart == null)
                {
                    return null;
                }

                string typeName = GetFullyQualifiedTypeName(method.DeclaringType);
                if (typeName == null)
                {
                    return null;
                }

                string parametersPart = GetMemberParametersPart(method);
                var parts = new[]
                {
                    typeName,
                    methodNamePart,
                    parametersPart
                };

                string methodDeclarationString = string.Concat(parts);

                if (MethodNameNormalizer.SkipProcessingMethod(methodDeclarationString))
                {
                    return null;
                }

                return methodDeclarationString; 
            }
            catch
            {
                Console.WriteLine("Exception stringifying method. Method details: \r\n" + method);
                throw;
            }
        }

        public static string GetFullyQualifiedTypeName(ITypeInfoProvider @type)
        {
            string typeName = GetTypeName(@type);

            if (MethodNameNormalizer.IsLambdaMethod(typeName))
            {
                return null;
            }

            string ns = @type.Namespace;
            if (!string.IsNullOrEmpty(ns))
            {
                typeName = ns + "." + typeName;
            }

            return typeName;
        }

        private static string GetMemberParametersPart(IMethodInfoProvider method)
        {
            switch (method.MethodType)
            {
                case MethodType.Method:
                    return GetMethodParametersPart(method);
                case MethodType.PropertyGetter:
                    return GetPropertyParametersPart(method);
                case MethodType.PropertySetter:
                    return GetPropertyParametersPart(method);
                default:
                    throw new NotImplementedException();
            }
        }

        private static string GetMethodNamePart(IMethodInfoProvider method)
        {
            var sb = new StringBuilder();
            sb.Append(TypeFromMethodDelimiter);
            sb.Append(MethodNameNormalizer.Normalize(method.Name));
            if (method.IsGenericMethodDefinition)
            {
                sb.Append("<");
                sb.Append(string.Join(", ", method.GetGenericArguments().Select(a => a.ToString())));
                sb.Append(">");
            }
            string methodNamePart = sb.ToString();

            // exclude compiler-generated lambda methods like "SomeTestType::<MethodWithLambdasInside>b__0(object actParam)"
            if (MethodNameNormalizer.IsLambdaMethod(methodNamePart))
            {
                return null;
            }

            return methodNamePart;
        }

        private static string GetTypeName(ITypeInfoProvider type)
        {
            try
            {
                var sb = new StringBuilder();

                var name = type.Name ?? type.FullName;

                if (type.IsGenericType)
                {
                    int lastIndexOf = name.LastIndexOf('`');
                    if (lastIndexOf != -1)
                    {
                        name = name.Substring(0, lastIndexOf);
                    }
                }
                sb.Append(name);

                if (type.IsGenericType || type.IsGenericTypeDefinition)
                {
                    sb.Append("<");
                    sb.Append(string.Join(", ", type.GetGenericArguments().Select(a => GetTypeName(a))));
                    sb.Append(">");
                }

                return Normalizer.Normalize(sb.ToString());
            }
            catch
            {
                Console.WriteLine("Exception stringifying type. Type details: \r\n" + type);
                throw;
            }
        }

        private static string GetMethodParametersPart(IMethodInfoProvider method)
        {
            var sb = new StringBuilder();
            IParameterInfoProvider[] parameters = method.GetParameters();
            sb.Append("(");
            for (int i = 0; i < parameters.Length; i++)
            {
                if (i > 0)
                {
                    sb.Append(", ");
                }
                var p = parameters[i];
                // only the last method parametrer should be checked for "params" prefix
                if ((parameters.Length - 1) == i && p.IsParams)
                {
                    sb.Append("params ");
                } 
                else if (p.IsOut)
                {
                    sb.Append("out ");
                }
                else if (p.IsRef)
                {
                    sb.Append("ref ");
                }
                string typeName = GetTypeName(p.ParameterType);
                sb.Append(p.IsDynamic ? "dynamic" : typeName);

                if (p.IsArray && !typeName.EndsWith("[]"))
                {
                    sb.Append("[]");
                }

                sb.Append(" ");
                sb.Append(p.Name);
            }
            sb.Append(")");
            var methodParametersPart = sb.ToString();
            return methodParametersPart;
        }

        private static string GetPropertyParametersPart(IMethodInfoProvider method)
        {
            if (method.MethodType == MethodType.PropertyGetter)
            {
                return "()";
            }
            if (method.MethodType == MethodType.PropertySetter)
            {
                //  string.Format("({0} value)", GetTypeName(method.GetParameters().First().ParameterType))
                return GetMethodParametersPart(method);
            }
            throw new InvalidOperationException();
        }
    }
}