using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace LibWeaver
{
    /// <summary>
    /// Note that calling twice the weaving for the same assembly is a dirty way - just for fast weaving and studying the code. 
    /// It relies on CleanPreviousWeaving which is not allways works at it should - for example in case of WeaverNotifier type rename, 
    /// can have bugs in implementation, assumes that WeavedNotifier is not used/added anywhere in the code manually.
    /// For production code it's better to weave only once - after CLEAN solution build (REBUILD ALL).
    /// todo: change logging to DB-based - table LoggerLog (app, time, string) instead of file/event, 
    /// todo: log changed cofig; log heartbeat in LogNewMethodsIfAny; log insert time in LoggedMethods
    /// todo: for logging use 5minutes based TimeLogFilter
    /// </summary>
    public class Program
    {
        private static readonly string _logFileName = GetLogFileName();

        public static IEnumerable<string> GetLibsToWeave(
            string entryDir,
            IEnumerable<string> additionalExcludeFileContains = null)
        {
            additionalExcludeFileContains = additionalExcludeFileContains ?? new string[0];

            return new LibFinder().GetLibs(
                entryDir,
                new[] { "Rtx", "Authentication.", "TestLib", "CaptchaService", "CaptchaSite", "CommonEmulation", "SamApiWrapper", "Activity.Service", "WidgetsSite" },
                new[] { ".dll", ".exe" },
                new[] { ".svn", "_svn", ".git", "obj"},
                new[] { ".Test", ".Tests", ".Tests.UI", ".Tests.Integration", "TestUtils", "RtxResourceCodeGenerator" },
                new[] { "MethodLogger.", "LibWeaver.", "CodeCleaner.", "Stringification.", "TestLib.", "Rtx.Core.Log.", "Rtx.Core.FtpExtensions.Interfaces.", "Rtx.Core.FCKeditor.", "ActivitySyncConsole.", "Activity.CommentCountersUpdater", "Activity.PeriodCountersUpdater", ".vshost.exe" }.Concat(additionalExcludeFileContains).ToArray());
        }

        private static void Main(string[] args)
        {
            string entryDir = args.FirstOrDefault();
            if (entryDir == null || !Directory.Exists(entryDir))
            {
                Log(@"Please, provide valid entry directory path as the argument. For example: MethodLogger.exe ""c:\\temp\\MethodLoggerInput\\""");
                Console.ReadLine();
                return;
            }

            var additionalFileMasksToExclude = args.Length == 2 ? args[1].Split(',') : new string[0];

            string status = "succeeded";
            // try
            // {
            IEnumerable<string> libs = GetLibsToWeave(entryDir, additionalFileMasksToExclude);
            
            var weaver = new LibWeaver();
            int weavedMethodsNum = weaver.WeaveList(libs, Notify.Enter, Log);

            // }
            // catch (Exception e)
            // {
            // Log(e.ToString());
            // status = "FAILED!";
            // }
            Log(string.Format("\r\nWeaving {0}. Weaved {1} methods. Warning!! Make sure the mumber less than WeavedNotifier.MaxMethodCount!", status, weavedMethodsNum));
            if (Debugger.IsAttached)
                Console.ReadLine();
        }

        private static void Log(string line)
        {
            File.AppendAllLines(_logFileName, new[] { line });
            Console.WriteLine(line);
        }

        private static string GetLogFileName()
        {
            string name = "Log000.txt";
            string[] logs = Directory.GetFiles(Environment.CurrentDirectory, "Log*.txt");
            if (logs.Length > 0)
            {
                string last = logs[logs.Length - 1].Substring(logs[logs.Length - 1].LastIndexOf('\\') + 1);
                int lastNum = last.EndsWith("Log000.txt") ? 0 : int.Parse(last.Substring(3, 3).Trim('0'));
                name = "Log" + (++lastNum).ToString("000") + ".txt";
            }

            return Path.Combine(Environment.CurrentDirectory, name);
        }
    }
}