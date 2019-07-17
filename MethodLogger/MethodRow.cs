using System;

namespace MethodLogger
{
    public struct MethodRow
    {
        public string Method { get; set; }
        public string ExecutingApp { get; set; }
        public string Machine { get; set; }
        public int Pid { get; set; }
        public DateTime InsertedOn { get; set; }

        public override string ToString()
        {
            return string.Join("; ", new[] { Method, ExecutingApp, Machine, Pid.ToString(), InsertedOn.ToString() });
        }
    }
}