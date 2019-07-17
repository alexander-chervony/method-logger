using System.Linq;
using Roslyn.Compilers.CSharp;
using Stringification;

namespace CodeCleaner
{
    public class RoslynParameterInfoProvider : IParameterInfoProvider
    {
        private readonly ParameterSyntax _p;

        public RoslynParameterInfoProvider(ParameterSyntax p)
        {
            _p = p;
        }

        public ITypeInfoProvider ParameterType { get { return RoslynTypeInfoProvider.ResolveProvider(_p.Type); } }
        public bool IsParams { get { return _p.Modifiers.Any(SyntaxKind.ParamsKeyword); } }
        public bool IsOut { get { return _p.DescendantNodesAndTokens().Any(x => x.Kind == SyntaxKind.OutKeyword); } }
        public bool IsRef { get { return _p.DescendantNodesAndTokens().Any(x => x.Kind == SyntaxKind.RefKeyword); } }
        /// <summary>
        /// dynamic type correctly determined since roslyn analyses code text, so no need special handling
        /// </summary>
        public bool IsDynamic { get { return false; } }
        //public bool IsArray { get { return _p.DescendantNodesAndTokens().Any(x => x.Kind == SyntaxKind.array); } }
        public bool IsArray { get { return false; } }
        public string Name { get { return _p.Identifier.ValueText; } }

        public override string ToString()
        {
            return _p.ToFullString();
        }
    }
}