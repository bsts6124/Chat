using System.Windows;
using System.Windows.Input;

namespace Client
{
    /// <summary>
    /// MediaContainerWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MediaContainerWindow : Window
    {
        public MediaContainerWindow()
        {
            InitializeComponent();
        }

        private void Rectangle_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }
    }
}
