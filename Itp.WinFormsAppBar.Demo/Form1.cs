namespace Itp.WinFormsAppBar.Demo;

public partial class Form1 : AppBarForm
{
    public Form1()
    {
        InitializeComponent();

        cbEdge.Items.AddRange(Enum.GetNames<AppBarDockMode>());
        cbEdge.SelectedIndex = 0;
        cbEdge.SelectedIndexChanged += cbEdge_SelectedIndexChanged;

        var monitors = MonitorInfo.GetAllMonitors().ToArray();
        Monitor = monitors[0];
        cbMonitor.Items.AddRange(monitors);
        cbMonitor.SelectedIndex = 0;
        cbMonitor.SelectedIndexChanged += cbMonitor_SelectedIndexChanged;
    }

    private void cbEdge_SelectedIndexChanged(object sender, EventArgs e)
    {
        DockMode = Enum.Parse<AppBarDockMode>((string)cbEdge.SelectedItem);
    }

    private void cbMonitor_SelectedIndexChanged(object sender, EventArgs e)
    {
        Monitor = (MonitorInfo)cbMonitor.SelectedItem;
    }

    private void btClose_Click(object sender, EventArgs e)
    {
        Close();
    }

    //private void rzThumb_DragCompleted(object sender, DragCompletedEventArgs e)
    //{
    //    double delta;
    //    switch (DockMode)
    //    {
    //        case AppBarDockMode.Left:
    //            delta = e.HorizontalChange;
    //            break;
    //        case AppBarDockMode.Right:
    //            delta = e.HorizontalChange * -1;
    //            break;
    //        case AppBarDockMode.Top:
    //            delta = e.VerticalChange;
    //            break;
    //        case AppBarDockMode.Bottom:
    //            delta = e.VerticalChange * -1;
    //            break;
    //        default: throw new NotSupportedException();
    //    }

    //    this.DockedWidthOrHeight += (int)(delta / VisualTreeHelper.GetDpi(this).PixelsPerDip);
    //}
}
