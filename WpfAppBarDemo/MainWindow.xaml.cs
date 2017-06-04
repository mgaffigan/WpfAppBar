using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using WpfAppBar;

namespace WpfAppBarDemo
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
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
            double delta;
            switch (DockMode)
            {
                case AppBarDockMode.Left:
                    delta = e.HorizontalChange;
                    break;
                case AppBarDockMode.Right:
                    delta = e.HorizontalChange * -1;
                    break;
                case AppBarDockMode.Top:
                    delta = e.VerticalChange;
                    break;
                case AppBarDockMode.Bottom:
                    delta = e.VerticalChange * -1;
                    break;
                default: throw new NotSupportedException();
            }

            this.DockedWidthOrHeight += (int)(delta / VisualTreeHelper.GetDpi(this).PixelsPerDip);
        }
    }
}
