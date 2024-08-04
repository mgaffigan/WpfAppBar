namespace Itp.WinFormsAppBar.Demo;

public partial class Form1 : AppBarForm
{
    public Form1()
    {
        InitializeComponent();

        cbEdge.Items.AddRange(Enum.GetNames<AppBarDockMode>());
        cbEdge.SelectedIndex = 0;
        cbEdge.SelectedIndexChanged += cbEdge_SelectedIndexChanged;

        var monitors = MonitorInfo.GetAllMonitors()
                .OrderBy(o => o.ViewportBounds.Left)
                .ThenBy(o => o.ViewportBounds.Top)
                .ToArray();
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

    private void btMinimize_Click(object sender, EventArgs e)
    {
        if (!ShowInTaskbar)
        {
            this.ShowInTaskbar = true;
            this.btMinimize.Text = "&Minimize";
        }
        else
        {
            this.WindowState = FormWindowState.Minimized;
        }
    }

    private void dragThumb1_DragDelta(object sender, DragDeltaEventArgs e)
    {
        this.DockedWidthOrHeight -= e.Delta;
    }
}
