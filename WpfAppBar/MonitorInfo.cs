using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using static WpfAppBar.NativeMethods;

namespace WpfAppBar
{
    public sealed class MonitorInfo : IEquatable<MonitorInfo>
    {
        public Rect ViewportBounds { get; }

        public Rect WorkAreaBounds { get; }

        public bool IsPrimary { get; }

        public string DeviceId { get; }

        internal MonitorInfo(MONITORINFOEX mex)
        {
            this.ViewportBounds = (Rect)mex.rcMonitor;
            this.WorkAreaBounds = (Rect)mex.rcWork;
            this.IsPrimary = mex.dwFlags.HasFlag(MONITORINFOF.PRIMARY);
            this.DeviceId = mex.szDevice;
        }

        public static IEnumerable<MonitorInfo> GetAllMonitors()
        {
            var monitors = new List<MonitorInfo>();
            MonitorEnumDelegate callback = delegate (IntPtr hMonitor, IntPtr hdcMonitor, ref RECT lprcMonitor, IntPtr dwData)
            {
                MONITORINFOEX mi = new MONITORINFOEX();
                mi.cbSize = Marshal.SizeOf(typeof(MONITORINFOEX));
                if (!GetMonitorInfo(hMonitor, ref mi))
                {
                    throw new Win32Exception();
                }

                monitors.Add(new MonitorInfo(mi));
                return true;
            };

            EnumDisplayMonitors(IntPtr.Zero, IntPtr.Zero, callback, IntPtr.Zero);

            return monitors;
        }

        public override string ToString() => DeviceId;

        public override bool Equals(object obj) => Equals(obj as MonitorInfo);

        public override int GetHashCode() => DeviceId.GetHashCode();

        public bool Equals(MonitorInfo other) => this.DeviceId == other?.DeviceId;

        public static bool operator ==(MonitorInfo a, MonitorInfo b)
        {
            if (ReferenceEquals(a, b))
            {
                return true;
            }

            if (ReferenceEquals(a, null))
            {
                return false;
            }

            return a.Equals(b);
        }

        public static bool operator !=(MonitorInfo a, MonitorInfo b) => !(a == b);
    }
}
