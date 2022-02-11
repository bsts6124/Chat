using System;
using System.Security;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Client
{
    /// <summary>
    /// LogInWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class LogInWindow : Window
    {
        public SecureString SecurePassword;


        public LogInWindow()
        {
            InitializeComponent();
            LoginWindowViewModel vm = new LoginWindowViewModel();
            this.DataContext = vm;
            if (vm.HideAction == null)
                vm.HideAction = new Action(() => this.Hide());

        }

        void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }

        void PasswordChange(object sender, RoutedEventArgs e)
        {
            if (DataContext != null)
            {
                ((dynamic)DataContext).isPassword = !String.IsNullOrEmpty(((PasswordBox)sender).Password);
                ((dynamic)DataContext).Password = ((PasswordBox)sender).SecurePassword;
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Hide();
        }

    }
}
