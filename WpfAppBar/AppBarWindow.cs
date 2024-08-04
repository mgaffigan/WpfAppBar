using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Threading;

namespace Itp.WpfAppBar
{
    using static NativeMethods;

    public class AppBarWindow : Window
    {
        private bool IsAppBarRegistered;
        private bool IsInAppBarResize;
        private bool IsMinimized;

        static AppBarWindow()
        {
            ShowInTaskbarProperty.OverrideMetadata(typeof(AppBarWindow), new FrameworkPropertyMetadata(false));
            MinHeightProperty.OverrideMetadata(typeof(AppBarWindow), new FrameworkPropertyMetadata(20d, MinMaxHeightWidth_Changed));
            MinWidthProperty.OverrideMetadata(typeof(AppBarWindow), new FrameworkPropertyMetadata(20d, MinMaxHeightWidth_Changed));
            MaxHeightProperty.OverrideMetadata(typeof(AppBarWindow), new FrameworkPropertyMetadata(MinMaxHeightWidth_Changed));
            MaxWidthProperty.OverrideMetadata(typeof(AppBarWindow), new FrameworkPropertyMetadata(MinMaxHeightWidth_Changed));
        }

        public AppBarWindow()
        {
            this.WindowStyle = WindowStyle.None;
            this.ResizeMode = ResizeMode.NoResize;
            this.Topmost = true;
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
                new FrameworkPropertyMetadata(200, DockLocation_Changed, DockedWidthOrHeight_Coerce));

        private static object DockedWidthOrHeight_Coerce(DependencyObject d, object baseValue)
        {
            var @this = (AppBarWindow)d;
            var newValue = (int)baseValue;

            switch (@this.DockMode)
            {
                case AppBarDockMode.Left:
                case AppBarDockMode.Right:
                    return BoundIntToDouble(newValue, @this.MinWidth, @this.MaxWidth);

                case AppBarDockMode.Top:
                case AppBarDockMode.Bottom:
                    return BoundIntToDouble(newValue, @this.MinHeight, @this.MaxHeight);

                default: throw new NotSupportedException();
            }
        }

        private static int BoundIntToDouble(int value, double min, double max)
        {
            if (min > value)
            {
                return (int)Math.Ceiling(min);
            }
            if (max < value)
            {
                return (int)Math.Floor(max);
            }

            return value;
        }

        private static void MinMaxHeightWidth_Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            d.CoerceValue(DockedWidthOrHeightProperty);
        }

        private static void DockLocation_Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var @this = (AppBarWindow)d;

            @this.OnDockLocationChanged();
        }

        protected override void OnDpiChanged(DpiScale oldDpi, DpiScale newDpi)
        {
            base.OnDpiChanged(oldDpi, newDpi);

            Dispatcher.BeginInvoke(DispatcherPriority.SystemIdle, () =>
            {
                // WPF seems to have a race condition during DPI changes where the window size
                // is not updated.  We have to force it away to get it to update.
                Width -= 1;
                Height -= 1;
                OnDockLocationChanged();
            });
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);

            // add the hook, setup the appbar
            var source = (HwndSource)PresentationSource.FromVisual(this);

            if (!ShowInTaskbar)
            {
                var exstyle = (ulong)GetWindowLongPtr(source.Handle, GWL_EXSTYLE);
                exstyle |= (ulong)((uint)WS_EX_TOOLWINDOW);
                SetWindowLongPtr(source.Handle, GWL_EXSTYLE, unchecked((IntPtr)exstyle));
            }

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

            if (e.Cancel)
            {
                return;
            }

            if (IsAppBarRegistered)
            {
                var abd = GetAppBarData();
                SHAppBarMessage(ABM.REMOVE, ref abd);
                IsAppBarRegistered = false;
            }
        }

        private int WpfDimensionToDesktop(double dim)
        {
            var dpi = VisualTreeHelper.GetDpi(this);

            return (int)Math.Ceiling(dim * dpi.PixelsPerDip);
        }

        private double DesktopDimensionToWpf(double dim)
        {
            var dpi = VisualTreeHelper.GetDpi(this);

            return dim / dpi.PixelsPerDip;
        }

        private void OnDockLocationChanged()
        {
            if (DesignerProperties.GetIsInDesignMode(this))
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
                    WindowBounds = (Rect)abd.rc;
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
            if (msg == WM_SIZE)
            {
                this.IsMinimized = ShowInTaskbar && wParam == (IntPtr)SIZE_MINIMIZED;
                OnDockLocationChanged();
            }
            else if (msg == WM_WINDOWPOSCHANGING && !IsInAppBarResize)
            {
                var wp = Marshal.PtrToStructure<WINDOWPOS>(lParam);
                const int NOMOVE_NORESIZE = SWP_NOMOVE | SWP_NOSIZE;
                if ((wp.flags & NOMOVE_NORESIZE) != NOMOVE_NORESIZE
                    && !IsMinimized
                    && !(wp.x == -32_000 && wp.y == -32_000) /* loc for minimized windows */)
                {
                    wp.flags |= NOMOVE_NORESIZE;
                    Marshal.StructureToPtr(wp, lParam, false);
                }
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

        private Thickness FrameThickness;
        private Point LastPosition = new Point(-32000, -32000);
        private Rect WindowBounds
        {
            set
            {
                var ft = FrameThickness;
                if (LastPosition != value.TopLeft)
                {
                    this.FrameThickness = ft = default;
                }

                void SetTopLeft()
                {
                    this.Left = DesktopDimensionToWpf(value.Left - ft.Left);
                    this.Top = DesktopDimensionToWpf(value.Top - ft.Top);
                }
                SetTopLeft();

                if (LastPosition != value.TopLeft)
                {
                    this.FrameThickness = ft = GetFrameThickness();
                    this.LastPosition = value.TopLeft;
                }

                // If we got a new frame thickness (varies per monitor), update
                if (FrameThickness.Top != 0 || FrameThickness.Left != 0)
                {
                    SetTopLeft();
                }

                this.Width = DesktopDimensionToWpf(value.Width + ft.Left + ft.Right);
                this.Height = DesktopDimensionToWpf(value.Height + ft.Top + ft.Bottom);
            }
        }

        private Thickness GetFrameThickness()
        {
            var hWnd = new WindowInteropHelper(this).Handle;
            if (!GetWindowRect(hWnd, out var clientBounds))
            {
                return default;
            }
            if (DwmGetWindowAttribute(hWnd, 9 /* DWMWA_EXTENDED_FRAME_BOUNDS */,
                    out var frameBounds, Marshal.SizeOf<RECT>()) != 0 /* S_OK */)
            {
                return default;
            }
            return new Thickness(
                frameBounds.left - clientBounds.left,
                frameBounds.top - clientBounds.top,
                clientBounds.right - frameBounds.right,
                clientBounds.bottom - frameBounds.bottom);
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
