using Roslyn.Compilers.CSharp;
using Stringification;

namespace CodeCleaner
{
    public class RoslynParameterInfoProviderForProperty : IParameterInfoProvider
    {
        private readonly TypeSyntax _t;

        public RoslynParameterInfoProviderForProperty(TypeSyntax t)
        {
            _t = t;
        }

        public ITypeInfoProvider ParameterType { get { return RoslynTypeInfoProvider.ResolveProvider(_t); } }
        public string Name { get { return "value"; } }
        public bool IsParams { get { return false; } }
        public bool IsOut { get { return false; } }
        public bool IsRef { get { return false; } }
        public bool IsDynamic { get { return false; } }
        public bool IsArray { get { return false; } }

        public override string ToString()
        {
            return _t.ToFullString() + " value";
        }
    }
}