using System.Diagnostics;

namespace Monitoring.Revit.Interfaces
{
    public interface IEventTimer
    {
        Stopwatch Stopwatch { get; set; }
        void AddArgs(string key, object value);
        void Start();
        void Stop();
        void LogTime();
    }
}