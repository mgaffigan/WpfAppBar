using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Itp.WpfAppBar
{
    internal static class NativeMethods
    {
        private const string 
            User32 = "User32.dll",
            Shell32 = "Shell32.dll",
            ShCore = "ShCore.dll",
            Dwmapi = "Dwmapi.dll";

        [StructLayout(LayoutKind.Sequential)]
        public struct APPBARDATA
        {
            public int cbSize;
            public IntPtr hWnd;
            public int uCallbackMessage;
            public int uEdge;
            public RECT rc;
            public IntPtr lParam;
        }

        public struct THICKNESS(int l, int t, int r, int b) : IEquatable<THICKNESS>
        {
            public int left = l;
            public int top = t;
            public int right = r;
            public int bottom = b;

            public override string ToString() => $"{{l: {left}, t: {top}, r: {right}, b: {bottom}}}";

            public override bool Equals(object obj) => obj is THICKNESS t && Equals(t);
            public override int GetHashCode() => left ^ top ^ right ^ bottom;
                
            public bool Equals(THICKNESS t) => t.left == left && t.top == top && t.right == right && t.bottom == bottom;

            public static bool operator ==(THICKNESS a, THICKNESS b) => a.Equals(b);
            public static bool operator !=(THICKNESS a, THICKNESS b) => !a.Equals(b);
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int left;
            public int top;
            public int right;
            public int bottom;

            public RECT(int left, int top, int right, int bottom)
            {
                this.left = left;
                this.top = top;
                this.right = right;
                this.bottom = bottom;
            }

            public int Width
            {
                get { return right - left; }
            }

            public int Height
            {
                get { return bottom - top; }
            }

            public static explicit operator Int32Rect(RECT r)
            {
                return new Int32Rect(r.left, r.top, r.Width, r.Height);
            }

            public static explicit operator Rect(RECT r)
            {
                return new Rect(r.left, r.top, r.Width, r.Height);
            }

            public static explicit operator RECT(Rect r)
            {
                return new RECT((int)r.Left, (int)r.Top, (int)r.Right, (int)r.Bottom);
            }

            public readonly RECT Inflate(THICKNESS t)
            {
                return new RECT(
                    left - t.left,
                    top - t.top,
                    right + t.right,
                    bottom + t.bottom
                );
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct WINDOWPOS
        {
            public IntPtr hwnd;
            public IntPtr hwndInsertAfter;
            public int x;
            public int y;
            public int cx;
            public int cy;
            public int flags;
        }

        public const int
            SWP_NOMOVE = 0x0002,
            SWP_NOSIZE = 0x0001;

        public const int
            WM_SIZE = 0x0005,
            WM_ACTIVATE = 0x0006,
            WM_WINDOWPOSCHANGED = 0x0047,
            WM_SYSCOMMAND = 0x0112,
            WM_WINDOWPOSCHANGING = 0x0046;

        public const int
            SC_MOVE = 0xF010;

        public const int
            SIZE_MINIMIZED = 1;

        public const int
            DWMWA_EXTENDED_FRAME_BOUNDS = 9;

        public static int SC_FROM_WPARAM(IntPtr wparam)
        {
            // In WM_SYSCOMMAND messages, the four low-order bits of the wParam parameter are used internally by
            // the system. To obtain the correct result when testing the value of wParam, an application must
            // combine the value 0xFFF0 with the wParam value by using the bitwise AND operator.
            return ((int)wparam & 0xfff0);
        }

        public enum ABM
        {
            NEW = 0,
            REMOVE,
            QUERYPOS,
            SETPOS,
            GETSTATE,
            GETTASKBARPOS,
            ACTIVATE,
            GETAUTOHIDEBAR,
            SETAUTOHIDEBAR,
            WINDOWPOSCHANGED,
            SETSTATE
        }

        public enum ABN
        {
            STATECHANGE = 0,
            POSCHANGED,
            FULLSCREENAPP,
            WINDOWARRANGE
        }

        public const int
            GWL_EXSTYLE = -20;

        public const int
            WS_EX_TOOLWINDOW = 0x00000080;

        [DllImport(Shell32, ExactSpelling = true)]
        public static extern uint SHAppBarMessage(ABM dwMessage, ref APPBARDATA pData);

        [DllImport(User32, CharSet = CharSet.Unicode)]
        public static extern int RegisterWindowMessage(string msg);

        [DllImport(User32, ExactSpelling = true, SetLastError = true)]
        public static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int x, int y, int cx, int cy, uint flags);

        public static bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, RECT rect, uint flags)
            => SetWindowPos(hWnd, hWndInsertAfter, rect.left, rect.top, rect.Width, rect.Height, flags);

        public static IntPtr GetWindowLongPtr(IntPtr hWnd, int index)
            => IntPtr.Size == 4 ? GetWindowLongPtr32(hWnd, index) : GetWindowLongPtr64(hWnd, index);

        [DllImport(User32, EntryPoint = "GetWindowLong")]
        private static extern IntPtr GetWindowLongPtr32(IntPtr hWnd, int index);

        [DllImport(User32, EntryPoint = "GetWindowLongPtr")]
        private static extern IntPtr GetWindowLongPtr64(IntPtr hWnd, int index);

        public static IntPtr SetWindowLongPtr(IntPtr hWnd, int index, IntPtr newLong)
            => IntPtr.Size == 4 ? SetWindowLongPtr32(hWnd, index, newLong) : SetWindowLongPtr64(hWnd, index, newLong);

        [DllImport(User32, EntryPoint = "SetWindowLong")]
        private static extern IntPtr SetWindowLongPtr32(IntPtr hWnd, int index, IntPtr newLong);

        [DllImport(User32, EntryPoint = "SetWindowLongPtr")]
        private static extern IntPtr SetWindowLongPtr64(IntPtr hWnd, int index, IntPtr newLong);

        [DllImport(User32, SetLastError = true)]
        public static extern bool GetWindowRect(IntPtr hWnd, out RECT rect);

        [DllImport(Dwmapi)]
        public static extern int DwmGetWindowAttribute(IntPtr hWnd, uint dwAttribute, out RECT pvAttribute, int cbAttribute);

        [Flags]
        public enum MONITORINFOF
        {
            PRIMARY = 0x1
        }

        [Flags]
        public enum MONITOR_DPI_TYPE
        {
            MDT_EFFECTIVE_DPI = 0,
            MDT_ANGULAR_DPI = 1,
            MDT_RAW_DPI = 2,
            MDT_DEFAULT = MDT_EFFECTIVE_DPI
        }

        private const int CCHDEVICENAME = 32;

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public struct MONITORINFOEX
        {
            public int cbSize;
            public RECT rcMonitor;
            public RECT rcWork;
            public MONITORINFOF dwFlags;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = CCHDEVICENAME)]
            public string szDevice;
        }

        public delegate bool MonitorEnumDelegate(IntPtr hMonitor, IntPtr hdcMonitor, ref RECT lprcMonitor, IntPtr dwData);

        [DllImport(User32, CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern bool GetMonitorInfo(IntPtr hMonitor, ref MONITORINFOEX lpmi);

        [DllImport(User32)]
        public static extern bool EnumDisplayMonitors(IntPtr hdc, IntPtr lprcClip, MonitorEnumDelegate lpfnEnum, IntPtr dwData);

        [DllImport(ShCore, ExactSpelling = true, PreserveSig = false)]
        public static extern void GetDpiForMonitor(IntPtr hMonitor, MONITOR_DPI_TYPE dpiType, out int dpiX, out int dpiY);

        //[DllImport(User32, ExactSpelling = true, SetLastError = true)]
        //public static extern bool PhysicalToLogicalPointForPerMonitorDPI(IntPtr hWnd, POINT point);
    }
}
