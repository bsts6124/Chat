using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    class UserData
    {
        public byte Tag;
        public int Num;
        public string Pos;
        public string ID;
        public string PW;
        public string Name;
        public string Team;

        public UserData(string id, string pw, int num,string name, byte tag, string pos,string team)
        {
            ID = id;
            PW = pw;
            Num = num;
            Name = name;
            Tag = tag;
            Pos = pos;
            Team = team;
        }

        public UserData(string name, string team)
        {
            Tag = 0;
            Name = name;
            Team = team;
        }

        public byte[] Serialize()
        {
            List<byte[]> m = new List<byte[]>();
            if (Tag == 1)
            {
                m.Add(new byte[] { Tag });
                m.Add(BitConverter.GetBytes(Num));
                byte[] p = Encoding.UTF8.GetBytes(Pos);
                m.Add(BitConverter.GetBytes(p.Length));
                m.Add(p);
                byte[] id = Encoding.UTF8.GetBytes(ID);
                m.Add(BitConverter.GetBytes(id.Length));
                m.Add(id);
                byte[] name = Encoding.UTF8.GetBytes(Name);
                m.Add(BitConverter.GetBytes(name.Length));
                m.Add(name);
                byte[] team = Encoding.UTF8.GetBytes(Team);
                m.Add(BitConverter.GetBytes(team.Length));
                m.Add(team);
            }
            else if (Tag == 0)
            {
                m.Add(new byte[] { Tag });
                byte[] name = Encoding.UTF8.GetBytes(Name);
                m.Add(BitConverter.GetBytes(name.Length));
                m.Add(name);
                byte[] team = Encoding.UTF8.GetBytes(Team);
                m.Add(BitConverter.GetBytes(team.Length));
                m.Add(team);
            }
            return m.SelectMany(a=>a).ToArray();
        }
    }
}
