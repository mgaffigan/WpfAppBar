using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace WpfAppBar
{
    using static NativeMethods;

    public class AppBarWindow : Window
    {
        private bool IsAppBarRegistered;
        private bool IsResizeAllowed;

        public AppBarWindow()
        {
            this.WindowStyle = WindowStyle.None;
            this.ResizeMode = ResizeMode.NoResize;
            this.Topmost = true;
            this.ShowInTaskbar = false;
        }

        public AppBarDockMode DockMode
        {
            get { return (AppBarDockMode)GetValue(DockModeProperty); }
            set { SetValue(DockModeProperty, value); }
        }

        public static readonly DependencyProperty DockModeProperty =
            DependencyProperty.Register("DockMode", typeof(AppBarDockMode), typeof(AppBarWindow),
                new FrameworkPropertyMetadata(AppBarDockMode.Left, DockLocation_Changed));

        public MonitorInfo Monitor
        {
            get { return (MonitorInfo)GetValue(MonitorProperty); }
            set { SetValue(MonitorProperty, value); }
        }

        public static readonly DependencyProperty MonitorProperty =
            DependencyProperty.Register("Monitor", typeof(MonitorInfo), typeof(AppBarWindow),
                new FrameworkPropertyMetadata(null, DockLocation_Changed));

        public int DockedWidthOrHeight
        {
            get { return (int)GetValue(DockedWidthOrHeightProperty); }
            set { SetValue(DockedWidthOrHeightProperty, value); }
        }

        public static readonly DependencyProperty DockedWidthOrHeightProperty =
            DependencyProperty.Register("DockedWidthOrHeight", typeof(int), typeof(AppBarWindow),
                new FrameworkPropertyMetadata(200, DockLocation_Changed));

        private static void DockLocation_Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var @this = (AppBarWindow)d;

            if (@this.IsAppBarRegistered)
            {
                @this.OnDockLocationChanged();
            }
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);

            // add the hook, setup the appbar
            var source = (HwndSource)PresentationSource.FromVisual(this);
            source.AddHook(WndProc);

            var abd = GetAppBarData();
            SHAppBarMessage(ABM.NEW, ref abd);

            // set our initial location
            this.IsAppBarRegistered = true;
            OnDockLocationChanged();
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);

            if (IsAppBarRegistered)
            {
                var abd = GetAppBarData();
                SHAppBarMessage(ABM.REMOVE, ref abd);
                IsAppBarRegistered = false;
            }
        }

        private void OnDockLocationChanged()
        {
            var abd = GetAppBarData();
            abd.rc = (RECT)GetSelectedMonitor().ViewportBounds;

            SHAppBarMessage(ABM.QUERYPOS, ref abd);

            switch (DockMode)
            {
                case AppBarDockMode.Top:
                    abd.rc.bottom = abd.rc.top + DockedWidthOrHeight;
                    break;
                case AppBarDockMode.Bottom:
                    abd.rc.top = abd.rc.bottom - DockedWidthOrHeight;
                    break;
                case AppBarDockMode.Left:
                    abd.rc.right = abd.rc.left + DockedWidthOrHeight;
                    break;
                case AppBarDockMode.Right:
                    abd.rc.left = abd.rc.right - DockedWidthOrHeight;
                    break;
                default: throw new NotSupportedException();
            }

            SHAppBarMessage(ABM.SETPOS, ref abd);
            IsResizeAllowed = true;
            try
            {
                WindowBounds = (Rect)abd.rc;
            }
            finally
            {
                IsResizeAllowed = false;
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
                hWnd = new WindowInteropHelper(this).Handle,
                uCallbackMessage = AppBarMessageId,
                uEdge = (int)DockMode
            };
        }

        private static int _AppBarMessageId;
        public static int AppBarMessageId
        {
            get
            {
                if (_AppBarMessageId == 0)
                {
                    _AppBarMessageId = RegisterWindowMessage("AppBarMessage_EEDFB5206FC3");
                }

                return _AppBarMessageId;
            }
        }

        public IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == WM_WINDOWPOSCHANGING && !IsResizeAllowed)
            {
                var wp = Marshal.PtrToStructure<WINDOWPOS>(lParam);
                wp.flags |= SWP_NOMOVE | SWP_NOSIZE;
                Marshal.StructureToPtr(wp, lParam, false);
            }
            else if (msg == WM_ACTIVATE)
            {
                var abd = GetAppBarData();
                SHAppBarMessage(ABM.ACTIVATE, ref abd);
            }
            else if (msg == WM_WINDOWPOSCHANGED)
            {
                var abd = GetAppBarData();
                SHAppBarMessage(ABM.WINDOWPOSCHANGED, ref abd);
            }
            else if (msg == AppBarMessageId)
            {
                switch ((ABN)(int)wParam)
                {
                    case ABN.POSCHANGED:
                        OnDockLocationChanged();
                        handled = true;
                        break;
                }
            }

            return IntPtr.Zero;
        }

        private Rect WindowBounds
        {
            set
            {
                this.Left = value.Left;
                this.Top = value.Top;
                this.Width = value.Width;
                this.Height = value.Height;
            }
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
