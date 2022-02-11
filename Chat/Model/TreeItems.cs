using System.Collections.ObjectModel;

namespace Client
{
    public class Item
    {
        /// <summary>
        /// 0 = team, 1 = user, 2 = favorite folder, 3 = favorite user, 4 = favorite chat
        /// </summary>
        public byte Tag { get; set; }
        public int Num { get; set; }
        public string Pos { get; set; }
        public string ID { get; set; }
        public string Name { get; set; }
        public byte State { get; set; } = 0;
        public string Team { get; set; }
    }

    public class TreeItems : Item
    {
        /// <summary>
        /// 유저
        /// </summary>
        public TreeItems(int num, string pos, string id, string name, byte tag, string team)
        {
            Num = num;
            Pos = pos;
            ID = id;
            Name = name;
            Tag = tag;
            Title = Name + "_" + ID + " " + "[" + pos + "]";
            Team = team;
        }

        /// <summary>
        /// 팀
        /// </summary>
        public TreeItems(string name, byte tag)
        {
            Items = new ObservableCollection<TreeItems>();
            Title = name;
            Tag = tag;
        }

        /// <summary>
        /// 채팅
        /// </summary>
        public TreeItems(string name, int roomNum)
        {
            Title = name;
            Num = roomNum;
            Tag = 4;
        }

        public TreeItems DeepCopy()
        {
            return new TreeItems(this.Num, this.Pos, this.ID, this.Name, 3, this.Team);
        }

        public string Title { get; set; }

        public ObservableCollection<TreeItems> Items { get; set; }
    }

}
