using System.Linq;
using Roslyn.Compilers.CSharp;
using Stringification;

namespace CodeCleaner
{
    public abstract class RoslynMethodInfoProviderBase
    {
        private readonly SyntaxNode _p;

        protected RoslynMethodInfoProviderBase(SyntaxNode p)
        {
            _p = p;
        }

        public ITypeInfoProvider DeclaringType { get { return new RoslynTypeInfoProvider((TypeDeclarationSyntax)_p.Parent); } }

        protected abstract string IdentifierText { get; }

        public string Name
        {
            get
            {
                string methName = IdentifierText;

                // add explicit interface name to the method name
                var exInterface = _p.ChildNodesAndTokens().FirstOrDefault(n => n.Kind == SyntaxKind.ExplicitInterfaceSpecifier);
                if (exInterface != null)
                {
                    methName = exInterface + methName;
                }

                return methName;
            }
        }

        public virtual bool IsGenericMethodDefinition { get { return false; } }

        public virtual object[] GetGenericArguments()
        {
            return new object[0];
        }

        public virtual IParameterInfoProvider[] GetParameters()
        {
            return Enumerable.Empty<IParameterInfoProvider>().ToArray();
        }

        public bool IsAbstractClassOrInterfaceMember { get { return _p.Parent.ChildNodesAndTokens().Any(n => n.Kind == SyntaxKind.AbstractKeyword || n.Kind == SyntaxKind.InterfaceKeyword); } }
        public virtual bool IsPartialMethodWithoutBody { get { return false; } }
        public bool IsExternal { get { return _p.ChildNodesAndTokens().Any(n => n.Kind == SyntaxKind.ExternKeyword); } }

        public override string ToString()
        {
            return _p != null ? string.Format("MethodParent: {0}\r\nMethod: {1}", ((ClassDeclarationSyntax)_p.Parent).Identifier.ValueText, _p.ToFullString()) : "_m == null";
        }
    }
}