using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace osu_notsodirect_overlay.Helpers
{
    public static class WindowHelper
    {
        private const int GWL_EXSTYLE = -20;
        private const int WS_EX_TRANSPARENT = 0x00000020;

        [DllImport("user32.dll")]
        private static extern int GetWindowLong(IntPtr hwnd, int index);

        [DllImport("user32.dll")]
        private static extern int SetWindowLong(IntPtr hwnd, int index, int newStyle);

        public static void MakeWindowClickThrough(Window window)
        {
            var hwnd = new WindowInteropHelper(window).Handle;
            var extendedStyle = GetWindowLong(hwnd, GWL_EXSTYLE);
            SetWindowLong(hwnd, GWL_EXSTYLE, extendedStyle | WS_EX_TRANSPARENT);
        }

        public static void MakeWindowClickable(Window window)
        {
            var hwnd = new WindowInteropHelper(window).Handle;
            var extendedStyle = GetWindowLong(hwnd, GWL_EXSTYLE);
            SetWindowLong(hwnd, GWL_EXSTYLE, extendedStyle & ~WS_EX_TRANSPARENT);
        }

        public static void ToggleWindowClickThrough(Window window, bool makeClickThrough)
        {
            if (makeClickThrough)
                MakeWindowClickThrough(window);
            else
                MakeWindowClickable(window);
        }
    }
}