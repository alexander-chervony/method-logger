namespace Stringification
{
    public interface IParameterInfoProvider
    {
        string Name { get; }

        ITypeInfoProvider ParameterType { get; }

        /// <summary>
        /// void ParamsArgMethod(int i, params string[] pars)
        /// 
        /// IsParams must be true for the second parameter
        /// </summary>
        bool IsParams { get; }

        /// <summary>
        /// MethodWithOutParam(string value, out byte result)
        /// 
        /// IsOut must be true for the second parameter
        /// </summary>
        bool IsOut { get; }

        /// <summary>
        /// MethodWithRefParam(ref byte result)
        /// </summary>
        bool IsRef { get; }

        /// <summary>
        /// MethodWithDynamicParam(dynamic result)
        /// </summary>
        bool IsDynamic { get; }

        bool IsArray { get; }
    }
}