using System.Linq;
using System.Reflection;
using Stringification;

namespace Tests.ReflectionTestInfoProviders
{
    public class ReflectionMethodInfoProvider : IMethodInfoProvider
    {
        private readonly MethodBase _m;

        public ReflectionMethodInfoProvider(MethodBase m)
        {
            _m = m;
        }

        public ITypeInfoProvider DeclaringType { get { return new ReflectionTypeInfoProvider(_m.DeclaringType.IsGenericType ? _m.DeclaringType.GetGenericTypeDefinition() : _m.DeclaringType); } }
        public string Name { get { return _m.Name; } }
        public bool IsGenericMethodDefinition { get { return _m.IsGenericMethodDefinition; } }
        /// <summary>
        /// not supported
        /// </summary>
        public bool IsExternal { get { return (_m.Attributes & MethodAttributes.PinvokeImpl) > 0; } }
        public MethodType MethodType { get { return MethodType.Method; } }

        public object[] GetGenericArguments()
        {
            return _m.GetGenericArguments();
        }

        public IParameterInfoProvider[] GetParameters()
        {
            return _m.GetParameters().Select(p => new ReflectionParameterInfoProvider(p)).ToArray();
        }

        public bool IsAbstractClassOrInterfaceMember { get { return _m.DeclaringType != null && (_m.DeclaringType.IsAbstract || _m.DeclaringType.IsInterface) && !_m.DeclaringType.IsSealed; } }
        public bool IsPartialMethodWithoutBody { get { return false; } }
    }
}