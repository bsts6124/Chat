using Command;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Data;

namespace Client
{
    public class UserAddWindowViewModel : ViewModelBase
    {
        public int RoomNum { get; set; }

        private string RoomName { get; set; }
        public string roomName { get { return RoomName; } set { RoomName = value; } }

        public UserAddWindowViewModel(int roomNum)
        {
            RoomNum = roomNum;

            AddCommand = new DelegateCommand(AddButtonClick);
            DeleteCommand = new DelegateCommand(DeleteButtonClick);
            ApplyCommand = new DelegateCommand(ApplyButtonClick, CanApply);
            CancleCommand = new DelegateCommand(CancleButtonClick);
            SelectedTreeItemChangedCommand = new DelegateCommand(SelectedTreeItemChange);
            SelectedListItemChangedCommand = new DelegateCommand(SelectedListItemChange);
        }

        private ObservableCollection<TreeItems> AddUserCollection = new ObservableCollection<TreeItems>();
        public ObservableCollection<TreeItems> AddUsers
        {
            get { return AddUserCollection; }
            set
            {
                AddUserCollection = value;
                OnPropertyChanged();
            }
        }

        #region Buttons
        public DelegateCommand AddCommand { get; set; }

        void AddButtonClick(object o)
        {
            if (SelectTreeItem == null) return;

            TreeItems t = AddUsers.ToList().Find(a => a == SelectTreeItem);
            if (t == null)
            {
                AddUsers.Add(SelectTreeItem);
                OnPropertyChanged("AddUsers");
            }
        }

        public DelegateCommand DeleteCommand { get; set; }

        void DeleteButtonClick(object o)
        {
            if (SelectListItem == null) return;

            TreeItems t = AddUserCollection.ToList().Find(a => a == SelectListItem);
            if (t != null)
            {
                SelectListItem = null;
                AddUsers.Remove(t);
            }
        }

        public DelegateCommand ApplyCommand { get; set; }

        void ApplyButtonClick(object o)
        {
            // new 0 = RoomNum, 1 = RoomName, 2 < UserNum
            // invite 0 = RoomNum, 1 < UserNum
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            sb.Append(RoomNum.ToString());
            sb.Append(";");

            if(RoomNum == -1)
            {
                sb.Append(RoomName);
                sb.Append(";");
                sb.Append(MainManager.Instance.CurrentUserNum.ToString());
            }

            for (int i = 0; i < AddUsers.Count; i++)
            {
                sb.Append(";");
                sb.Append(AddUsers[i].Num.ToString());
            }
            //NetWork.RequestPacket rp = new NetWork.RequestPacket(51, sb.ToString());
            NetWork.NetWorkManager.Instance.Send(new NetWork.RequestPacket(51, sb.ToString()));
            CloseAction();
        }

        bool CanApply(object o)
        {
            if (AddUsers.Any()) return true;
            else return false;
        }

        public DelegateCommand CancleCommand { get; set; }

        void CancleButtonClick(object o)
        {
            CloseAction();
        }

        public Action CloseAction { get; set; }

        TreeItems SelectTreeItem;
        public DelegateCommand SelectedTreeItemChangedCommand { get; set; }

        void SelectedTreeItemChange(object o)
        {
            SelectTreeItem = (TreeItems)o;
        }

        TreeItems SelectListItem;
        public DelegateCommand SelectedListItemChangedCommand { get; set; }
        void SelectedListItemChange(object o)
        {
            if (((IList<object>)o).Count != 0)
                SelectListItem = (TreeItems)((IList<object>)o)[0];
        }

        #endregion

    }

    [ValueConversion(typeof(bool), typeof(System.Windows.Visibility))]
    public class GridVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (int)value > 0 ? Visibility.Collapsed : Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class ListHeightConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (int)value > 0 ? (double)360 : (double)340;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class AddWindowFindBoxMarginConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (int)value > 0 ? new Thickness(20, 0, 0, 20) : new Thickness(20, 0, 0, 40);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class AddWindowButtonMarginConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (int)value > 0 ? new Thickness(45, 15, 45, 15) : new Thickness(45, 35, 45, 35);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
