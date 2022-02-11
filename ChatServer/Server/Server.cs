using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;
using System.Threading;
using System.Linq;
using MySql.Data.MySqlClient;

namespace Server
{
    static class Server
    {
        public static bool isOn;

        static Socket Listener;
        static public SocketAsyncEventArgsPool EventPool;

        static public Dictionary<int, Client> dicConnectUser = new Dictionary<int, Client>();
        static string CurrentClientVersion = "a";
        static public UserChartPacket Chartpacket = new UserChartPacket();

        public const int BufferSize = 524288;

        static public MySqlConnection ChatDB;

        #region test
        static public List<UserData> listData = new List<UserData>();
        #endregion

        static Server()
        {
            ChatDB = new MySqlConnection("SERVER=localhost;DATABASE=chattest;UID=root;PASSWORD=Password@123;");
            try
            {
                ChatDB.Open();
            }
            catch
            {
                Console.WriteLine("DB연결 오류");
            }
            //유저목록 DB 읽어오기
            GetUserChart();
            //채팅 DB 연결
            isOn = false;
            EventPool = new SocketAsyncEventArgsPool(50);
        }

        public static void Start()
        {
            Console.WriteLine("시작");
            Listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            Listener.Bind(new System.Net.IPEndPoint(System.Net.IPAddress.Any, 10000));
            Console.WriteLine("바인딩");
            Listener.Listen(20);
            Console.WriteLine("리스닝");

            SocketAsyncEventArgs s = new SocketAsyncEventArgs();
            s.Completed += new EventHandler<SocketAsyncEventArgs>(Accept);
            Listener.AcceptAsync(s);
            Console.WriteLine("받는중");

            isOn = true;
        }

        public static void Stop()
        {
            Listener.Close();
            isOn = false;
        }

        private static void GetUserChart()
        {
            List<byte[]> userArray = new List<byte[]>();
            userArray.Add(new byte[] { 0 });
            foreach(UserData u in listData)
            {
                userArray.Add(u.Serialize());
            }
            Chartpacket.userChart = userArray.SelectMany(a => a).ToArray();
        }

        static void Accept(object o, SocketAsyncEventArgs e)
        {
            e.Completed -= Accept;

            Client c = new Client(e.AcceptSocket);
            Console.WriteLine("연결 시작");
            c.BeginRecieve(EventPool.Pop());

            byte[] ver = Encoding.UTF8.GetBytes(CurrentClientVersion);
            SocketAsyncEventArgs s = EventPool.Pop();
            s.SetBuffer(ver, 0, ver.Length);
            c.Send(s);

            e.AcceptSocket = null;
            e.Completed += new EventHandler<SocketAsyncEventArgs>(Accept);
            if (Listener.AcceptAsync(e) == false)
            {
                Accept(o, e);
            }
        }

        //static void InsertLog()
        //{
        //    MySqlCommand comInsert = new MySqlCommand();
        //    comInsert.Connection = ChatDB;
        //    comInsert.CommandText = "INSERT INTO logs (roomnum, type, receivedate, message, sender, data) VALUES (@rn, @type, @date, @msg, @sender, @data)";

        //    comInsert.Parameters.Add("@rn", MySqlDbType.Int32);
        //    comInsert.Parameters.Add("@type", MySqlDbType.Byte);
        //    comInsert.Parameters.Add("@date", MySqlDbType.DateTime);
        //    comInsert.Parameters.Add("@msg", MySqlDbType.Text);
        //    comInsert.Parameters.Add("@sender", MySqlDbType.Int32);
        //    comInsert.Parameters.Add("@data", MySqlDbType.Text);

        //    comInsert.Parameters[].Value =
        //    try
        //    {
        //        comInsert.ExecuteNonQuery();
        //    }
        //    catch
        //    {
        //        Console.WriteLine("채팅 데이터 입력 불가");
        //    }
            
        //}

    }

    class SocketAsyncEventArgsPool
    {
        Stack<SocketAsyncEventArgs> ArgsStack = new Stack<SocketAsyncEventArgs>();

        public SocketAsyncEventArgsPool()
        {
            for (int i = 0; i < 30; i++)
            {
                SocketAsyncEventArgs s = new SocketAsyncEventArgs();
                //s.SetBuffer(new byte[bufferSize], 0, bufferSize);
                ArgsStack.Push(s);
            }
        }

        public SocketAsyncEventArgsPool(int num)
        {
            for (int i = 0; i < num; i++)
            {
                SocketAsyncEventArgs s = new SocketAsyncEventArgs();
                //s.SetBuffer(new byte[bufferSize], 0, bufferSize);
                ArgsStack.Push(s);
            }
        }

        public SocketAsyncEventArgs Pop()
        {
            SocketAsyncEventArgs s;
            if (ArgsStack.Count == 0)
            {
                s = new SocketAsyncEventArgs();
            }
            else
            {
                s = ArgsStack.Pop();
            }
            s.SetBuffer(new byte[Server.BufferSize], 0, Server.BufferSize);
            return s;
        }

        public void Push(SocketAsyncEventArgs e)
        {
            e.SetBuffer(null, 0, 0);
            ArgsStack.Push(e);
        }

        ~SocketAsyncEventArgsPool()
        {
            ArgsStack.Clear();
        }
    }

    class BufferPool
    {
        public BufferPool()
        {

        }


    }
}
