using System;
using System.IO;
using System.Linq;
using Configuration;

namespace CodeCleaner
{
    public class Logger : IErrorLogger, ILogger
    {
        private static readonly string _logFileName = GetLogFileName();

        public void Log(string line, bool separateLine = true, bool displayTime = true)
        {
            if (displayTime)
                line = AddDateTime(line);

            File.AppendAllLines(_logFileName, new[] { line });

            if (separateLine)
                Console.WriteLine(line);
            else
                Console.Write(line);
        }

        public void Error(string err, Exception exc)
        {
            string msg = err + exc;
            Console.WriteLine(msg);
        }

        private static string AddDateTime(string line)
        {
            string trimmed = line.TrimStart('\r', '\n');
            string startChars = line.Substring(0, line.Length - trimmed.Length);
            line = startChars + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "\t" + trimmed;
            return line;
        }

        private static string GetLogFileName()
        {
            string name = "Log000.txt";
            string logDir = Environment.CurrentDirectory + "\\Logs";
            if (!Directory.Exists(logDir))
            {
                Directory.CreateDirectory(logDir);
            }
            string[] logs = Directory.GetFiles(logDir, "Log*.txt").OrderBy(f => f).ToArray();
            if (logs.Length > 0)
            {
                string last = logs[logs.Length - 1].Substring(logs[logs.Length - 1].LastIndexOf('\\') + 1);
                int lastNum = last.EndsWith("Log000.txt") ? 0 : int.Parse(last.Substring(3, 3).TrimStart('0'));
                name = "Log" + (++lastNum).ToString("000") + ".txt";
            }

            return Path.Combine(logDir, name);
        }
    }
}