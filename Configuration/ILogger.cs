namespace Configuration
{
    public interface ILogger
    {
        void Log(string line, bool separateLine = true, bool displayTime = true);
    }
}