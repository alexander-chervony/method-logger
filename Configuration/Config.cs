using System;
using System.IO;

namespace Configuration
{
    public class Config : IDisposable, IConfig
    {
        private const string ConfigPath = @"c:\temp\_methodLoggerConfig.xml";

        private readonly IErrorLogger _errorLogger;
        private FileSystemWatcher _watcher;

        public Config(IErrorLogger errorLogger)
        {
            _errorLogger = errorLogger;
            InitAndWatch(ConfigPath);
        }

        ~Config()
        {
            Dispose();
        }

        public event Action Changed;
        public bool Enabled { get; private set; }
        public string ConnectionString { get; private set; }
        public int PersistIntervalMs { get; internal set; }
        public int ApproximateLoggersCount { get; internal set; }
        public int ChunkSize { get; internal set; }

        public void Dispose()
        {
            if (_watcher != null)
            {
                _watcher.Dispose();
            }
        }

        private void InitAndWatch(string file)
        {
            if (string.IsNullOrEmpty(file) || !File.Exists(file))
                throw new FileNotFoundException("Config file not found!!!", file);

            UpdateValues(file);

            _watcher = new FileSystemWatcher(Path.GetDirectoryName(file), Path.GetFileName(file))
            {
                NotifyFilter = NotifyFilters.Size | NotifyFilters.LastWrite
            };
            _watcher.Changed += UpdateValues;
            _watcher.EnableRaisingEvents = true;
        }

        private void UpdateValues(object sender, FileSystemEventArgs e)
        {
            UpdateValues(e.FullPath);

            var changedHandler = Changed;
            if (changedHandler != null)
            {
                changedHandler();
            }
        }

        private void UpdateValues(string filePath)
        {
            try
            {
                // todo: replace dynamic with simple XElement based parsing
                dynamic config = new DynamicXmlParser(filePath);
                Enabled = bool.Parse(config.Enabled.ToString().ToLower());
                ConnectionString = config.ConnectionString.ToString();
                PersistIntervalMs = int.Parse(config.PersistIntervalSec.ToString()) * 1000;
                ApproximateLoggersCount = int.Parse(config.ApproximateLoggersCount.ToString());
                ChunkSize = int.Parse(config.ChunkSize.ToString());
            }
            catch (Exception exc)
            {
                _errorLogger.Error("Config file parse or load error\r\n", exc);
            }
        }
    }
}