using System.Diagnostics;

namespace Monitoring.Revit.Logging
{
    public interface ITimer
    {
        Stopwatch Stopwatch { get; set; }
        void AddArgs(string key, object value);
        void Start();
        void Stop();
        void LogTime();
    }
}