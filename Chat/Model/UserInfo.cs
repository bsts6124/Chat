using System.Windows.Media;

namespace Client
{
    class UserInfo
    {
        public UserInfo(int num, string position, string _ID, string name, string telNum, string team)
        {
            Num = num;
            Pos = position;
            ID = _ID;
            Name = name;
            TelNum = telNum;
            Team = team;
        }

        public ImageSource Profile { get; set; }
        public int Num { get; private set; }
        public string Pos { get; private set; }
        public string ID { get; private set; }
        public string Name { get; private set; }
        public string TelNum { get; private set; }
        public string Team { get; private set; }
    }

    public class ChatUserInfo
    {
        public ChatUserInfo(int num, string name, byte sta)
        {
            //Profile = s;
            Num = num;
            Name = name;
            State = sta;
        }
        //public ImageSource Profile { get; set; }
        public int Num { get; set; }
        public string Name { get; set; }
        public byte State { get; set; }
    }
}
