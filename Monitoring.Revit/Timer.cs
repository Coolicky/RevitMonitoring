using System.Collections.Generic;
using System.Diagnostics;
using Serilog;

namespace Monitoring.Revit
{

    public interface ITimer
    {
        Stopwatch Stopwatch { get; set; }
        void AddArgs(string key, object value);
        void Start();
        void Stop();
        void LogTime();
    }
    public class Timer : ITimer
    {
        private Dictionary<string, object> _args;

        public Timer(Dictionary<string, object> args)
        {
            _args = args;
            Stopwatch = new Stopwatch();
        }
        public Stopwatch Stopwatch { get; set; }
        public void AddArgs(string key, object value)
        {
            if (_args == null)
                _args = new Dictionary<string, object>();

            _args.Add(key, value);
        }

        public void Start()
        {
            Stopwatch.Reset();
            Stopwatch.Start();
        }

        public void Stop()
        {
            Stopwatch.Stop();
            _args.Add("Opening Time", Stopwatch.ElapsedMilliseconds/1000);
            LogTime();
        }

        public void LogTime()
        {
            Log.Information("Revit Timer {Data}", _args);
        }
    }
}