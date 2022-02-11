using Command;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.SQLite;
using System.Globalization;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Client
{
    public class ChattingWindowViewModel : ViewModelBase
    {
        int CurrentLogNum; //DB에서 받은 마지막 번호
        public int RoomNum;

        SQLiteConnection sqlCon;

        List<ChatUserInfo> roomUserList = new List<ChatUserInfo>();

        ~ChattingWindowViewModel()
        {
        }

        public List<ChatUserInfo> RoomUserList
        {
            get { return roomUserList; }
            set
            {
                roomUserList = value;
                OnPropertyChanged();
            }
        }

        public Size CurrentGridSize
        {
            set
            {
                MessageBoxWidth = value.Width - (double)51;
            }
        }
            
        double _messageBoxWidth;
        public double MessageBoxWidth
        {
            get { return _messageBoxWidth; }
            set
            {
                _messageBoxWidth = value;
                OnPropertyChanged();
            }
        }

        private string _roomName;
        public string RoomName
        {
            get { return _roomName; }
            set
            {
                _roomName = value;
                OnPropertyChanged();
            }
        }

        public ChattingWindowViewModel(int roomNum)
        {
            RoomNum = roomNum;
            RoomUserList.Add(new ChatUserInfo(MainManager.Instance.CurrentUserNum, MainManager.Instance.UserName, 0));

            if (roomNum < 1000000)
            {
                if(MainManager.Instance.FindUser(roomNum)!=null)
                RoomUserList.Add(new ChatUserInfo(roomNum, MainManager.Instance.FindUser(roomNum).Name, 0));
            }
            else
            {
                SQLiteCommand UserListcom = new SQLiteCommand("SELECT * FROM userlist", MainManager.Instance.dicChatDB[RoomNum]);
                SQLiteDataReader reader = UserListcom.ExecuteReader();

                while(reader.Read())
                {
                    if(MainManager.Instance.FindUser(reader.GetInt32(0)) != null)
                    {
                        TreeItems user = MainManager.Instance.FindUser(reader.GetInt32(0));
                        RoomUserList.Add(new ChatUserInfo(user.Num,user.Name, BitConverter.GetBytes(reader.GetInt32(1))[0]));
                    }
                    else
                    {
                        RoomUserList.Add(new ChatUserInfo(reader.GetInt32(0), reader.GetInt32(0).ToString(), 1));
                    }
                }
            }

            InitCommand();

            if (MainManager.Instance.dicChatDB.ContainsKey(roomNum))
            {
                sqlCon = MainManager.Instance.dicChatDB[roomNum];
                SQLiteCommand sqlcom = new SQLiteCommand("SELECT lognum FROM chatlogs ORDER BY ROWID DESC LIMIT 1 ", sqlCon);
                CurrentLogNum = Convert.ToInt32(sqlcom.ExecuteScalar());

                ReadLog(5);
            }
        }

        #region SendMessage
        public DelegateCommand SendMessageCommand { private get; set; }
        private string _message = null; 
        public string Message
        {
            get { return _message; }
            set
            {
                _message = value;
                OnPropertyChanged();
            }
        }

        public void SendMessage(object obj)
        {
            if (Encoding.UTF8.GetByteCount(_message) > 6000) { MessageBox.Show("내용이 너무 많아요"); return; }

            NetWork.SendMessagePacket m = new NetWork.SendMessagePacket(RoomNum, 0, Message);
            NetWork.NetWorkManager.Instance.Send(m);
            Message = "";

        }
        public bool CanSendMessage(object obj)
        {
            if (string.IsNullOrEmpty(_message)) return false;

            return true;
        }
        #endregion

        #region Log
        private ObservableCollection<Chat> _chattingLogList = new ObservableCollection<Chat>();
        public ObservableCollection<Chat> ChattingLogList
        {
            get { return _chattingLogList; }
            set
            {
                _chattingLogList = value;
                OnPropertyChanged();
            }
        }

        void ReadLog(int num)
        {
            int destnum = CurrentLogNum - num;
            if (CurrentLogNum < 1)
                return;

            string read = "SELECT * FROM chatlogs ORDER BY datetime DESC";

            //if (destnum < 0)
            //    read = "select * from Logs where Num between " + CurrentLogNum + " and " + 0;
            //else
            //    read = "select * from Logs where Num between " + CurrentLogNum + " and " + destnum;

            SQLiteCommand com = new SQLiteCommand(read, MainManager.Instance.dicChatDB[RoomNum]);
            SQLiteDataReader rdr = com.ExecuteReader();

            while (rdr.Read())
            {
                AddFront(roomUserList.Find(a => a.Num == rdr.GetInt32(5)), rdr.GetInt32(0), rdr.GetInt32(3), 0, rdr.GetString(2), rdr.GetString(6), (byte[])rdr[1]);;
            }
        }

        public void ReceiveMessage(NetWork.ReceiveMessagePacket e)
        {
            AddBack(roomUserList.Find(a => a.Num == e.Sender), ++CurrentLogNum, e.Type, 0, e.DateTime, e.fileData, e.Data);
            if (isAuto == true) ScrollToBottom();
        }
        #endregion

        #region MessageUtil
        void AddFront(ChatUserInfo sender, int logNum, int type, int read, string date, string filename, byte[] message)
        {
            Chat c = new Chat(sender, logNum, type, read, date, filename, message);
            if (ChattingLogList.Count == 0)
            {
                ChattingLogList.Add(new Chat(new ChatUserInfo(0, null, 0), 0, 0, 0, date, null, null));
                ChattingLogList.Add(c);
                return;
            }

            if (ChattingLogList[0].Sender.Num == sender.Num)
            {
                ChattingLogList[0].IsPersonChange = false;
                if (ChattingLogList[0].Date == c.Date)
                {
                    c.IsTimeChange = false;
                }
                else if (ChattingLogList[0].Date.Date != c.Date.Date)
                {
                    ChattingLogList.Insert(0, new Chat(new ChatUserInfo(0, null, 0), 0, 0, 0, date, null, null));
                }
                ChattingLogList.Insert(0, c);
            }
            else
            {
                ChattingLogList.Insert(0, c);
            }
        }

        void AddBack(ChatUserInfo sender,int logNum, int type, int read, string date, string filename, byte[] message)
        {
            Chat c = new Chat(sender, logNum, type, read, date, filename, message);

            if (ChattingLogList.Count == 0)
            {
                ChattingLogList.Add(new Chat(new ChatUserInfo(0, null, 0), 0, 0, 0, date, null, null));
                ChattingLogList.Add(c);
                return;
            }

            if (ChattingLogList[ChattingLogList.Count - 1].Sender.Num == sender.Num)
            {
                c.IsPersonChange = false;
                if (ChattingLogList[ChattingLogList.Count - 1].Date == c.Date)
                {
                    ChattingLogList[ChattingLogList.Count - 1].IsTimeChange = false;
                }
                else if (ChattingLogList[ChattingLogList.Count - 1].Date.Date != c.Date.Date)
                {
                    ChattingLogList.Add(new Chat(new ChatUserInfo(0, null, 0), 0, 0, 0, date, null, null));
                }
                ChattingLogList.Add(c);
            }
            else ChattingLogList.Add(c);
        }

        #endregion

        #region Control
        public DelegateCommand UserAddCommand { get; set; }

        void UserAddButtonClick(object o)
        {
            UserAddWindowViewModel vm = new UserAddWindowViewModel(RoomNum);
            UserAddWindow UAW = new UserAddWindow(vm);
            UAW.ShowDialog();
        }

        #endregion

        #region WindowControl
        public Command.DelegateCommand CloseCommand { get; set; }
        void CloseButtonClick(object o)
        {
            MainManager.Instance.dicChatWindows.Remove(RoomNum);
            CloseAction();
        }

        public bool isAuto { get; set; }

        public void IsScrollBottom(object o, ScrollChangedEventArgs e)
        {
            ScrollViewer scv = (ScrollViewer)o;
            if (scv.ScrollableHeight * 0.95 <= scv.VerticalOffset) isAuto = true;
            else isAuto = false;
            OnPropertyChanged("isAuto");
        }

        public DelegateCommand ScrollBottomCommand { get; set; }
        void ScrollBottomButtonClick(object o) { ScrollToBottom(); }

        void InitCommand()
        {
            CloseCommand = new DelegateCommand(CloseButtonClick);
            SendMessageCommand = new DelegateCommand(SendMessage, CanSendMessage);
            ScrollBottomCommand = new DelegateCommand(ScrollBottomButtonClick);
            UserAddCommand = new DelegateCommand(UserAddButtonClick);
        }

        public Action CloseAction { get; set; }

        public Action ScrollToBottom { get; set; }
        #endregion
    }

    #region views
    public class Chat : ViewModelBase
    {
        //public Chat(ChatUserInfo sender, int type, int read, string date, string message)
        //{
        //    Sender = sender;
        //    Read = read;
        //    Type = type;
        //    Date = DateTime.ParseExact(date, "dd/MM/yyyy HH:mm", null);
        //    Message = message;
        //}

        public Chat(ChatUserInfo sender,int logNum, int type, int read, string date, string filename, byte[] data)
        {
            LogNum = logNum;
            Sender = sender;
            Read = read;
            Type = type;
            FileName = filename;
            Date = DateTime.ParseExact(date, "dd/MM/yyyy HH:mm", null);
            switch (type)
            {
                case 0:
                    {
                        if (data != null)
                            Message = Encoding.UTF8.GetString(data);
                        else
                            Message = null;
                        break;
                    }
                case 1:
                    {
                        byte[] image = new byte[data.Length - 4];
                        Array.Copy(data, 4, image, 0, image.Length);
                        using (MemoryStream ms = new MemoryStream(image))
                        {
                            Image = new BitmapImage();
                            Image.BeginInit();
                            Image.StreamSource = ms;
                            Image.CacheOption = BitmapCacheOption.OnLoad;
                            Image.EndInit();
                        }
                        break;
                    }
                case 2:
                    {
                        break;
                    }
                case 50:
                    {
                        break;
                    }
            }
        }

        public ChatUserInfo Sender { get; set; }
        /// <summary>
        /// 0.message 1.img 2.files 3.emoticon
        /// </summary>
        public int Type { get; set; }
        public int LogNum;
        int _read;
        public int Read
        {
            get { return _read; }
            set
            {
                _read = value;
                OnPropertyChanged();
            }

        }
        public DateTime Date { get; set; }
        public string Message { get; set; }
        public BitmapImage Image { get; set; }
        public string FileName;

        bool _isPersonChange = true;
        public bool IsPersonChange
        {
            get { return _isPersonChange; }
            set
            {
                _isPersonChange = value;
                OnPropertyChanged();
            }
        }
        bool _isTimeChange = true;
        public bool IsTimeChange
        {
            get { return _isTimeChange; }
            set
            {
                _isTimeChange = value;
                OnPropertyChanged();
            }
        }
    }

    [ValueConversion(typeof(bool), typeof(System.Windows.Visibility))]
    public class BooleanToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (bool)value ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }

    [ValueConversion(typeof(bool), typeof(Brush))]
    public class ChatBubbleBackGroundConverter : IValueConverter
    {
        public Brush FalseBrush { get; set; } = new SolidColorBrush(Colors.White);
        public Brush TrueBrush { get; set; } = new SolidColorBrush(Colors.Yellow);

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return (bool)value ? TrueBrush : FalseBrush;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return value;
        }
    }

    [ValueConversion(typeof(bool), typeof(System.Windows.Visibility))]
    public class ChatTimeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (bool)value ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    [ValueConversion(typeof(bool), typeof(System.Windows.Visibility))]
    public class AutoScrollButtonVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (bool)value ? Visibility.Collapsed : Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class UTCtoLocalTimeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is DateTime dt)
                return dt.ToLocalTime();
            else
                return DateTime.Parse(value?.ToString()).ToLocalTime();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class ChatBubbleSizeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return double.Parse(value.ToString()) - 135;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class MessageBoxSizeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return double.Parse(value.ToString()) - 51;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class CurrentUserSelector : DataTemplateSelector
    {
        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            Chat c = (Chat)item;

            FrameworkElement e = container as FrameworkElement;

            if (c.Sender.Num == 0)
                return (DataTemplate)e.FindResource("DayChange");

            if (MainManager.Instance.CurrentUserNum == c.Sender.Num)
            {
                if (c.Type == 0)
                {
                    return (DataTemplate)e.FindResource("CurrentUserTemplate");
                }
                else if (c.Type == 1)
                {
                    return (DataTemplate)e.FindResource("CurrentUserImageTemplate");
                }
                else if (c.Type == 2)
                {

                }
                return (DataTemplate)e.FindResource("CurrentUserTemplate");
            }
            else
            {
                if (c.Type == 0)
                {
                    return (DataTemplate)e.FindResource("DifferentUserTemplate");
                }
                else if (c.Type == 1)
                {

                }
                else if (c.Type == 2)
                {

                }
                return (DataTemplate)e.FindResource("DifferentUserTemplate");
            }
        }
    }

    #endregion
}
