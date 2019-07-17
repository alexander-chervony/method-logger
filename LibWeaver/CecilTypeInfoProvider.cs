using System.Collections.Generic;
using System.Linq;
using Mono.Cecil;
using Stringification;

namespace LibWeaver
{
    public class CecilTypeInfoProvider : ITypeInfoProvider
    {
        private readonly TypeDefinition _t;

        public CecilTypeInfoProvider(TypeDefinition t)
        {
            _t = t;
        }

        public bool IsGenericType { get { return _t.HasGenericParameters; } }

        public string Namespace { get { return !string.IsNullOrEmpty(_t.Namespace) ? _t.Namespace : _t.DeclaringType != null ? _t.DeclaringType.Namespace : string.Empty; } }

        public bool IsGenericTypeDefinition { get { return _t.HasGenericParameters; } }

        public string Name { get { return _t.Name; } }

        public string FullName { get { return _t.FullName; } }

        public IEnumerable<ITypeInfoProvider> GetGenericArguments()
        {
            return _t.GenericParameters.Select(gp => new CecilGenericParameterTypeInfoProvider(gp)).ToArray();
        }

        internal static ITypeInfoProvider ResolveTypeReference(TypeReference tr)
        {
            if (tr.IsGenericInstance || (tr is TypeSpecification && ((TypeSpecification)(tr)).ElementType is GenericInstanceType))
            {
                var subjectType = tr.IsGenericInstance 
                    ? (GenericInstanceType)tr
                    : (GenericInstanceType)(((TypeSpecification)(tr)).ElementType);

                return new CecilGenericInstanceTypeInfoProvider(subjectType);
            }
            TypeDefinition td = tr.Resolve();
            if (td == null || !tr.IsGenericParameter)
            {
                return new CecilPlainTypeInfoProvider(tr);
            }
            return new CecilTypeInfoProvider(td);
        }
    }

    public class CecilGenericInstanceTypeInfoProvider : ITypeInfoProvider
    {
        private readonly GenericInstanceType _t;

        public CecilGenericInstanceTypeInfoProvider(GenericInstanceType t)
        {
            _t = t;
        }

        public bool IsGenericType { get { return true; } }

        public string Namespace { get { throw new System.NotImplementedException(); } }
        public bool IsGenericTypeDefinition { get { return false; } }

        public string Name { get { return _t.Name; } }

        public string FullName { get { return _t.FullName; } }

        public IEnumerable<ITypeInfoProvider> GetGenericArguments()
        {
            return _t.GenericArguments.Select(a => CecilTypeInfoProvider.ResolveTypeReference(a)).ToArray();
        }
    }

    public class CecilGenericParameterTypeInfoProvider : ITypeInfoProvider
    {
        private readonly GenericParameter _gp;

        public CecilGenericParameterTypeInfoProvider(GenericParameter gp)
        {
            _gp = gp;
        }

        public bool IsGenericType { get { return _gp.HasGenericParameters; } }

        public string Namespace { get { throw new System.NotImplementedException(); } }
        public bool IsGenericTypeDefinition { get { return _gp.HasGenericParameters; } }

        public string Name { get { return _gp.Name; } }

        public string FullName { get { return _gp.FullName; } }

        public IEnumerable<ITypeInfoProvider> GetGenericArguments()
        {
            return _gp.GenericParameters.Select(gp => new CecilGenericParameterTypeInfoProvider(gp)).ToArray();
        }
    }

    public class CecilPlainTypeInfoProvider : ITypeInfoProvider
    {
        private readonly TypeReference _t;

        public CecilPlainTypeInfoProvider(TypeReference t)
        {
            _t = t;
        }

        public bool IsGenericType { get { return false; } }

        public string Namespace { get { throw new System.NotImplementedException(); } }
        public bool IsGenericTypeDefinition { get { return false; } }

        public string Name { get { return _t.Name.TrimEnd('&'); } }

        public string FullName { get { return _t.FullName; } }

        public IEnumerable<ITypeInfoProvider> GetGenericArguments()
        {
            return Enumerable.Empty<ITypeInfoProvider>();
        }
    }
}