using System.Linq;
using Roslyn.Compilers.CSharp;
using Stringification;

namespace CodeCleaner
{
    public class RoslynMethodInfoProvider : RoslynMethodInfoProviderBase, IMethodInfoProvider
    {
        private readonly MethodDeclarationSyntax _p;

        public RoslynMethodInfoProvider(MethodDeclarationSyntax p) : base(p)
        {
            _p = p;
        }

        protected override string IdentifierText { get { return _p.Identifier.ValueText; } }

        public override bool IsGenericMethodDefinition { get { return _p.TypeParameterList != null; } }

        public override object[] GetGenericArguments()
        {
            return _p.TypeParameterList == null
                ? new object[0]
                : _p.TypeParameterList.Parameters.OfType<object>().ToArray();
        }

        public override IParameterInfoProvider[] GetParameters()
        {
            return _p.ParameterList.Parameters.Select(p => (IParameterInfoProvider)new RoslynParameterInfoProvider(p)).ToArray();
        }

        public override bool IsPartialMethodWithoutBody { get { return _p.ChildNodesAndTokens().Any(n => n.Kind == SyntaxKind.PartialKeyword) && _p.Body == null; } }
        public MethodType MethodType { get { return MethodType.Method; } }

        public override string ToString()
        {
            return _p != null ? string.Format("MethodParent: {0}\r\nMethod: {1}", ((ClassDeclarationSyntax)_p.Parent).Identifier.ValueText, _p.ToFullString()) : "_m == null";
        }
    }
}