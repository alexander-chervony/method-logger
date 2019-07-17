using System;
using System.Diagnostics;

namespace CodeCleaner
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var options = new Options();
            if (!CommandLine.Parser.Default.ParseArguments(args, options))
            {
                ReadIfNeeded();
                return;
            }

            var logger = new Logger();

            logger.Log(string.Format("\r\nStarting processing with params: {0}", options));

            string status = "succeeded";
            try
            {
                new Cleaner(options, logger).RunCleanup();
            }
            catch (Exception e)
            {
                logger.Log(e.ToString());
                status = "FAILED!";
            }

            logger.Log(string.Format("\r\nProcessing {0}.", status));
            ReadIfNeeded();
        }

        private static void ReadIfNeeded()
        {
            if (Debugger.IsAttached)
                Console.ReadLine();
        }
    }
}