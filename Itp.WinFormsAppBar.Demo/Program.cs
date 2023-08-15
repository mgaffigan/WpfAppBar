using Microsoft.Win32;
using System.Runtime.InteropServices;

namespace Itp.WinFormsAppBar.Demo;

internal static class Program
{
    [STAThread]
    static void Main()
    {
        ApplicationConfiguration.Initialize();
        Application.Run(new SystemEventsSafeAppContext(new Form1()));
        //Application.Run(new Form1());
    }
}