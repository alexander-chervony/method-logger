using System.Collections.Generic;

namespace Stringification
{
    public interface ITypeInfoProvider
    {
        bool IsGenericType { get; }
        IEnumerable<ITypeInfoProvider> GetGenericArguments();
        string Name { get; }
        string FullName { get; }
        string Namespace { get; }
        bool IsGenericTypeDefinition { get; }
    }
}