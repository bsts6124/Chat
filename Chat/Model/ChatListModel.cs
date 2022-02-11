using System;
using System.Windows.Media.Imaging;

namespace Client
{
    public class ChatListModel 
    {
        public BitmapSource ChatRoomImage { get; set; }
        public int NotReadLogs { get; set; }
        public int RoomNum { get; set; }
        public string RoomName { get; set; }
        public string LastLog { get; set; }
        public DateTime LastDate { get; set; }

        public ChatListModel(int logs, int num, string name, string log, string date)
        {
            NotReadLogs = logs;
            RoomNum = num;
            RoomName = name;
            LastLog = log;
            LastDate = DateTime.ParseExact(date, "dd/MM/yyyy HH:mm", null);
            ChatRoomImage = null;
        }

        public ChatListModel(int logs, int num, string name, string log, DateTime date)
        {
            NotReadLogs = logs;
            RoomNum = num;
            RoomName = name;
            LastLog = log;
            LastDate = date;
            ChatRoomImage = null;
        }

    }
}
