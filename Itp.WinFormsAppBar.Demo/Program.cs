using Microsoft.Win32;
using System.Runtime.InteropServices;

namespace Itp.WinFormsAppBar.Demo;

internal static class Program
{
    [STAThread]
    static void Main()
    {
        ApplicationConfiguration.Initialize();
#if NET5_0_OR_GREATER && !NET8_0_OR_GREATER
        Application.Run(new SystemEventsSafeAppContext(new Form1()));
#else
        Application.Run(new Form1());
#endif
    }
}