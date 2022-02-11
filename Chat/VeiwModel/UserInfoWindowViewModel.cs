using System;
using System.Windows.Media.Imaging;

namespace Client
{
    class UserInfoWindowViewModel
    {
        public string Name { get; set; }
        public string Position { get; set; }
        public string Team { get; set; }
        public string ID { get; set; }
        public string telNum { get; set; }
        public BitmapImage Profile { get; set; }
        public UserInfoWindowViewModel(string name, string pos, string team, string id, string telnum, BitmapImage b)
        {
            Name = name;
            Position = pos;
            Team = team;
            ID = id + "@consoto.com";
            telNum = telnum;
            Profile = b;
        }

        public Action CloseAction { get; set; }
    }
}
