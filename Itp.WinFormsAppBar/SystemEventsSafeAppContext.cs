using Microsoft.Win32;
using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;

namespace Itp.WinFormsAppBar;

#if NET5_0_OR_GREATER && !NET8_0_OR_GREATER
public class SystemEventsSafeAppContext : ApplicationContext
{
    [ThreadStatic]
    private static bool HasSafeContext;

    public SystemEventsSafeAppContext(Form mainForm)
        : base(mainForm)
    {
        HasSafeContext = true;
    }

    public static void AssertSafeContext()
    {
        if (!HasSafeContext)
        {
            throw new InvalidOperationException("SystemEventsSafeAppContext required.  See https://github.com/mgaffigan/WpfAppBar#Winforms-Safe-Context");
        }
    }

    [DllImport("user32.dll")]
    static extern void PostQuitMessage(int nExitCode);

    protected override void OnMainFormClosed(object sender, EventArgs e)
    {
        var syncCtx = SynchronizationContext.Current;
        SystemEvents.InvokeOnEventsThread(() =>
        {
            PostQuitMessage(0);
            syncCtx.Post((_) =>
            {
                ExitThread();
            }, null);
        });
    }
}
#else
public class SystemEventsSafeAppContext : ApplicationContext
{
    [Obsolete("SystemEventsSafeAppContext is only required for .NET 5 to .NET 7.  See https://github.com/dotnet/winforms/issues/9721")]
    public SystemEventsSafeAppContext(Form mainForm) : base(mainForm) { /* nop */ }
    public static void AssertSafeContext() { /* nop */ }
}
#endif