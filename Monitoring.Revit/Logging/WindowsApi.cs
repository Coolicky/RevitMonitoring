using System;
using System.Runtime.InteropServices;
using System.Text;

namespace Monitoring.Revit.Logging
{
    public class WindowsApi
    {
        public enum GetAncestorFlags
        {
            GetParent = 1,
            GetRoot = 2,
            GetRootOwner = 3
        }


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
        
        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr GetWindow(IntPtr hWnd, GetWindowType uCmd);
        
        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

        public static string GetWindowTitle(IntPtr pointer)
        {
            const int nChars = 256;
            var buff = new StringBuilder(nChars);

            if (GetWindowText(pointer, buff, nChars) > 0)
            {
                return buff.ToString();
            }

            return null;
        }

        public static string GetActiveWindowTitle()
        {
            var handle = GetForegroundWindow();
            var parentHandle = GetWindow(handle, GetWindowType.GW_OWNER);

            return GetWindowTitle(parentHandle);
        }
    }
}