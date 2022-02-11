using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Media.Imaging;

namespace NetWork
{
    static class Serialization
    {
        public delegate void delLogIn(Client.ResultArgs e);
        public static event delLogIn LogInEvent;

        static public byte[] Serialize(Packet o)
        {
            List<byte[]> LByteArr = new List<byte[]>();

            switch (o.classKind)
            {
                case 0: //로그인 정보
                    {
                        LoginPacket l = (LoginPacket)o;
                        byte[] b = { 0 };
                        LByteArr.Add(b);

                        byte[] id = Encoding.UTF8.GetBytes(l.ID);
                        LByteArr.Add(BitConverter.GetBytes(id.Length));
                        LByteArr.Add(id);

                        byte[] pw = Encoding.UTF8.GetBytes(l.PW);
                        LByteArr.Add(BitConverter.GetBytes(pw.Length));
                        LByteArr.Add(pw);
                    }
                    break;

                case 1: //요청 패킷
                    {
                        RequestPacket r = (RequestPacket)o;
                        byte[] b = { 1, r.reKind };
                        LByteArr.Add(b);
                        LByteArr.Add(Encoding.UTF8.GetBytes(r.Request));
                    }
                    break;

                case 2: //메시지 전송
                    {
                        SendMessagePacket s = (SendMessagePacket)o;
                        byte[] b = { 2, s.Type };
                        LByteArr.Add(b);
                        LByteArr.Add(BitConverter.GetBytes(s.roomNum));
                        LByteArr.Add(BitConverter.GetBytes(s.Data.Length));
                        LByteArr.Add(s.Data);
                    }
                    break;

                case 99:
                    {
                        return ((PrevFilePacket)o).fileSize;
                    }
                case 100: //파일 전송
                    {
                        //FilePacket f = (FilePacket)o;
                        return ((FilePacket)o).fileData;
                    }

                case 255: //접속 종료
                    {
                        byte[] b = { 255 };
                        LByteArr.Add(b);
                    }
                    break;
            }

            byte[] serByteArr = LByteArr.SelectMany(a => a).ToArray();

            //byte[] sendByte = new byte[serByteArr.Length + 4];

            //Array.Copy(BitConverter.GetBytes(serByteArr.Length), sendByte, 4);
            //Array.Copy(serByteArr, 0, sendByte, 4, serByteArr.Length);

            return serByteArr;
        }

        static public void DeSerialize(byte[] ba)
        {
            int byIter = 1;
            switch (ba[0])
            {
                case 0: //친구목록
                    {
                        while (byIter < ba.Length)
                        {
                            switch (ba[byIter])
                            {
                                case (byte)0: //팀
                                    {
                                        //다음 바이트
                                        byte tag = 0;
                                        ++byIter;
                                        //name 길이 받기
                                        int NextVarLength = BitConverter.ToInt32(ba, byIter);
                                        //다음 바이트
                                        byIter += 4;
                                        //name 할당(ba에서 byIter부터 시작해 NextVarLength만큼)
                                        string name = Encoding.UTF8.GetString(ba, byIter, NextVarLength);
                                        //다음 바이트
                                        byIter += NextVarLength;
                                        NextVarLength = BitConverter.ToInt32(ba, byIter);
                                        byIter += 4;
                                        Client.TreeItems t = new Client.TreeItems(name, tag);
                                        string team = Encoding.UTF8.GetString(ba, byIter, NextVarLength);
                                        byIter += NextVarLength;

                                        //소속팀이 없을경우
                                        if (team == "X")
                                        {
                                            Client.MainManager.Instance.DicTeam.Add(name, t);
                                            Client.MainManager.Instance.Charts.Add(t);
                                            break;
                                        }

                                        //데이터 입력
                                        Client.MainManager.Instance.DicTeam[team].Items.Add(t);
                                        Client.MainManager.Instance.DicTeam.Add(name, t);
                                    }
                                    break;
                                case (byte)1: //유저
                                    {
                                        byte tag = 1;
                                        ++byIter;
                                        int num = BitConverter.ToInt32(ba, byIter);
                                        byIter += 4;
                                        int NextVarLength = BitConverter.ToInt32(ba, byIter);
                                        byIter += 4;
                                        string pos = Encoding.UTF8.GetString(ba, byIter, NextVarLength);
                                        byIter += NextVarLength;
                                        NextVarLength = BitConverter.ToInt32(ba, byIter);
                                        byIter += 4;
                                        string id = Encoding.UTF8.GetString(ba, byIter, NextVarLength);
                                        byIter += NextVarLength;
                                        NextVarLength = BitConverter.ToInt32(ba, byIter);
                                        byIter += 4;
                                        string name = Encoding.UTF8.GetString(ba, byIter, NextVarLength);
                                        byIter += NextVarLength;
                                        NextVarLength = BitConverter.ToInt32(ba, byIter);
                                        byIter += 4;
                                        string team = Encoding.UTF8.GetString(ba, byIter, NextVarLength);
                                        byIter += NextVarLength;

                                        Client.TreeItems item = new Client.TreeItems(num, pos, id, name, tag, team);

                                        Client.MainManager.Instance.DicUser.Add(item.Num, item);
                                        Client.MainManager.Instance.DicTeam[team].Items.Add(item);
                                    }
                                    break;
                                case 2: // 즐겨찾기 폴더
                                    {
                                        //다음 바이트
                                        byte tag = 2;
                                        ++byIter;
                                        //name 길이 받기
                                        int NextVarLength = BitConverter.ToInt32(ba, byIter);
                                        //다음 바이트
                                        byIter += 4;
                                        //name 할당(ba에서 byIter부터 시작해 NextVarLength만큼)
                                        string name = Encoding.UTF8.GetString(ba, byIter, NextVarLength);
                                        //다음 바이트
                                        byIter += NextVarLength;
                                        Client.TreeItems t = new Client.TreeItems(name, tag);
                                        name = Encoding.UTF8.GetString(ba, byIter, NextVarLength);
                                        byIter += NextVarLength;

                                        //데이터 입력
                                        Client.MainManager.Instance.DicTeam[name].Items.Add(t);
                                        Client.MainManager.Instance.DicTeam.Add(t.Name, t);
                                    }
                                    break;
                                case 3: //즐겨찾기 유저
                                    {
                                        ++byIter;
                                        int num = BitConverter.ToInt32(ba, byIter);
                                        byIter += 4;
                                        Client.TreeItems t = Client.MainManager.Instance.Charts.Where(a => a.Num == num).FirstOrDefault().DeepCopy();
                                    }
                                    break;
                                case 4: //즐겨찾기 채팅 
                                    {
                                        ++byIter;
                                        int num = BitConverter.ToInt32(ba, byIter);
                                        byIter += 4;
                                        int NextVarLength = BitConverter.ToInt32(ba, byIter);
                                        byIter += 4;
                                        string name = Encoding.UTF8.GetString(ba, byIter, NextVarLength);
                                        byIter += NextVarLength;
                                        Client.TreeItems t = new Client.TreeItems(name, num);
                                        name = Encoding.UTF8.GetString(ba, byIter, NextVarLength);
                                        byIter += NextVarLength;
                                        Client.MainManager.Instance.DicTeam[name].Items.Add(t);
                                    }
                                    break;
                            }
                        }
                        break;
                    }
                case 1: //유저 정보
                    {
                        int NextVarLength = BitConverter.ToInt32(ba, byIter);
                        byIter += 4;
                        byte[] image = new byte[NextVarLength];
                        Array.Copy(ba, byIter, image, 0, NextVarLength);
                        BitmapImage bImage = ConvertImage(image);
                        NextVarLength = BitConverter.ToInt32(ba, byIter);
                        byIter += 4;
                        string Name = Encoding.UTF8.GetString(ba, byIter, NextVarLength);
                        byIter += NextVarLength;
                        NextVarLength = BitConverter.ToInt32(ba, byIter);
                        byIter += 4;
                        string Position = Encoding.UTF8.GetString(ba, byIter, NextVarLength);
                        byIter += NextVarLength;
                        NextVarLength = BitConverter.ToInt32(ba, byIter);
                        byIter += 4;
                        string Team = Encoding.UTF8.GetString(ba, byIter, NextVarLength);
                        byIter += NextVarLength;
                        NextVarLength = BitConverter.ToInt32(ba, byIter);
                        byIter += 4;
                        string ID = Encoding.UTF8.GetString(ba, byIter, NextVarLength);
                        byIter += NextVarLength;
                        NextVarLength = BitConverter.ToInt32(ba, byIter);
                        byIter += 4;
                        string telNum = Encoding.UTF8.GetString(ba, byIter, NextVarLength);
                        byIter += NextVarLength;

                        Client.UserInfoWindow u = new Client.UserInfoWindow(Name, Position, Team, ID, telNum, bImage);
                        u.Show();
                        break;
                    }
                case 2: //채팅
                    {
                        byte type = ba[byIter];
                        ++byIter;
                        int roomnum = BitConverter.ToInt32(ba, byIter);
                        byIter += 4;
                        int datalength = BitConverter.ToInt32(ba, byIter);
                        byIter += 4;
                        byte[] data = new byte[datalength];
                        Array.Copy(ba, byIter, data, 0, datalength);
                        byIter += datalength;
                        int Sender = BitConverter.ToInt32(ba, byIter);
                        byIter += 4;
                        datalength = BitConverter.ToInt32(ba, byIter);
                        byIter += 4;
                        string filename = Encoding.UTF8.GetString(ba, byIter, datalength);
                        byIter += datalength;

                        if (roomnum == Client.MainManager.Instance.CurrentUserNum)
                        {
                            roomnum = Sender;
                        }

                        datalength = BitConverter.ToInt32(ba, byIter);
                        byIter += 4;
                        byte[] bDate = new byte[datalength];
                        Array.Copy(ba, byIter, bDate, 0, datalength);
                        ReceiveMessagePacket r = new ReceiveMessagePacket(type, roomnum, data, Sender, Encoding.UTF8.GetString(bDate), filename);

                        //Client.ChattingWindow c;

                        Client.MainManager.Instance.RecieveMessage(r);
                        break;
                    }
                case 3: //밀린 채팅
                    {

                        break;
                    }
                case 4: //현재 유저 정보
                    {
                        Client.MainManager.Instance.CurrentUserNum = BitConverter.ToInt32(ba, byIter);
                        byIter += 4;
                        int NextVarLength = BitConverter.ToInt32(ba, byIter);
                        byIter += 4;
                        string id = Encoding.UTF8.GetString(ba, byIter, NextVarLength);
                        byIter += NextVarLength;
                        NextVarLength = BitConverter.ToInt32(ba, byIter);
                        byIter += 4;
                        string name = Encoding.UTF8.GetString(ba, byIter, NextVarLength);
                        byIter += NextVarLength;
                    break;
                    }
                case 5: //로그인 결과
                    {
                        byte r = ba[1];
                        int num = BitConverter.ToInt32(ba, 2);
                        string name = Encoding.UTF8.GetString(ba, 6, ba.Length - 6);
                        Client.ResultArgs ra = new Client.ResultArgs(r, num, name);
                        LogInEvent(ra);
                    }
                    break;
                case 50: //새 방 만들기
                    byte namelength = ba[byIter];
                    ++byIter;
                    string newroomname = Encoding.UTF8.GetString(ba, byIter, namelength);
                    byIter += namelength;
                    int newroomnum = BitConverter.ToInt32(ba, byIter);
                    byIter += 4;

                    Client.MainManager.Instance.CreateDB(newroomnum, newroomname);
                    System.Data.SQLite.SQLiteConnection newroom = Client.MainManager.Instance.dicChatDB[newroomnum];
                    System.Data.SQLite.SQLiteCommand scm = new System.Data.SQLite.SQLiteCommand("INSERT INTO userlist (usernum, state) values (@usernum, @state)", newroom);

                    while (byIter > ba.Length)
                    {
                        scm.Parameters.Add(new System.Data.SQLite.SQLiteParameter("@usernum", BitConverter.ToInt32(ba,byIter)));
                        scm.Parameters.Add(new System.Data.SQLite.SQLiteParameter("@state", 0));
                        scm.ExecuteNonQuery();
                        byIter += 4;
                    }

                    break;
            }

        }

        static BitmapImage ConvertImage(byte[] b)
        {
            if (b == null || b.Length == 0) return null;
            var image = new BitmapImage();
            using (var mem = new MemoryStream(b))
            {
                mem.Position = 0;
                image.BeginInit();
                image.CreateOptions = BitmapCreateOptions.PreservePixelFormat;
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.UriSource = null;
                image.StreamSource = mem;
                image.EndInit();
            }
            image.Freeze();
            return image;
        }
    }
}
