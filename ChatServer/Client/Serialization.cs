using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;

namespace Server
{
    static class Serialization
    {
        static public byte[] Serialize(Packet p)
        {
            return null;
        }

        static public void Deserialize(byte[] ba, Client c)
        {
            int byIter = 1;
            switch(ba[0])
            {
                case (byte)0: //로그인 요청
                    {
                        int NextVarLength = BitConverter.ToInt32(ba, byIter);
                        byIter += 4;
                        byte[] id = new byte[NextVarLength];
                        Array.Copy(ba, byIter, id, 0, NextVarLength);
                        string ID = Encoding.UTF8.GetString(id);
                        byIter += NextVarLength;

                        NextVarLength = BitConverter.ToInt32(ba, byIter);
                        byIter += 4;
                        byte[] pw = new byte[NextVarLength];
                        Array.Copy(ba, byIter, pw, 0, NextVarLength);
                        string PW = Encoding.UTF8.GetString(pw);
                        byIter += NextVarLength;

                        try
                        {
                            UserData u = Server.listData.Find(a => String.Compare(a.ID, ID, true) == 0);

                            if(u==null)
                            {
                                UserDataPacket p = new UserDataPacket(1, 0, "a");
                                c.Send(p);
                                break;
                            }
                            if(u.PW == PW)
                            {
                                UserDataPacket p = new UserDataPacket(0,u.Num, u.Name);
                                c.Send(p);

                                //만약 서버에 같은 계정이 있을경우
                                if (Server.dicConnectUser.ContainsKey(u.Num))
                                {
                                    Server.dicConnectUser[u.Num] = c;
                                    c.CurrentUser = u.Num;
                                }
                                else
                                {
                                    Server.dicConnectUser.Add(u.Num, c);
                                    c.CurrentUser = u.Num;
                                }

                                c.Send(Server.Chartpacket);
                            }
                            else
                            {
                                UserDataPacket p = new UserDataPacket(1, 0, "a");
                                c.Send(p);
                            }
                        }
                        catch
                        {
                        }

                        break;
                    }
                case 1: // 기타 요청
                    {
                        switch(ba[byIter])
                        {
                            case 1:
                                break;
                            case 2:
                                break;
                            case 3: 
                                break;
                            case 51: //단체방 정보
                                {
                                    ++byIter;
                                    string rq = Encoding.UTF8.GetString(ba, 2, ba.Length - 2);
                                    string[] rqb = rq.Split(';');
                                    int[] Userlist = new int[rqb.Length];
                                    int roomnum;
                                    string roomname;

                                    MySqlCommand addUser = new MySqlCommand("INSERT INTO  roomuserlist (roomnum, roomname, usernum) VALUES (@rnum, @rname, @unum)", Server.ChatDB);
                                    addUser.Parameters.Add("@rnum", MySqlDbType.Int32);
                                    addUser.Parameters.Add("@rname", MySqlDbType.VarChar, 20);
                                    addUser.Parameters.Add("@unum", MySqlDbType.Int32);

                                    if (Int32.Parse(rqb[0]) < 0)
                                    {
                                        //db에 방 추가
                                        roomname = rqb[1];
                                        string Roomsql = "INSERT INTO roomlist (roomname) VALUES ('" + roomname + "')";
                                        MySqlCommand AddRoom = new MySqlCommand(Roomsql, Server.ChatDB);
                                        AddRoom.ExecuteNonQuery();
                                        roomnum = (int)AddRoom.LastInsertedId;
                                        addUser.Parameters[0].Value = roomnum;
                                        for (int i = 2; i < rqb.Length; i++)
                                        {
                                            Userlist[i] = Int32.Parse(rqb[i]);
                                            //DB에 유저 추가
                                            addUser.Parameters[1].Value = rqb[1];
                                            addUser.Parameters[2].Value = Int32.Parse(rqb[i]);

                                            addUser.ExecuteNonQuery();
                                        }
                                    }
                                    else
                                    {
                                        roomnum = int.Parse(rqb[0]);
                                        //기존 방 이름 가지고 오기
                                        MySqlCommand getRoomcom = new MySqlCommand("SELECT roomname FROM roomlist WHERE roomnum = " + roomnum.ToString() + ";",Server.ChatDB);
                                        roomname = (string)getRoomcom.ExecuteScalar();
                                        addUser.Parameters[0].Value = roomnum;
                                        for (int i = 1; i<rqb.Length;i++)
                                        {
                                            Userlist[i] = Int32.Parse(rqb[i]);
                                            //DB에 유저 추가
                                            addUser.Parameters[1].Value = roomname;
                                            addUser.Parameters[2].Value = Int32.Parse(rqb[i]);

                                            addUser.ExecuteNonQuery();
                                        }
                                    }
                                    //방생성 메시지?
                                    //채팅 메시지 전송
                                    //byte classkind = 2;
                                    //MessagePacket smp = new MessagePacket

                                    byte[] roomnamearr = Encoding.UTF8.GetBytes(roomname);
                                    int namelength = roomnamearr.Length;

                                    byte[] AddRoomByteArray = new byte[6 + namelength + Userlist.Length * 4];
                                    
                                    AddRoomByteArray[0] = 50;
                                    AddRoomByteArray[1] = Convert.ToByte(namelength);
                                    Array.Copy(BitConverter.GetBytes(roomnum), 0, AddRoomByteArray, 2, 4);
                                    Array.Copy(roomnamearr, 0, AddRoomByteArray, 6, namelength);
                                    for(int i = 0; i<namelength; i++)
                                    {
                                        Array.Copy(BitConverter.GetBytes(Userlist[i]), 0, AddRoomByteArray, 2 + namelength + i * 4, 4);
                                    }
                                    //방 정보 전송
                                    for(int i = 0;i<Userlist.Length; i++)
                                    {
                                        if(Server.dicConnectUser.ContainsKey(Userlist[i]))
                                        {
                                            Server.dicConnectUser[Userlist[i]].Send(AddRoomByteArray);
                                        }
                                    }



                                    break;
                                }
                        }
                    }
                    break;
                case 2: // 채팅
                    {
                        MessagePacket m = new MessagePacket();

                        MySqlCommand comInsert = new MySqlCommand();
                        comInsert.Connection = Server.ChatDB;
                        comInsert.CommandText = "INSERT INTO logs (roomnum, type, receivedate, message, sender, data) VALUES (@rn, @type, @date, @msg, @sender, @data)";

                        comInsert.Parameters.Add("@rn", MySqlDbType.Int32);
                        comInsert.Parameters.Add("@type", MySqlDbType.Byte);
                        comInsert.Parameters.Add("@date", MySqlDbType.Blob);
                        comInsert.Parameters.Add("@msg", MySqlDbType.MediumBlob);
                        comInsert.Parameters.Add("@sender", MySqlDbType.Int32);
                        comInsert.Parameters.Add("@data", MySqlDbType.Text);

                        switch (ba[byIter])
                        {
                            case 0: //일반 텍스트 메시지
                                {
                                    comInsert.Parameters[1].Value = ba[byIter];

                                    m.Data = ba;
                                    comInsert.Parameters[3].Value = ba;

                                    m.Sender = c.CurrentUser;
                                    comInsert.Parameters[4].Value = m.Sender;

                                    ++byIter;
                                    m.RoomNum = BitConverter.ToInt32(ba, byIter);
                                    comInsert.Parameters[0].Value = m.RoomNum;

                                    comInsert.Parameters[2].Value = m.ReceiveTime;
                                    comInsert.Parameters[5].Value = null;

                                    m.FileName = "";
                                    break;
                                }
                            case 1: //이미지
                                {
                                    comInsert.Parameters[1].Value = ba[byIter];

                                    m.Data = ba;
                                    comInsert.Parameters[3].Value = ba;

                                    m.Sender = c.CurrentUser;
                                    comInsert.Parameters[4].Value = m.Sender;

                                    ++byIter;
                                    m.RoomNum = BitConverter.ToInt32(ba, byIter);
                                    comInsert.Parameters[0].Value = m.RoomNum;

                                    comInsert.Parameters[2].Value = m.ReceiveTime;

                                    comInsert.Parameters[5].Value = c.fStream.Name;
                                    m.FileName = c.fStream.Name;
                                    c.fStream.Flush();
                                    break;
                                }
                            /*case 1: //이미지
                                {
                                    m.Data = ba;
                                    m.Sender = c.CurrentUser;
                                    ++byIter;
                                    m.RoomNum = BitConverter.ToInt32(ba, byIter);

                                    break;
                                }*/
                            case 2: //파일
                                {

                                    comInsert.Parameters[1].Value = ba[byIter];

                                    m.Data = ba;
                                    comInsert.Parameters[3].Value = ba;

                                    m.Sender = c.CurrentUser;
                                    comInsert.Parameters[4].Value = m.Sender;

                                    ++byIter;
                                    m.RoomNum = BitConverter.ToInt32(ba, byIter);
                                    comInsert.Parameters[0].Value = m.RoomNum;

                                    comInsert.Parameters[2].Value = m.ReceiveTime;

                                    comInsert.Parameters[5].Value = c.fStream.Name;
                                    m.FileName = c.fStream.Name;
                                    c.fStream.Flush();
                                    break;
                                }
                            case 3:
                                {

                                    break;
                                }
                        }
                        Client rec;

                        if (c.CurrentUser == m.RoomNum)
                        {

                        }
                        else if (m.RoomNum < 1000000)
                        {
                            if (Server.dicConnectUser.TryGetValue(m.RoomNum, out rec) == true)
                            {
                                rec.Send(m);
                            }
                        }
                        else
                        {

                        }

                        c.Send(m);

                        try
                        {
                            comInsert.ExecuteNonQuery();
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.Message);
                        }

                        m.Dispose();
                        break;
                    }

                case 99:
                    {
                        c.fileSize = BitConverter.ToInt32(ba,1);
                        c.fStream = new FileStream("C:\\Test\\"+c.CurrentUser.ToString()+DateTime.Now.ToBinary().ToString(), FileMode.Create);
                        break;
                    }

                case 100:
                    {
                        c.fStream.Seek(0, SeekOrigin.End);
                        c.fStream.Write(ba, 1, ba.Length - 1);
                        c.fileSize -= ba.Length - 1;
                        break;
                    }

                case 255: // 종료
                    {
                        System.Threading.Thread.Sleep(1000);
                        c.Disconnect();
                        break;
                    }
            }
        }
    }

}
