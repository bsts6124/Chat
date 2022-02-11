using System;
using MySql.Data.MySqlClient;

namespace Server
{
    static class Program
    {
        static MySqlConnection ChatDB;
        static void Main(string[] args)
        {
            Server.Start();
            while(true)
            {
                string c = Console.ReadLine();
                if(c=="Quit")
                {
                    if (Server.isOn == true) Server.Stop();
                    //ChatDB.Close();
                    break;
                }
                switch(c)
                {
                    case "Start":
                        if (Server.isOn == false) Server.Start();
                        else Console.WriteLine("서버가 실행중인뎁쇼?");
                        break;
                    case "Stop":
                        if (Server.isOn == true) Server.Stop();
                        else Console.WriteLine("서버가 이미 멈춰있습니다.");
                        break;
                }
            }

        }
    }
}
