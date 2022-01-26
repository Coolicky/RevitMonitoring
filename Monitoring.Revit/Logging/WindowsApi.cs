using System;
using System.Runtime.InteropServices;
using System.Text;

namespace Monitoring.Revit.Logging
{
    public static class WindowsApi
    {
        private enum GetWindowType : uint
        {
            GW_HWNDFIRST = 0,
            GW_HWNDLAST = 1,
            GW_HWNDNEXT = 2,
            GW_HWNDPREV = 3,
            GW_OWNER = 4,
            GW_CHILD = 5,
            GW_ENABLEDPOPUP = 6
        }

        private struct LASTINPUTINFO
        {
            public uint cbSize;
            public uint dwTime;
        }

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr GetWindow(IntPtr hWnd, GetWindowType uCmd);

        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

        [DllImport("user32.dll")]
        private static extern bool GetLastInputInfo(ref LASTINPUTINFO plii);

        public static string GetWindowTitle(IntPtr handle)
        {
            const int nChars = 256;
            var buff = new StringBuilder(nChars);

            return GetWindowText(handle, buff, nChars) > 0 ? buff.ToString() : null;
        }

        public static string GetActiveWindowTitle()
        {
            var handle = GetForegroundWindow();
            var parentHandle = GetWindow(handle, GetWindowType.GW_OWNER);

            return parentHandle == IntPtr.Zero ? GetWindowTitle(handle) : GetWindowTitle(parentHandle);
        }

        public static uint GetLastInputInfoValue()
        {
            var lastInPut = new LASTINPUTINFO();
            lastInPut.cbSize = (uint)Marshal.SizeOf(lastInPut);
            GetLastInputInfo(ref lastInPut);
            return lastInPut.dwTime;
        }
    }
}