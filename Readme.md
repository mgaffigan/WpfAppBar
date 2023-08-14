# Itp.WpfAppBar

[![Nuget](https://img.shields.io/nuget/v/Itp.WpfAppBar)](https://www.nuget.org/packages/Itp.WpfAppBar)

Implementation of an `AppBar` in WPF based off of [Using Application Desktop Toolbars](https://msdn.microsoft.com/en-us/library/bb776821.aspx) and [Extend the Windows 95 Shell with Application Desktop Toolbars](https://www.microsoft.com/msj/archive/S274.aspx).

## Goals:

 - Allow dock to any side of the screen
 - Allow dock to a particular monitor
 - Allow resizing of the appbar
 - Handle screen layout changes and monitor disconnections
 - Handle <kbd>Win</kbd> + <kbd>Shift</kbd> + <kbd>Left</kbd> and attempts to minimize or move the window
 - Handle co-operation with other appbars (OneNote et al.)
 - Handle per-monitor DPI scaling
 
 ## License

MIT License

## Example use

    <apb:AppBarWindow x:Class="WpfAppBarDemo.MainWindow" xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation" xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        xmlns:apb="clr-namespace:WpfAppBar;assembly=WpfAppBar"
        DataContext="{Binding RelativeSource={RelativeSource Self}}" Title="MainWindow" 
        DockedWidthOrHeight="200" MinWidth="100" MinHeight="100">
        <Grid>
            <Button x:Name="btClose" Content="Close" HorizontalAlignment="Left" VerticalAlignment="Top" Width="75" Height="23" Margin="10,10,0,0" Click="btClose_Click"/>
            <ComboBox x:Name="cbMonitor" SelectedItem="{Binding Path=Monitor, Mode=TwoWay}" HorizontalAlignment="Left" VerticalAlignment="Top" Width="120" Margin="10,38,0,0"/>
            <ComboBox x:Name="cbEdge" SelectedItem="{Binding Path=DockMode, Mode=TwoWay}" HorizontalAlignment="Left" Margin="10,65,0,0" VerticalAlignment="Top" Width="120"/>

            <Thumb Width="5" HorizontalAlignment="Right" Background="Gray" x:Name="rzThumb" Cursor="SizeWE" DragCompleted="rzThumb_DragCompleted" />
        </Grid>
    </apb:AppBarWindow>

Codebehind:

    public partial class MainWindow
    {
        public MainWindow()
        {
            InitializeComponent();

            this.cbEdge.ItemsSource = new[]
            {
                AppBarDockMode.Left,
                AppBarDockMode.Right,
                AppBarDockMode.Top,
                AppBarDockMode.Bottom
            };
            this.cbMonitor.ItemsSource = MonitorInfo.GetAllMonitors();
        }

        private void btClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void rzThumb_DragCompleted(object sender, DragCompletedEventArgs e)
        {
            this.DockedWidthOrHeight += (int)(e.HorizontalChange / VisualTreeHelper.GetDpi(this).PixelsPerDip);
        }
    }

App.config (for Monitor-specific DPI support):

    <?xml version="1.0" encoding="utf-8"?>
    <configuration>
    <startup>
        <supportedRuntime version="v4.0" sku=".NETFramework,Version=v4.6.2"/>
    </startup>
    <runtime>
        <AppContextSwitchOverrides value="Switch.System.Windows.DoNotScaleForDpiChanges=false"/>
    </runtime>
    </configuration>

App.manifest (for Monitor-specific DPI support):

    <?xml version="1.0" encoding="utf-8"?>
    <assembly manifestVersion="1.0" xmlns="urn:schemas-microsoft-com:asm.v1">
    <assemblyIdentity version="1.0.0.0" name="MyApplication.app"/>
    <trustInfo xmlns="urn:schemas-microsoft-com:asm.v2">
        <security>
        <requestedPrivileges xmlns="urn:schemas-microsoft-com:asm.v3">
            <requestedExecutionLevel level="asInvoker" uiAccess="false" />
        </requestedPrivileges>
        </security>
    </trustInfo>
    <compatibility xmlns="urn:schemas-microsoft-com:compatibility.v1">
        <application>
        <!-- Windows 10 -->
        <supportedOS Id="{8e0f7a12-bfb3-4fe8-b9a5-48fd50a15a9a}" />
        </application>
    </compatibility>
    <application xmlns="urn:schemas-microsoft-com:asm.v3">
        <windowsSettings>
        <dpiAware xmlns="http://schemas.microsoft.com/SMI/2005/WindowsSettings">true</dpiAware>
        <dpiAwareness xmlns="http://schemas.microsoft.com/SMI/2016/WindowsSettings">PerMonitor</dpiAwareness>
        </windowsSettings>
    </application>
    </assembly>

## Screenshots

Changing docked position ([sample](https://github.com/mgaffigan/WpfAppBar/tree/master/WpfAppBarDemo)):

> [![AppBar docked to edges][1]][1]

Resizing with thumb:

> [![Resize][2]][2]

Cooperation with other appbars:

> [![Coordination][3]][3]

Dock and undock ([sample](https://github.com/mgaffigan/WpfAppBar/tree/master/UndockSample)):

> [![Undock][4]][4]

[Add Nuget](https://www.nuget.org/packages/Itp.WpfAppBar) or clone [from GitHub](https://github.com/mgaffigan/WpfAppBar) if you want to use it.  The library itself is only three files, and can easily be dropped in a project.

    <PackageReference Include="Itp.WpfAppBar" Version="*" />

  [1]: https://i.stack.imgur.com/f13P8.gif
  [2]: https://i.stack.imgur.com/ifgn8.gif
  [3]: https://i.stack.imgur.com/PiydR.gif
  [4]: https://user-images.githubusercontent.com/12316225/95240977-c552da00-07d2-11eb-8ceb-8031641b1151.gif