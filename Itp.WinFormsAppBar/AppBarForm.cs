using System;
using System.Linq;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Drawing;

namespace Itp.WinFormsAppBar
{
    using static NativeMethods;

    public class AppBarForm : Form
    {
        private bool IsAppBarRegistered;
        private bool IsInAppBarResize;
        private bool IsMinimized;

        public AppBarForm()
        {
            this.ShowInTaskbar = false;
            this.FormBorderStyle = FormBorderStyle.None;
            this.TopMost = true;
            // Min Size cannot be set unless max size is also set
            this.MaximumSize = new(10240, 10240);
            this.MinimumSize = new(20, 20);
            this.MinimumSizeChanged += MinMaxHeightWidth_Changed;
        }

        private AppBarDockMode _DockMode = AppBarDockMode.Left;
        public AppBarDockMode DockMode
        {
            get => _DockMode;
            set
            {
                _DockMode = value;
                DockLocation_Changed(this, EventArgs.Empty);
            }
        }

        private MonitorInfo? _Monitor = null;
        public MonitorInfo? Monitor
        {
            get => _Monitor;
            set
            {
                _Monitor = value;
                DockLocation_Changed(this, EventArgs.Empty);
            }
        }

        private int _DockedWidthOrHeight = 200;
        public int DockedWidthOrHeight
        {
            get => _DockedWidthOrHeight;
            set
            {
                _DockedWidthOrHeight = DockedWidthOrHeight_Coerce(value);
                DockLocation_Changed(this, EventArgs.Empty);
            }
        }

        private int DockedWidthOrHeight_Coerce(int newValue)
        {
            switch (this.DockMode)
            {
                case AppBarDockMode.Left:
                case AppBarDockMode.Right:
                    return Clamp(newValue, this.MinimumSize.Width, this.MaximumSize.Width);

                case AppBarDockMode.Top:
                case AppBarDockMode.Bottom:
                    return Clamp(newValue, this.MinimumSize.Height, this.MaximumSize.Height);

                default: throw new NotSupportedException();
            }
        }

        private static int Clamp(int value, int min, int max)
        {
#if NET
            return int.Clamp(value, min, max);
#else
            if (min > value)
            {
                return min;
            }
            if (max < value)
            {
                return max;
            }

            return value;
#endif
        }

        private void MinMaxHeightWidth_Changed(object d, EventArgs e)
        {
            DockedWidthOrHeight = DockedWidthOrHeight;
        }

        private void DockLocation_Changed(object d, EventArgs e)
        {
            this.OnDockLocationChanged();
        }

#if NET472_OR_GREATER
        protected override void OnDpiChanged(DpiChangedEventArgs e)
        {
            base.OnDpiChanged(e);

            OnDockLocationChanged();
        }
#endif

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);

            if (DesignMode)
            {
                return;
            }

            SystemEventsSafeAppContext.AssertSafeContext();

            // add the hook, setup the appbar
            if (!ShowInTaskbar)
            {
                var exstyle = (ulong)GetWindowLongPtr(Handle, GWL_EXSTYLE);
                exstyle |= (ulong)((uint)WS_EX_TOOLWINDOW);
                SetWindowLongPtr(Handle, GWL_EXSTYLE, unchecked((IntPtr)exstyle));
            }

            var abd = GetAppBarData();
            SHAppBarMessage(ABM.NEW, ref abd);

            // set our initial location
            this.IsAppBarRegistered = true;
            OnDockLocationChanged();
        }

#if NET472_OR_GREATER
        private int WpfDimensionToDesktop(int dim)
        {
            return (int)Math.Ceiling(dim * (DeviceDpi / 96d));
        }

        private int DesktopDimensionToWpf(int dim)
        {
            return (int)Math.Ceiling(dim / (DeviceDpi / 96d));
        }
#else
        private int WpfDimensionToDesktop(int dim) => dim;

        private int DesktopDimensionToWpf(int dim) => dim;
#endif

        private void OnDockLocationChanged()
        {
            if (DesignMode || LicenseManager.UsageMode == LicenseUsageMode.Designtime)
            {
                return;
            }
            if (!IsAppBarRegistered || IsInAppBarResize)
            {
                return;
            }

            var abd = GetAppBarData();
            abd.rc = (RECT)GetSelectedMonitor().ViewportBounds;

            SHAppBarMessage(ABM.QUERYPOS, ref abd);

            var dockedWidthOrHeightInDesktopPixels = IsMinimized ? 0 : WpfDimensionToDesktop(DockedWidthOrHeight);
            switch (DockMode)
            {
                case AppBarDockMode.Top:
                    abd.rc.bottom = abd.rc.top + dockedWidthOrHeightInDesktopPixels;
                    break;
                case AppBarDockMode.Bottom:
                    abd.rc.top = abd.rc.bottom - dockedWidthOrHeightInDesktopPixels;
                    break;
                case AppBarDockMode.Left:
                    abd.rc.right = abd.rc.left + dockedWidthOrHeightInDesktopPixels;
                    break;
                case AppBarDockMode.Right:
                    abd.rc.left = abd.rc.right - dockedWidthOrHeightInDesktopPixels;
                    break;
                default: throw new NotSupportedException();
            }

            SHAppBarMessage(ABM.SETPOS, ref abd);
            if (!IsMinimized)
            {
                IsInAppBarResize = true;
                try
                {
                    SetBounds(
                        DesktopDimensionToWpf(abd.rc.left),
                        DesktopDimensionToWpf(abd.rc.top),
                        DesktopDimensionToWpf(abd.rc.Width),
                        DesktopDimensionToWpf(abd.rc.Height)
                    );
                }
                finally
                {
                    IsInAppBarResize = false;
                }
            }
        }

        private MonitorInfo GetSelectedMonitor()
        {
            var monitor = this.Monitor;
            var allMonitors = MonitorInfo.GetAllMonitors();
            if (monitor == null || !allMonitors.Contains(monitor))
            {
                monitor = allMonitors.First(f => f.IsPrimary);
            }

            return monitor;
        }

        private APPBARDATA GetAppBarData()
        {
            return new APPBARDATA()
            {
                cbSize = Marshal.SizeOf(typeof(APPBARDATA)),
                hWnd = Handle,
                uCallbackMessage = AppBarMessageId,
                uEdge = (int)DockMode
            };
        }

        private static int AppBarMessageId = RegisterWindowMessage("AppBarMessage_EEDFB5206FC4");

        protected override void WndProc(ref Message m)
        {
            if (DesignMode)
            {
                // nop
            }
            if (m.Msg == WM_SIZE)
            {
                this.IsMinimized = ShowInTaskbar && m.WParam == (IntPtr)SIZE_MINIMIZED;
                OnDockLocationChanged();
            }
            else if (m.Msg == WM_WINDOWPOSCHANGING && !IsInAppBarResize)
            {
                var wp = Marshal.PtrToStructure<WINDOWPOS>(m.LParam);
                const int NOMOVE_NORESIZE = SWP_NOMOVE | SWP_NOSIZE;
                if ((wp.flags & NOMOVE_NORESIZE) != NOMOVE_NORESIZE
                    && !IsMinimized
                    && !(wp.x == -32_000 && wp.y == -32_000) /* loc for minimized windows */)
                {
                    wp.flags |= NOMOVE_NORESIZE;
                    Marshal.StructureToPtr(wp, m.LParam, false);
                }
            }
            else if (m.Msg == WM_ACTIVATE)
            {
                var abd = GetAppBarData();
                SHAppBarMessage(ABM.ACTIVATE, ref abd);
            }
            else if (m.Msg == WM_DESTROY)
            {
                if (IsAppBarRegistered)
                {
                    var abd = GetAppBarData();
                    SHAppBarMessage(ABM.REMOVE, ref abd);
                    IsAppBarRegistered = false;
                }
            }
            else if (m.Msg == WM_WINDOWPOSCHANGED)
            {
                var abd = GetAppBarData();
                SHAppBarMessage(ABM.WINDOWPOSCHANGED, ref abd);
            }
            else if (m.Msg == AppBarMessageId)
            {
                switch ((ABN)(int)m.WParam)
                {
                    case ABN.POSCHANGED:
                        OnDockLocationChanged();
                        return;
                }
            }

            base.WndProc(ref m);
        }
    }

    public enum AppBarDockMode
    {
        Left = 0,
        Top,
        Right,
        Bottom
    }
}
