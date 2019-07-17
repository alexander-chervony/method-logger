namespace Stringification
{
    public interface IMethodInfoProvider
    {
        ITypeInfoProvider DeclaringType { get; }
        string Name { get; }
        bool IsGenericMethodDefinition { get; }
        object[] GetGenericArguments();
        IParameterInfoProvider[] GetParameters();
        bool IsAbstractClassOrInterfaceMember { get; }
        bool IsPartialMethodWithoutBody { get; }
        bool IsExternal { get; }
        MethodType MethodType { get; }
    }

    public enum MethodType
    {
        Method,
        PropertyGetter,
        PropertySetter
    }
}