using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using WpfAppBar;

namespace UndockSample
{
    public class DockableWindowControl : ContentControl
    {
        public DockableWindowMode DockMode
        {
            get => AppBarHost == null ? DockableWindowMode.Free : DockableWindowMode.Docked;
            set
            {
                if (WindowHost == null && AppBarHost == null)
                {
                    throw new InvalidOperationException("Call Show before changing dock mode");
                }
                if (value == DockMode)
                {
                    return;
                }

                ShowDockMode(value);
            }
        }

        public Window CurrentWindow => WindowHost ?? AppBarHost;

        private void ShowDockMode(DockableWindowMode value)
        {
            var app = Application.Current;
            var currentShutdownMode = app.ShutdownMode;
            if (app.MainWindow == CurrentWindow)
            {
                app.ShutdownMode = ShutdownMode.OnExplicitShutdown;
            }
            try
            {
                if (value == DockableWindowMode.Free)
                {
                    if (AppBarHost != null)
                    {
                        AppBarHost.Content = null;
                        AppBarHost.Close();
                    }

                    WindowHost = new Window();
                    SetupWindowHost(WindowHost);
                    WindowHost.Content = this;
                    WindowHost.Show();
                }
                else
                {
                    if (WindowHost != null)
                    {
                        WindowHost.Content = null;
                        WindowHost.Close();
                    }

                    AppBarHost = new AppBarWindow();
                    SetupAppBarHost(AppBarHost);
                    AppBarHost.Content = this;
                    AppBarHost.Show();
                }
            }
            finally
            {
                app.MainWindow = CurrentWindow;
                app.ShutdownMode = currentShutdownMode;
            }
        }

        public void Show(DockableWindowMode mode)
        {
            ShowDockMode(mode);
        }

        public virtual void SetupAppBarHost(AppBarWindow wnd)
        {
            // no-op
        }

        public virtual void SetupWindowHost(Window wnd)
        {
            // no-op
        }

        private Window WindowHost;
        private AppBarWindow AppBarHost;
    }

    public class WindowChangingEventArgs : EventArgs
    {
        public Window Window { get; }

        public WindowChangingEventArgs(Window window)
        {
            this.Window = window ?? throw new ArgumentNullException(nameof(window));
        }
    }

    public enum DockableWindowMode { Free, Docked }
}
