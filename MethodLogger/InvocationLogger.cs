using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using Configuration;

namespace MethodLogger
{
    public class InvocationLogger : IDisposable
    {
        private readonly IErrorLogger _errorLogger;
        private readonly IConfig _config;
        private readonly ConcurrentQueue<string> _newMethods = new ConcurrentQueue<string>();
        private readonly IMethodLoggerRepository _repository;
        private readonly Timer _persistTimer;
        private readonly object _persistLocker = new object();
        // our project has ~ 46000 weaved methods in March 2014.
        private const int MaxMethodCount = 70000;
        private readonly bool[] _enteredMethods = new bool[MaxMethodCount];

        public InvocationLogger(IErrorLogger errorLogger, IConfig config, IMethodLoggerRepository repository)
        {
            _repository = repository;
            _errorLogger = errorLogger;
            _config = config;
            int startShiftMs = new Random().Next(0, _config.PersistIntervalMs * _config.ApproximateLoggersCount);
            _persistTimer = new Timer(LogNewMethodsIfAny, null, startShiftMs, _config.PersistIntervalMs);
            _config.Changed += () => _persistTimer.Change(startShiftMs, _config.PersistIntervalMs);
        }

        ~InvocationLogger()
        {
            Dispose();
        }

        public void Log(string methodName, int methodIndex)
        {
            if (!_config.Enabled)
                return;

            if (!_enteredMethods[methodIndex])
            {
                _newMethods.Enqueue(methodName);
                _enteredMethods[methodIndex] = true;
            }
        }

        public void Dispose()
        {
            _persistTimer.Dispose();
        }

        private void LogNewMethodsIfAny(object state)
        {
            lock (_persistLocker)
            {
                IEnumerable<MethodRow> rows = null;
                var methods = new List<string>(_config.ChunkSize);
                try
                {
                    if (!_config.Enabled || _newMethods.IsEmpty)
                        return;

                    int cnt = Math.Min(_newMethods.Count, _config.ChunkSize);

                    for (int i = 0; i < cnt; i++)
                    {
                        // doesn't really matter if the chunk will be less than cnt
                        string method;
                        if (_newMethods.TryDequeue(out method))
                        {
                            methods.Add(method);
                        }
                    }

                    if (methods.Count > 0)
                    {
                        rows = ComposeRows(methods.Distinct()).ToArray();
                        _repository.AddMethods(rows);

                        if (AllNewMethodsLogged != null && _newMethods.Count == 0)
                        {
                            AllNewMethodsLogged();
                        }
                    }
                }
                catch (Exception e)
                {
                    // Put back all the non-saved methods into the queue. Let's try to save them next time. This integration test proves that it's possible: IntegrationTests.EnsureAllLoggableWeavedMethodsCanBeSavedIntoDbSuccessfully
                    foreach (var method in methods)
                    {
                        _newMethods.Enqueue(method);
                    }
                    // _errorLogger.Error(string.Format("Method Logger Exception:\r\n\r\n{0}", rows == null ? null : string.Join("\r\n", rows.Select(r => r.ToString()))), e);
                    _errorLogger.Error("Method Logger Exception:\r\n\r\n", e);
                }
            }
        }

        public static IEnumerable<MethodRow> ComposeRows(IEnumerable<string> methods)
        {
            AppInfo info = GetAppInfo(); 
            return methods.Select(
                m => new MethodRow
                {
                    Method = m,
                    ExecutingApp = info.ExecutingApp,
                    Machine = info.Machine,
                    Pid = info.Pid,
                    InsertedOn = info.InsertedOn
                });
        }

        private static AppInfo GetAppInfo()
        {
            var assembly = Assembly.GetEntryAssembly();
            string fullName = assembly != null ? assembly.Location : Process.GetCurrentProcess().MainModule.FileName;
            string name = Path.GetFileNameWithoutExtension(fullName) + " " + GetValuableArg();
            return new AppInfo { ExecutingApp = name, Machine = Environment.MachineName, Pid = Process.GetCurrentProcess().Id, InsertedOn = DateTime.Now };
        }

        private static string GetValuableArg()
        {
            var args = Environment.GetCommandLineArgs();
            // for win service
            if (args.Length == 2)
                return args[1];
            // for web apps
            if (args.Length >= 3)
            {
                var appArr = args.SkipWhile(a => !"-ap".Equals(a)).ToArray();
                if (appArr.Length >= 2)
                {
                    return appArr[1];
                }
            }
            // unknown cases
            return string.Join(" ", args);
        }

        /// <summary>
        /// Just for unit tests.
        /// </summary>
        internal event Action AllNewMethodsLogged;
    }

    public class AppInfo
    {
        public string ExecutingApp { get; set; }
        public string Machine { get; set; }
        public int Pid { get; set; }
        public DateTime InsertedOn { get; set; }
    }
}