using System;
using System.Text;

namespace NetWork
{
    class Packet
    {
        public byte classKind;
    }

    ///////////////////SendPacket

    /// <summary>
    /// Packet num 0
    /// </summary>
    class LoginPacket : Packet
    {
        public string ID;
        public string PW;
        public LoginPacket(string id, string pw)
        {
            classKind = 0;
            ID = id;
            PW = pw;
        }
    }

    /// <summary>
    /// Packet num 1
    /// </summary>
    class RequestPacket : Packet
    {
        // 1 유저 정보 요청
        // 2 
        // 50 멀티챗 정보
        // 51 멀티챗 생성
        public byte reKind;
        public string Request;
        public RequestPacket(byte kind, string request)
        {
            reKind = kind;
            Request = request;
            classKind = 1;
        }
    }

    /// <summary>
    /// Packet num 2
    /// </summary>
    class SendMessagePacket : Packet
    {
        /// <summary>
        /// 0 = string  1 = image  2 = files 3 = imoticon 50 = userAdd
        /// </summary>
        public byte Type;
        public int roomNum;
        public byte[] Data;

        public SendMessagePacket(int room, byte type, string data)
        {
            classKind = 2;
            roomNum = room;
            Type = type;
            Data = Encoding.UTF8.GetBytes(data);
        }

        public SendMessagePacket(int room, byte type, byte[] data)
        {
            classKind = 2;
            roomNum = room;
            Type = type;
            Data = data;
        }
    }

    /// <summary>
    /// Packet num 99
    /// </summary>
    class PrevFilePacket : Packet
    {
        public byte[] fileSize = new byte[5];
        public PrevFilePacket(int FileSize)
        {
            classKind = 99;
            fileSize[0] = classKind;
            Array.Copy(BitConverter.GetBytes(FileSize), 0, fileSize, 1, 4);
        }
    }

    /// <summary>
    /// Packet num 100
    /// </summary>
    class FilePacket : Packet
    {
        public byte[] fileData;
        public FilePacket(byte[] f)
        {
            classKind = 100;
            fileData = new byte[f.Length + 1];
            fileData[0] = classKind;
            Array.Copy(f, 0, fileData, 1, f.Length);
        }
    }

    /// <summary>
    /// Packet num 255
    /// </summary>
    class QuitPacket : Packet
    {
        public QuitPacket()
        {
            classKind = 255;
        }
    }




    //////////////////////////// receivepacket

    //public class TeamInfoPacket
    //{
    //    public List<Client.TreeItems> TeamList;
    //}

    public class UserInfoPacket
    {

    }

    public class ReceiveMessagePacket : EventArgs
    {
        public byte Type;
        public int RoomNum;
        public byte[] Data;
        public int Sender;
        public string DateTime;
        public string fileData;
        public ReceiveMessagePacket(byte t, int rn, byte[] data, int sender, string date,string filedata)
        {
            Type = t;
            RoomNum = rn;
            Data = data;
            Sender = sender;
            DateTime = date;
            fileData = filedata;
        }
    }


}