using System;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using Command;

namespace Client
{
    public class MainWindowViewModel : ViewModelBase
    {
        public MainWindowViewModel()
        {
            CloseCommand = new Command.DelegateCommand(CloseButtonClick);
            CreateRoomCommand = new DelegateCommand(CreateButtonClick);
        }

        #region FriendList

        public void ListSelectionChange(object o, SelectionChangedEventArgs e)
        {
            ((ListView)o).SelectedItem = null;
        }

        public void ListDoubleClick(object o, MouseButtonEventArgs e)
        {

        }
        #endregion

        #region ChattingList

        public DelegateCommand CreateRoomCommand { get; set; }

        void CreateButtonClick(object o)
        {
            UserAddWindowViewModel vm = new UserAddWindowViewModel(-1);
            UserAddWindow UAW = new UserAddWindow(vm);
            UAW.ShowDialog();
        }

        #endregion

        TreeItems FindTeam(string name)
        {
            return MainManager.Instance.Charts.Where(z => z.Name == name).FirstOrDefault();
        }

        public Command.DelegateCommand CloseCommand { get; set; }

        void CloseButtonClick(object obj)
        {
            CloseAction();
        }

        public Action CloseAction { get; set; }
    }

    #region view
    //public class DateVisibilityConverter : IValueConverter
    //{
    //    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    //    {
    //        if (value is DateTime dt)
    //            return;
    //    }

    //    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    //    {
    //        throw new NotImplementedException();
    //    }
    //}

    public class TabWidthConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return double.Parse(value.ToString()) / 3;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class UniformTabPanel : UniformGrid
    {
        public UniformTabPanel()
        {
            this.IsItemsHost = true;
            this.Rows = 1;

            //Default, so not really needed..
            this.HorizontalAlignment = HorizontalAlignment.Stretch;
        }

        protected override Size MeasureOverride(Size constraint)
        {
            var totalMaxWidth = this.Children.OfType<TabItem>().Sum(tab => tab.MaxWidth);
            if (!double.IsInfinity(totalMaxWidth))
            {
                this.HorizontalAlignment = (constraint.Width > totalMaxWidth)
                                                    ? HorizontalAlignment.Left
                                                    : HorizontalAlignment.Stretch;
            }

            return base.MeasureOverride(constraint);
        }
    }
    #endregion
}