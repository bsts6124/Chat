using System.Windows;
using System.Windows.Input;

namespace Client
{
    /// <summary>
    /// MediaViewerWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MediaViewerWindow : Window
    {
        public MediaViewerWindow()
        {
            InitializeComponent();
        }

        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void ImageBorder_MouseEnter(object sender, MouseEventArgs e)
        {

        }
    }
}
