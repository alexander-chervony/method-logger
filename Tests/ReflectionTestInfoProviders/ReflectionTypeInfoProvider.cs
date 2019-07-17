using System;
using System.Collections.Generic;
using System.Linq;
using Stringification;

namespace Tests.ReflectionTestInfoProviders
{
    public class ReflectionTypeInfoProvider : ITypeInfoProvider
    {
        private readonly Type _t;

        public ReflectionTypeInfoProvider(Type t)
        {
            _t = t;
        }

        public bool IsGenericType { get { return _t.IsGenericType || _t.GetGenericArguments().Length > 0; } }

        public string Namespace { get { return _t.Namespace; } }

        public bool IsGenericTypeDefinition { get { return _t.IsGenericTypeDefinition; } }

        public string Name { get { return _t.Name.TrimEnd('&'); } }

        public string FullName { get { return _t.FullName; } }

        public IEnumerable<ITypeInfoProvider> GetGenericArguments()
        {
            return _t.GetGenericArguments().Select(a => new ReflectionTypeInfoProvider(a));
        }
    }
}