using System;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Command;
using OutLook = Microsoft.Office.Interop.Outlook;

namespace Client
{
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainWindow : Window
    {
        MainWindowViewModel vm;

        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainWindowViewModel();

            MainManager.Instance.RefreshEvent += ListRefresh;

            ChattingCommand = new DelegateCommand(Chatting, null);
            UserInfoCommand = new DelegateCommand(Userinfo, null);
            MailCommand = new DelegateCommand(MailProto, null);

            if (((dynamic)DataContext).CloseAction == null) ((dynamic)DataContext).CloseAction = new Action(() =>
            {
                NetWork.NetWorkManager.Instance.Send(new NetWork.QuitPacket());
                this.Close();
                Application.Current.MainWindow.Close();
                Environment.Exit(0);
            });

        }

        #region treeview
        private void TreeViewItem_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (!((TreeViewItem)sender).IsSelected) return;
            DependencyObject obj = e.OriginalSource as DependencyObject;
            TreeViewItem item = GetDependencyObjectFromVisualTree(obj, typeof(TreeViewItem)) as TreeViewItem;
            TreeItems ele = (TreeItems)item.Header;

            if (ele.Tag == 1)
            {
                Chatting(ele.Num);
            }
        }

        private void TreeViewItem_KeyDown(object sender, KeyEventArgs e)
        {

        }

        private void TreeViewItem_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            DependencyObject obj = e.OriginalSource as DependencyObject;
            TreeViewItem item = GetDependencyObjectFromVisualTree(obj, typeof(TreeViewItem)) as TreeViewItem;
            TreeItems ele = (TreeItems)item.Header;

            switch (ele.Tag)
            {
                case 0: //team
                    {
                        ContextMenu menu = new ContextMenu();

                        MenuItem _4 = new MenuItem
                        {
                            Header = "메일 보내기",
                            Command = MailCommand,
                            CommandParameter = ele
                        };
                        menu.Items.Add(_4);

                        (sender as TreeViewItem).ContextMenu = menu;
                    }
                    break;
                case 1: //User
                    {
                        ContextMenu menu = new ContextMenu();

                        MenuItem _1 = new MenuItem
                        {
                            Header = "채팅하기",
                            Command = ChattingCommand,
                            CommandParameter = ele.Num
                        };
                        menu.Items.Add(_1);

                        menu.Items.Add(new Separator());

                        MenuItem _4 = new MenuItem
                        {
                            Header = "메일 보내기",
                            Command = MailCommand,
                            CommandParameter = ele
                        };
                        menu.Items.Add(_4);

                        MenuItem _5 = new MenuItem
                        {
                            Header = "유저 정보",
                            Command = UserInfoCommand,
                            CommandParameter = ele
                        };
                        menu.Items.Add(_5);

                        (sender as TreeViewItem).ContextMenu = menu;
                    }
                    break;
                case 2: //Favorite Folder
                    break;
                case 3: //Favorite User
                    break;
                case 4: //Chatting
                    break;
            }
        }

        #region Chatting
        DelegateCommand ChattingCommand { get; set; }

        void Chatting(object o)
        {
            MainManager.Instance.ShowWindow(MainManager.Instance.FindUser((int)o));
        }
        #endregion

        #region UserInfo
        DelegateCommand UserInfoCommand { get; set; }

        void Userinfo(object o)
        {
            
        }
        #endregion

        #region Mail
        DelegateCommand MailCommand { get; set; }

        void MailProto(object o)
        {
            TreeItems t = (TreeItems)o;
            if (t.Tag == 1 || t.Tag == 3)
            {
                OutLook.Application oApp = new OutLook.Application();
                OutLook._MailItem oItem = (OutLook._MailItem)oApp.CreateItem(OutLook.OlItemType.olMailItem);
                oItem.To = t.ID + "@consoto.com;";
                oItem.Display(false);
            }
            else
            {
                OutLook.Application oApp = new OutLook.Application();
                OutLook._MailItem oItem = (OutLook._MailItem)oApp.CreateItem(OutLook.OlItemType.olMailItem);
                StringBuilder Reciever = new StringBuilder();
                for (int i = 0; i < t.Items.Count; i++)
                {
                    if (t.Items[i].Tag == 1 || t.Items[i].Tag == 3)
                    {
                        Reciever.Append(t.Items[i].ID);
                        Reciever.Append("@consoto.com;");
                    }
                }
                oItem.To = Reciever.ToString();
                oItem.Display(false);
            }
        }
        #endregion

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
        #endregion

        #region listview

        void ListViewItemMouseDoubleClick(object obj, MouseButtonEventArgs e)
        {
            ChatListModel cm = ((ListViewItem)obj).DataContext as ChatListModel;
            MainManager.Instance.ShowWindow(MainManager.Instance.FindUser(cm.RoomNum));
        }

        void ListRefresh()
        {
            list.Items.Refresh();
        }
        #endregion

        #region views
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            NetWork.NetWorkManager.Instance.Send(new NetWork.QuitPacket());
            Application.Current.MainWindow.Close();
            Environment.Exit(0);
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }

        #endregion
    }
}
