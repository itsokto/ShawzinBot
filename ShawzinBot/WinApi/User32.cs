using System;
using System.Runtime.InteropServices;

namespace ShawzinBot.WinApi
{
    public static class User32
    {
        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        public static extern IntPtr FindWindow(string className, string windowTitle);

        public static IntPtr FindWindow(string lpWindowName)
        {
            return FindWindow(null, lpWindowName);
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
        public static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        public static extern bool SetForegroundWindow(IntPtr hWnd);
        
        private static bool IsWindowFocused(IntPtr windowPtr)
        {
            var hWnd = User32.GetForegroundWindow();
            return !windowPtr.Equals(IntPtr.Zero) && hWnd.Equals(windowPtr);
        }

        public static bool IsWindowFocused(string windowName)
        {
            var windowPtr = User32.FindWindow(windowName);
            return IsWindowFocused(windowPtr);
        }
    }
}