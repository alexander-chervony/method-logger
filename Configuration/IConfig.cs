using System;

namespace Configuration
{
    public interface IConfig
    {
        event Action Changed;
        bool Enabled { get; }
        string ConnectionString { get; }
        int PersistIntervalMs { get; }
        int ApproximateLoggersCount { get; }
        int ChunkSize { get; }
    }
}