using System.Linq;
using Mono.Cecil;
using Stringification;

namespace LibWeaver
{
    public class CecilParameterInfoProvider : IParameterInfoProvider
    {
        private readonly ParameterDefinition _p;

        public CecilParameterInfoProvider(ParameterDefinition p)
        {
            _p = p;
        }

        public ITypeInfoProvider ParameterType { get { return CecilTypeInfoProvider.ResolveTypeReference(_p.ParameterType); } }
        public bool IsParams { get { return _p.HasCustomAttributes && _p.CustomAttributes.Any(a => a.AttributeType.Name == "ParamArrayAttribute" && a.AttributeType.Namespace == "System"); } }
        public bool IsOut { get { return _p.IsOut; } }
        public bool IsRef { get { return _p.ParameterType.IsByReference && !IsOut; } }
        public bool IsDynamic { get { return _p.HasCustomAttributes && _p.CustomAttributes.Any(a => a.AttributeType.Name == "DynamicAttribute"); } }
        public bool IsArray { get { return _p.ParameterType.IsArray; } }
        public string Name { get { return _p.Name; } }
    }
}