using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Timers;
using Autodesk.Revit.UI;
using Serilog;

namespace Monitoring.Revit.Logging
{
    public class IdleTimer : IDisposable
    {
        private struct LASTINPUTINFO
        {
            public uint cbSize;
            public uint dwTime;
        }

        private readonly IntPtr _revitMainHandle;
        private readonly UIApplication _uiApp;
        private System.Timers.Timer _timer;

        private DateTime _startTime;
        private string _startingDocument;
        private bool _wasActive;

        //TODO: Get timeout from config
        private readonly double _logTimeOut = 60;
        private double _idleSeconds;
        private uint _previousLastInput;
        private DateTime _idleStart = DateTime.Now;
        private object _lock = new object();

        public IdleTimer(IntPtr revitMainHandle, UIApplication uiApp)
        {
            _revitMainHandle = revitMainHandle;
            _uiApp = uiApp;
            _timer = new System.Timers.Timer();
            _timer.Elapsed += TimerElapsed;
            //TODO: Get frequency from config
            _timer.Interval = 1000 * 10;
        }

        public void ChangeDocument()
        {
            LogActivity();
        }

        private void Reset()
        {
            _startTime = DateTime.Now;
            _startingDocument = _uiApp?.ActiveUIDocument?.Document?.PathName;
        }

        public void StartIdleTimer()
        {
            _timer.Enabled = true;
            _startTime = DateTime.Now;
            if (string.IsNullOrEmpty(_startingDocument))
                _startingDocument = _uiApp.ActiveUIDocument.Document.PathName;
            _wasActive = true;
            _timer.Start();
        }

        public void StopIdleTimer()
        {
            _timer.Stop();
            LogActivity();
        }

        private void TimerElapsed(object sender, ElapsedEventArgs e)
        {
            lock (_lock)
            {
                var lastInput = GetLastInputInfoValue();

                if (lastInput == _previousLastInput)
                {
                    _idleSeconds = (DateTime.Now - _idleStart).TotalSeconds;
                }
                else
                {
                    _idleSeconds = 0;
                    _idleStart = DateTime.Now;
                }

                _previousLastInput = lastInput;
            }

            if (_idleSeconds > _logTimeOut)
            {
                if (_wasActive) LogActivity();
                _wasActive = false;
                return;
            }

            var revitWindowTitle = WindowsApi.GetWindowTitle(_revitMainHandle);
            var activeWindowTitle = WindowsApi.GetActiveWindowTitle();
            var isRevitWindowActive = revitWindowTitle == activeWindowTitle;
            if (!isRevitWindowActive)
            {
                if (_wasActive) LogActivity();
                _wasActive = false;
            }
            else
            {
                _wasActive = true;
            }
        }

        private void LogActivity()
        {
            var data = new Dictionary<string, object>
            {
                { "Document", _startingDocument },
                { "StartTime", _startTime },
                { "EndTime", DateTime.Now },
                { "WorkTime", (DateTime.Now - _startTime).TotalMinutes },
            };
            Log.Information("Document Work {Data}", data);
            Reset();
        }

        [DllImport("user32.dll")]
        private static extern bool GetLastInputInfo(ref LASTINPUTINFO plii);

        private static uint GetLastInputInfoValue()
        {
            var lastInPut = new LASTINPUTINFO();
            lastInPut.cbSize = (uint)Marshal.SizeOf(lastInPut);
            GetLastInputInfo(ref lastInPut);
            return lastInPut.dwTime;
        }

        public void Dispose()
        {
            if (_wasActive)
            {
                LogActivity();
            }
            _timer?.Stop();
            _timer?.Dispose();
        }
    }
}