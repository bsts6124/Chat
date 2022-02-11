using System.Windows.Controls;

namespace Client
{
    /// <summary>
    /// ChattingList.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ChattingList : Page
    {
        public ChattingList()
        {
            InitializeComponent();
            DataContext = new ChattingListViewModel();
            MainManager.Instance.RefreshEvent += Refresh;
        }

        public void Refresh()
        {
            //list.Items.Refresh();
        }
    }
}
