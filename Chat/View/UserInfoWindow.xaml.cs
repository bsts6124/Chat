using System;
using System.Windows;
using System.Windows.Media.Imaging;

namespace Client
{
    /// <summary>
    /// UserInfoWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class UserInfoWindow : Window
    {
        public UserInfoWindow(string Name, string Position, string Team, string ID, string telNum, BitmapImage b)
        {
            InitializeComponent();
            UserInfoWindowViewModel v = new UserInfoWindowViewModel(Name, Position, Team, ID, telNum, b);
            if (v.CloseAction == null)
            {
                v.CloseAction = new Action(() => this.Close());
            }
        }


    }
}
