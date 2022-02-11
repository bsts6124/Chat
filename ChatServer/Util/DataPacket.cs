using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    public interface Packet : IDisposable
    {
        public byte[] Serialize();
    }

    public class UserDataPacket : Packet
    {
        byte Result;
        int userNum;
        byte[] userName;
        public UserDataPacket(byte r , int n, string name)
        {
            Result = r;
            userNum = n;
            userName = Encoding.UTF8.GetBytes(name);
        }

        public void Dispose()
        {
            userName = null;
        }

        public byte[] Serialize()
        {
            byte[] s = new byte[6 + userName.Length];
            s[0] = 5;
            if (Result == 0)
            {
                s[1] = 0;
                Array.Copy(BitConverter.GetBytes(userNum), 0, s, 2, 4);
                Array.Copy(userName, 0, s, 6, userName.Length);
                return s;
            }
            else
            {
                s[1] = 1;
                return s;
            }
        }
    }

    public class UserChartPacket : Packet
    {
        public byte[] userChart;

        public byte[] Serialize()
        {
            return userChart;
        }
        public void Dispose()
        {
            userChart = null;
        }
    }

    public class MessagePacket : Packet
    {
        public byte Type;
        public int Sender;
        public int RoomNum;
        public string FileName;
        public byte[] Data;
        public byte[] ReceiveTime;
        
        public MessagePacket()
        {
            ReceiveTime = Encoding.UTF8.GetBytes(DateTime.Now.ToString("dd/MM/yyyy HH:mm"));
        }

        public void Dispose()
        {
            Data = null;
            ReceiveTime = null;
        }

        public byte[] Serialize()
        {
            List<byte[]> b = new List<byte[]>();
            b.Add(Data);
            b.Add(BitConverter.GetBytes(Sender));
            b.Add(BitConverter.GetBytes(FileName.Length));
            b.Add(Encoding.UTF8.GetBytes(FileName));
            b.Add(BitConverter.GetBytes(ReceiveTime.Length));
            b.Add(ReceiveTime);
            return b.SelectMany(a => a).ToArray();
        }
    }
}
