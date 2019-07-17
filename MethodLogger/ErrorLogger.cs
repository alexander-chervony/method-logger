using System;
using System.Diagnostics;
using Configuration;

namespace MethodLogger
{
    public class ErrorLogger : IErrorLogger
    {
        /// <summary>
        /// It's not considered to be used for massive error logging.
        /// For more frequent error logging some kind of bufffering must be used.
        /// </summary>
        /// <param name="err"></param>
        /// <param name="exc"></param>
        public void Error(string err, Exception exc)
        {
            try
            {
                string msg = err + exc;
                Console.WriteLine(msg);
                EventLog.WriteEntry("Application", msg, EventLogEntryType.Error);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
    }
}