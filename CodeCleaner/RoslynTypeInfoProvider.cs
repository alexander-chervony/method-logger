using System.Collections.Generic;
using System.Linq;
using Roslyn.Compilers.CSharp;
using Stringification;

namespace CodeCleaner
{
    public class RoslynTypeInfoProvider : ITypeInfoProvider
    {
        private readonly TypeDeclarationSyntax _t;

        public RoslynTypeInfoProvider(TypeDeclarationSyntax t)
        {
            _t = t;
        }

        public bool IsGenericType { get { return _t.TypeParameterList != null; } }

        public string Namespace
        {
            get
            {
                NamespaceDeclarationSyntax namespaceDeclarationSyntax = _t.Ancestors().OfType<NamespaceDeclarationSyntax>().FirstOrDefault();
                if (namespaceDeclarationSyntax != null)
                {
                    return namespaceDeclarationSyntax.Name.ToString();

                }
                return string.Empty;
            }
        }

        public bool IsGenericTypeDefinition { get { return _t.TypeParameterList != null; } }

        public string Name { get { return RoslynTypeInfoProvider.GetInnerClassInsideGenericAwareName(_t, _t.Identifier.ValueText); } }

        public string FullName { get { return _t.Identifier.ToFullString().RemoveWhitespace(); } }

        public IEnumerable<ITypeInfoProvider> GetGenericArguments()
        {
            return _t.TypeParameterList == null
                ? Enumerable.Empty<ITypeInfoProvider>()
                : _t.TypeParameterList.Parameters.Select(tps => new RoslynPlainTypeInfoProvider(tps));
        }

        public override string ToString()
        {
            return _t != null ? string.Format("TypeParent: {0}\r\nType: {1}", _t.Parent, _t.ToFullString()) : "_t == null";
        }

        internal static ITypeInfoProvider ResolveProvider(TypeSyntax t)
        {
            var genericNameSyntax = t as GenericNameSyntax;
            if (genericNameSyntax != null)
            {
                return new RoslynGenericParameterTypeInfoProvider(genericNameSyntax);
            }
            return new RoslynPlainTypeInfoProvider(t);
        }

        /// <summary>
        /// See ShouldStringify_MethodWithExternalClassParam and ShouldStringify_MethodWithInnerClassParam
        /// </summary>
        internal static string GetInnerClassInsideGenericAwareName(SyntaxNode theType, string className)
        {
            var name = className;

            // check if it's class inside generic class (see GenericTestType<TClassParam>::MethodWithInnerClassParam)
            var ancestorClass = theType.Ancestors().OfType<TypeDeclarationSyntax>().FirstOrDefault();
            //bool isInsideMethod = theType.Ancestors().Any(a => a is MethodDeclarationSyntax);
            if (ancestorClass != null /*&& isInsideMethod*/ && ancestorClass.TypeParameterList != null && (ancestorClass.DescendantNodes().OfType<ClassDeclarationSyntax>().Any(t => t.Identifier.ValueText == name) || ancestorClass.DescendantNodes().OfType<DelegateDeclarationSyntax>().Any(t => t.Identifier.ValueText == name)))
            {
                name += ancestorClass.TypeParameterList.ToString();
            }

            return name.RemoveWhitespace();
        }
    }

    public class RoslynGenericParameterTypeInfoProvider : ITypeInfoProvider
    {
        private readonly GenericNameSyntax _gp;

        public RoslynGenericParameterTypeInfoProvider(GenericNameSyntax gp)
        {
            _gp = gp;
        }

        public bool IsGenericType { get { return true; } }

        public string Namespace { get { throw new System.NotImplementedException(); } }
        public bool IsGenericTypeDefinition { get { return false; } }

        public string Name { get { return _gp.Identifier.ValueText.RemoveWhitespace(); } }

        public string FullName { get { return _gp.ToFullString().RemoveWhitespace(); } }

        public IEnumerable<ITypeInfoProvider> GetGenericArguments()
        {
            return _gp.TypeArgumentList.Arguments.Select(gp => RoslynTypeInfoProvider.ResolveProvider(gp)).ToArray();
        }
    }

    public class RoslynPlainTypeInfoProvider : ITypeInfoProvider
    {
        private readonly SyntaxNode _t;

        public RoslynPlainTypeInfoProvider(SyntaxNode t)
        {
            _t = t;
        }

        public bool IsGenericType { get { return false; } }

        public string Namespace { get { throw new System.NotImplementedException(); } }
        public bool IsGenericTypeDefinition { get { return false; } }

        public string Name { get { return RoslynTypeInfoProvider.GetInnerClassInsideGenericAwareName(_t, _t.ToString()); } }

        public string FullName { get { return _t.ToFullString().RemoveWhitespace(); } }

        public IEnumerable<ITypeInfoProvider> GetGenericArguments()
        {
            return Enumerable.Empty<ITypeInfoProvider>();
        }

        public override string ToString()
        {
            return _t != null ? string.Format("TypeParent: {0}\r\nType: {1}", _t.Parent, _t.ToFullString()) : "_t == null";
        }
    }
}