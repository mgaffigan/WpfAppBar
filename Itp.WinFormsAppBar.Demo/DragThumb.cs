#nullable enable

namespace Itp.WinFormsAppBar.Demo;

internal class DragThumb : Control
{
    public DragThumb()
    {
        SetStyle(ControlStyles.Selectable, false);
        TabStop = false;

        Width = 4;
        Dock = DockStyle.Right;
    }

    public override AnchorStyles Anchor
    {
        get => AnchorStyles.None;
        set { /* nop */ }
    }

    protected override Cursor DefaultCursor => Cursors.VSplit;

    private Point? MoveStart;
    protected override void OnMouseDown(MouseEventArgs e)
    {
        base.OnMouseDown(e);

        if (e.Button == MouseButtons.Left)
        {
            Capture = true;
            MoveStart = e.Location;
        }
    }

    public event EventHandler<DragDeltaEventArgs>? DragDelta;
    protected override void OnMouseUp(MouseEventArgs e)
    {
        base.OnMouseUp(e);

        Capture = false;
        if (MoveStart is Point p)
        {
            var delta = p.X - e.X;
            DragDelta?.Invoke(this, new DragDeltaEventArgs(delta));
        }
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        base.OnMouseMove(e);

        if (Capture && !e.Button.HasFlag(MouseButtons.Left))
        {
            Capture = false;
        }
    }
}

internal class DragDeltaEventArgs : EventArgs
{
    public DragDeltaEventArgs(int delta)
    {
        Delta = delta;
    }

    public int Delta { get; }
}
