using System;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;



namespace Client
{
    /// <summary>
    /// ChattingWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ChattingWindow : Window
    {
        public delegate void delReceive(NetWork.ReceiveMessagePacket e);
        public event delReceive ReceiveEvent;


        ChattingWindowViewModel cwvm;
        public ChattingWindow(ChattingWindowViewModel cw)
        {
            InitializeComponent();
            cwvm = cw;
            DataContext = cw;
            if (((dynamic)DataContext).CloseAction == null) ((dynamic)DataContext).CloseAction = new Action(() => { ReceiveEvent -= cw.ReceiveMessage; this.Close(); });
            ScrollViewer sc = (ScrollViewer)this.FindName("LogScrollViewer");
            if (((dynamic)DataContext).ScrollToBottom == null) ((dynamic)DataContext).ScrollToBottom = new Action(() => { sc.ScrollToBottom(); });
        }

        private void Grid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }

        private void ListViewItem_Selected(object sender, RoutedEventArgs e)
        {
            DependencyObject obj = e.OriginalSource as DependencyObject;
            ListViewItem i = GetDependencyObjectFromVisualTree(obj, typeof(ListViewItem)) as ListViewItem;
            i.IsSelected = false;
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

        private void Grid_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (DataContext != null)
            {
                ((dynamic)DataContext).CurrentGridSize = e.NewSize;
            }
        }

        private void Grid_Loaded(object sender, RoutedEventArgs e)
        {
            if (DataContext != null)
            {
                ((dynamic)DataContext).CurrentGridSize = ((Grid)sender).RenderSize;
            }
        }

        private void ScrollViewer_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            ScrollViewer scv = (ScrollViewer)sender;
            scv.ScrollToVerticalOffset(scv.VerticalOffset - e.Delta);
            e.Handled = true;
        }

        public void ReceiveMessage(NetWork.ReceiveMessagePacket e)
        {
            ReceiveEvent(e);
        }

        void CloseWindow(object sender, CancelEventArgs e)
        {
            int k = MainManager.Instance.dicChatWindows.FirstOrDefault(a => a.Value == this).Key;
            MainManager.Instance.dicChatWindows.Remove(k);
        }


        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.V && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
            {
                if (Clipboard.ContainsImage())
                {
                    IDataObject clipboardData = Clipboard.GetDataObject();
                    if (clipboardData != null)
                    {
                        try
                        {
                            SendImageWindow s = new SendImageWindow(cwvm.RoomNum);
                            s.Owner = GetWindow(this);
                            s.Activate();
                            s.Show();
                        }
                        catch
                        {
                            MessageBox.Show("클립보드 이미지 창 오류");
                        }
                        clipboardData = null;
                    }
                }
            }
        }

        private void Window_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                FileDropWindowViewModel fw = new FileDropWindowViewModel(files, cwvm.RoomNum);
                FileDropWindow f = new FileDropWindow();
                f.DataContext = fw;
                if (((dynamic)f.DataContext).CloseAction == null) ((dynamic)f.DataContext).CloseAction = new Action(() => { f.Close(); });
                f.ShowDialog();
            }
        }

        private void Image_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {

        }

        private void Window_Activated(object sender, EventArgs e)
        {
            ChatListModel clm = MainManager.Instance.colChatList.Where(x => x.RoomNum == cwvm.RoomNum).FirstOrDefault();
            if (clm != null)
            {
                clm.NotReadLogs = 0;
                System.Data.SQLite.SQLiteCommand ReadLogcom = new System.Data.SQLite.SQLiteCommand("UPDATE chattinglist SET notreadlog = 0 WHERE roomnum = " + cwvm.RoomNum, MainManager.Instance.ChattingListDB);
            }
        }

        private void LogScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if (e.ExtentHeightChange != 1)
                (sender as ScrollViewer).ScrollToVerticalOffset(e.VerticalOffset + e.ExtentHeightChange);
        }
    }
}
