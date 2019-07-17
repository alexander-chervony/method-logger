using System.Collections.Generic;

namespace Stringification
{
    public interface IMethodLister
    {
        IEnumerable<IMethodInfoProvider> ListAllMethods();
    }
}