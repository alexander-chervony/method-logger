using System;

namespace Configuration
{
    public interface IErrorLogger
    {
        void Error(string err, Exception exc);
    }
}