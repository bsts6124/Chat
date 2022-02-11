using Command;
using System;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using OutLook = Microsoft.Office.Interop.Outlook;

namespace Client
{
    /// <summary>
    /// FriendList.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class FriendList : Page
    {
        public FriendList()
        {
            InitializeComponent();

            DataContext = new FreindListViewModel();
            ChattingCommand = new DelegateCommand(Chatting, null);
            UserInfoCommand = new DelegateCommand(UserInfo, null);
            MailCommand = new DelegateCommand(MailProto, null);
        }


        public void OnPreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            DependencyObject obj = e.OriginalSource as DependencyObject;
            TreeViewItem item = GetDependencyObjectFromVisualTree(obj, typeof(TreeViewItem)) as TreeViewItem;
            TreeItems ele = (TreeItems)item.Header;

            switch (ele.Tag)
            {
                case 0: //team
                    {
                        ContextMenu menu = new ContextMenu();


                        (sender as TreeViewItem).ContextMenu = menu;
                    }
                    break;
                case 1: //User
                    {
                        ContextMenu menu = new ContextMenu();

                        MenuItem _1 = new MenuItem();
                        _1.Header = "채팅하기";
                        _1.Command = ChattingCommand;
                        _1.CommandParameter = ele;
                        menu.Items.Add(_1);

                        menu.Items.Add(new Separator());

                        MenuItem _4 = new MenuItem();
                        _4.Header = "메일 보내기";
                        _4.Command = MailCommand;
                        _4.CommandParameter = ele;
                        menu.Items.Add(_4);

                        MenuItem _5 = new MenuItem();
                        _5.Header = "유저 정보";
                        _5.Command = UserInfoCommand;
                        _5.CommandParameter = ele;
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

        DelegateCommand ChattingCommand { get; set; }

        void Chatting(object obj)
        {
            MainManager.Instance.ShowWindow(((TreeItems)obj));
        }

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
            //System.Diagnostics.Process.Start("mailto:" + ((TreeItems)o).ID + "@consoto.com");
        }

        DelegateCommand UserInfoCommand { get; set; }

        void UserInfo(object o)
        {
            NetWork.RequestPacket r = new NetWork.RequestPacket(2, ((TreeItems)o).Num.ToString());
            NetWork.NetWorkManager.Instance.Send(r);
        }

        private void TreeViewItem_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (!((TreeViewItem)sender).IsSelected) return;
            DependencyObject obj = e.OriginalSource as DependencyObject;
            TreeViewItem item = GetDependencyObjectFromVisualTree(obj, typeof(TreeViewItem)) as TreeViewItem;
            TreeItems ele = (TreeItems)item.Header;

            if (ele.Tag == 1)
            {
                Chatting(ele);
            }
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

        private void TreeViewItem_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                DependencyObject obj = e.OriginalSource as DependencyObject;
                TreeViewItem item = GetDependencyObjectFromVisualTree(obj, typeof(TreeViewItem)) as TreeViewItem;
                TreeItems ele = (TreeItems)item.Header;

                if (ele.Tag == 1 || ele.Tag == 3 || ele.Tag == 4)
                {
                    MainManager.Instance.ShowWindow(ele);
                }
            }
        }

        //private void TreeViewItem_MouseDown(object sender, MouseButtonEventArgs e)
        //{
        //    DependencyObject obj = e.OriginalSource as DependencyObject;
        //    TreeViewItem item = GetDependencyObjectFromVisualTree(obj, typeof(TreeViewItem)) as TreeViewItem;
        //    TreeItems ele = (TreeItems)item.Header;

        //    if (e.ChangedButton == MouseButton.Right)
        //    {
        //        if (ele.Tag == "Team")
        //        {
        //            MenuItem _1 = new MenuItem();
        //            _1.Header = "추가";
        //            ContextMenu menu = new ContextMenu();
        //            menu.Items.Add(_1);
        //            (sender as TreeViewItem).ContextMenu = menu;
        //        }
        //        else if (ele.Tag == "User")
        //        {
        //            MenuItem _1 = new MenuItem();
        //            _1.Header = "채팅하기";
        //            _1.Command = ChattingCommand;
        //            _1.CommandParameter = ele;
        //            ContextMenu menu = new ContextMenu();
        //            menu.Items.Add(_1);
        //            (sender as TreeViewItem).ContextMenu = menu;
        //        }
        //    }
        //    else if(e.ChangedButton == MouseButton.Left)
        //    {
        //        if (e.ClickCount == 2)
        //        {
        //            if(ele.Tag == "User")
        //            {
        //                Chatting(ele);
        //            }
        //        }
        //    }
        //}

        //private void TreeViewItem_Expanded(object sender, RoutedEventArgs e)
        //{
        //    DependencyObject obj = e.OriginalSource as DependencyObject;
        //    TreeViewItem item = GetDependencyObjectFromVisualTree(obj, typeof(TreeViewItem)) as TreeViewItem;
        //    TreeItems ele = (TreeItems)item.Header;

        //    TreeItems t = new TreeItems(10032, "사원", "tsjeong", "정태설", "User");
        //    ele.Items.Clear();
        //    ele.Items.Add(t);


        //}

    }
}
