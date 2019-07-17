using System.Collections.Generic;

namespace MethodLogger
{
    public interface IMethodLoggerRepository
    {
        void AddMethods(IEnumerable<MethodRow> methods);
    }
}