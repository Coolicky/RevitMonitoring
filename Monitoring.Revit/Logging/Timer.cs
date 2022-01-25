using System.Collections.Generic;
using System.Diagnostics;
using Serilog;

namespace Monitoring.Revit.Logging
{
    public class Timer : ITimer
    {
        private readonly string _operationName;
        private Dictionary<string, object> _args;

        public Timer(string operationName)
        {
            _operationName = operationName;
            _args = new Dictionary<string, object>();
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
            AddArgs("Time Elapsed (seconds)", Stopwatch.ElapsedMilliseconds/1000);
            LogTime();
            
            Stopwatch.Reset();
            _args.Clear();
        }

        public void LogTime()
        {
            Log.Information("{OperationName}, {Data}", _operationName, _args);
        }
    }
}