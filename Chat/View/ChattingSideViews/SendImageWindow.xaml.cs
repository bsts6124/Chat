using System;
using System.Windows;
using System.Windows.Input;

namespace Client
{
    /// <summary>
    /// SendImageWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class SendImageWindow : Window
    {
        public SendImageWindow(int roomnum)
        {
            InitializeComponent();
            DataContext = new SendImageWindowViewModel(roomnum);
            if (((dynamic)DataContext).CloseAction == null) ((dynamic)DataContext).CloseAction = new Action(() => { this.Close(); });
        }

        ~SendImageWindow()
        {
        }

        private void Rectangle_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        private void Window_Deactivated(object sender, EventArgs e)
        {
            try
            {
                Close();
            }
            catch
            {

            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
