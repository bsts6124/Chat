using Command;
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;

namespace Client
{
    class ChattingListViewModel : ViewModelBase
    {
        public ChattingListViewModel()
        {


        }

        public void RightButtonClick(object o, MouseButtonEventArgs e)
        {
            ListViewItem lv = (ListViewItem)o;
            ChatListModel cm;

            ContextMenu menu = new ContextMenu();
            MenuItem _1 = new MenuItem();
            _1.Header = "채팅방 열기";
            _1.Command = ChattingCommand;
            //_1.CommandParameter = cm.RoomNum;
            menu.Items.Add(_1);
            menu.Items.Add(_1);
        }

        public void DoubleClick(object o, MouseButtonEventArgs e)
        {

        }

        DelegateCommand ChattingCommand { get; set; }

        void Chatting(object o)
        {

        }

        public void ListSelectionChange(object o, SelectionChangedEventArgs e)
        {
            ((ListView)o).SelectedItem = null;
        }

        private static DependencyObject GetDependencyObjectFromVisualTree(DependencyObject startObject, Type type)
        {
            var parent = startObject;
            while (parent != null)
            {
                if (type.IsInstanceOfType(parent))
                    break;
                parent = VisualTreeHelper.GetParent(parent);
            }
            return parent;
        }
    }



    public class ChatListItemWidthConverter : System.Windows.Data.IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return double.Parse(value.ToString()) - 28;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    [ValueConversion(typeof(int), typeof(System.Windows.Visibility))]
    public class IntToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (int)value != 0 ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }

    public class DaySelector : DataTemplateSelector
    {
        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            return null;
        }
    }
}
