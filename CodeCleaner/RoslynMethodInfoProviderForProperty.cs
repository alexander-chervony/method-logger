using Roslyn.Compilers.CSharp;
using Stringification;

namespace CodeCleaner
{
    public class RoslynMethodInfoProviderForProperty : RoslynMethodInfoProviderBase, IMethodInfoProvider
    {
        private readonly PropertyDeclarationSyntax _p;

        public RoslynMethodInfoProviderForProperty(PropertyDeclarationSyntax p, MethodType methodType)
            : base(p)
        {
            _p = p;
            MethodType = methodType;
        }

        public override IParameterInfoProvider[] GetParameters()
        {
            return new[] { new RoslynParameterInfoProviderForProperty(_p.Type) };
        }

        public MethodType MethodType { get; private set; }

        protected override string IdentifierText { get { return (MethodType == MethodType.PropertyGetter ? "get_" : "set_") + _p.Identifier.ValueText; } }
    }
}