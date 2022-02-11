using System;
using System.Windows;

namespace Client
{
    /// <summary>
    /// UserAddWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class UserAddWindow : Window
    {
        public UserAddWindow(UserAddWindowViewModel vm)
        {
            InitializeComponent();
            DataContext = vm;
            if (vm.CloseAction == null) vm.CloseAction = new Action(() => { this.Close(); });
        }
    }
}
