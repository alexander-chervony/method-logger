using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using Stringification;

namespace Tests.ReflectionTestInfoProviders
{
    public class ReflectionParameterInfoProvider : IParameterInfoProvider
    {
        private readonly ParameterInfo _p;
        private static readonly Type ParamAttrType = typeof(ParamArrayAttribute);
        private static readonly Type DynamicAttrType = typeof(DynamicAttribute);

        public ReflectionParameterInfoProvider(ParameterInfo p)
        {
            _p = p;
        }

        public ITypeInfoProvider ParameterType { get { return new ReflectionTypeInfoProvider(_p.ParameterType); } }
        public bool IsParams { get { return _p.GetCustomAttributes(ParamAttrType, false).Length > 0; } }
        public bool IsOut { get { return _p.IsOut; } }
        public bool IsRef { get { return _p.ParameterType.ToString().EndsWith("&") && !IsOut; } }
        public bool IsDynamic { get { return _p.GetCustomAttributes(DynamicAttrType, false).Length > 0; } }
        public bool IsArray { get { return _p.ParameterType.IsArray; } }
        public string Name { get { return _p.Name; } }
    }
}