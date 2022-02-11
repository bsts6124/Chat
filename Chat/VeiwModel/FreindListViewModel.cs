using Command;
using System;

namespace Client
{
    class FreindListViewModel : ViewModelBase
    {
        //DelegateCommand ChangeStateCommand {get; private set;}
        public FreindListViewModel()
        {
            FindUserCommand = new DelegateCommand(FindUser, CanFindUser);
            //ChattingCommand = new DelegateCommand(Chatting)
        }

        #region UserSearch Command
        public DelegateCommand FindUserCommand { get; private set; }
        private string _findName;
        public string FindName
        {
            get { return _findName; }
            set
            {
                _findName = value;
                OnPropertyChanged();
            }
        }

        public void FindUser(object obj)
        {
            //db where 
            //db select
            //query

        }

        public bool CanFindUser(object obj)
        {
            if (!String.IsNullOrEmpty(_findName)) return false;

            return true;
        }

        #endregion

        #region Add Group
        public DelegateCommand AddGroupCommand { get; private set; }
        //void AddGroupCommand = new DelegateCommand();
        #endregion

        //public void OnPreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        //{
        //    DependencyObject obj = e.OriginalSource as DependencyObject;
        //    TreeViewItem item = GetDependencyObjectFromVisualTree(obj, typeof(TreeViewItem)) as TreeViewItem;
        //    TreeItems ele = (TreeItems)item.Header;

        //    if (ele.Tag == "Team")
        //    {
        //        MenuItem _1 = new MenuItem();
        //        _1.Header = "추가";
        //        ContextMenu menu = new ContextMenu();
        //        menu.Items.Add(_1);
        //        (sender as TreeViewItem).ContextMenu = menu;
        //    }
        //    else if (ele.Tag == "User")
        //    {
        //        MenuItem _1 = new MenuItem();
        //        _1.Header = "채팅하기";
        //        ContextMenu menu = new ContextMenu();
        //        menu.Items.Add(_1);
        //        (sender as TreeViewItem).ContextMenu = menu;
        //    }
        //}

        //private static DependencyObject GetDependencyObjectFromVisualTree(DependencyObject startObject, Type type)
        //{
        //    var parent = startObject;
        //    while (parent != null)
        //    {
        //        if (type.IsInstanceOfType(parent))
        //            break;
        //        parent = VisualTreeHelper.GetParent(parent);
        //    }
        //    return parent;
        //}

    }
}
