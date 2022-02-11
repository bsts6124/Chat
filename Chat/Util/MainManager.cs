using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Text;

namespace Client
{
    class MainManager : ViewModelBase
    {
        #region constructor
        private static MainManager _instance = null;
        public static MainManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new MainManager();
                }
                return _instance;
            }
        }

        System.Windows.Forms.NotifyIcon ni = new System.Windows.Forms.NotifyIcon();


        public MainManager()
        {
            System.Windows.Forms.ContextMenu menu = new System.Windows.Forms.ContextMenu();

            System.Windows.Forms.MenuItem openitem = new System.Windows.Forms.MenuItem();
            System.Windows.Forms.MenuItem logoutitem = new System.Windows.Forms.MenuItem();
            System.Windows.Forms.MenuItem closeitem = new System.Windows.Forms.MenuItem();

            openitem.Text = "열기";
            openitem.Click += delegate (object o, EventArgs e)
            {

            };

            logoutitem.Text = "로그아웃";


            closeitem.Text = "종료";

            menu.MenuItems.Add(openitem);
            menu.MenuItems.Add(logoutitem);
            menu.MenuItems.Add(closeitem);

            //ni.Icon = Client.Properties.Resources.; 
            //ni.Icon = new System.Drawing.Icon("Resources/traytest.ico");
            ni.Visible = true;
            ni.Text = "test";
            ni.ContextMenu = menu;
        }

        ~MainManager()
        {
            //foreach(KeyValuePair<int,SQLiteConnection> item in dicChatDB)
            //{
            //    item.Value.Close();
            //}

        }

        #endregion

        public int CurrentUserNum;
        public string UserName = null;
        public string Version = "a";

        public bool isLogOn;

        #region TreeView

        /// <summary>
        /// 친구목록
        /// </summary>
        public Dictionary<string, TreeItems> DicTeam = new Dictionary<string, TreeItems>();
        private ObservableCollection<TreeItems> _Charts = new ObservableCollection<TreeItems>();

        public ObservableCollection<TreeItems> Charts
        {
            get
            {
                return _Charts;
            }
            set
            {
                _Charts = value;
                OnPropertyChanged();
            }
        }

        #endregion

        #region ChattingList
        /// <summary>
        /// 채팅목록
        /// </summary>
        private ObservableCollection<ChatListModel> _ChatList = new ObservableCollection<ChatListModel>();
        public ObservableCollection<ChatListModel> colChatList
        {
            get { return _ChatList; }
            set
            {
                _ChatList = value;
                OnPropertyChanged();
            }
        }
        public Dictionary<int, TreeItems> DicUser = new Dictionary<int, TreeItems>();

        public delegate void delChatListRefresh();
        public event delChatListRefresh RefreshEvent;
        #endregion

        #region ChattingWindowList
        /// <summary>
        /// 현재 열려있는 윈도우창 리스트
        /// </summary>
        public Dictionary<int, ChattingWindow> dicChatWindows = new Dictionary<int, ChattingWindow>();

        public ChattingWindow ShowWindow(TreeItems t)
        {
            ChattingWindow cw;
            if(dicChatWindows.ContainsKey(t.Num))
            {
                cw = dicChatWindows[t.Num];
                cw.Activate();
                cw.Topmost = true;
                System.Threading.Thread.Sleep(50);
                cw.Topmost = false;
            }
            else
            {
                if(!dicChatDB.ContainsKey(t.Num))
                {
                    SQLiteCommand scm = new SQLiteCommand("SELECT roomnum FROM chattinglist WHERE roomnum = "+t.Num.ToString(), ChattingListDB);
                    if(scm.ExecuteScalar() != null)
                    {
                        string logdb = LogsPath + t.Num.ToString() + ".db";
                        SQLiteConnection log = new SQLiteConnection("Data Source=" + logdb + ";Version=3;");
                        log.Open();
                        dicChatDB.Add(t.Num, log);
                    }
                }
                ChattingWindowViewModel cwvm = new ChattingWindowViewModel(t.Num);
                cw = new ChattingWindow(cwvm);
                dicChatWindows.Add(t.Num, cw);
                cwvm.RoomName = t.Name;
                cw.ReceiveEvent += cwvm.ReceiveMessage;
                cw.Show();
            }
            return cw;
        }
        #endregion

        #region SQLite
        public string DBpath;
        public string LogsPath;

        /// <summary>
        /// 프로그램 실행 후 열린 채팅방 로그 DB 사전
        /// </summary>
        public Dictionary<int, SQLiteConnection> dicChatDB = new Dictionary<int, SQLiteConnection>();
        /// <summary>
        /// 채팅 목록 관리 DB
        /// </summary>
        public SQLiteConnection ChattingListDB = null;

        public void ReadChatList()
        {
            if (File.Exists(DBpath) == false)
            {
                Directory.CreateDirectory(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\Chat\\" + CurrentUserNum.ToString());
                Directory.CreateDirectory(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\Chat\\" + CurrentUserNum.ToString() + "\\Logs");
                SQLiteConnection.CreateFile(DBpath);
                ChattingListDB = new SQLiteConnection("Data Source=" + DBpath + ";Version=3;");
                ChattingListDB.Open();
                string sql = "CREATE TABLE chattinglist(roomnum INTEGER PRIMARY KEY NOT NULL, roomname TEXT, notreadnum INTEGER, lastdate TEXT, lastlog TEXT, roomimage BLOB)";
                SQLiteCommand com = new SQLiteCommand(sql, ChattingListDB);
                com.ExecuteNonQuery();
            }
            else
            {
                ChattingListDB = new SQLiteConnection("Data Source=" + DBpath + ";Version=3;");
                ChattingListDB.Open();
            }

            string read = "select * from chattinglist order by LastDate desc";
            SQLiteCommand cmd = new SQLiteCommand(read, ChattingListDB);

            SQLiteDataReader reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                if (reader.GetInt32(0) < 1000000)
                {
                    ChatListModel cm;
                    if (FindUser(reader.GetInt32(0))== null)
                    {
                        cm = new ChatListModel(reader.GetInt32(2), reader.GetInt32(0), reader.GetInt32(0).ToString(), reader.GetString(4), reader.GetString(3));
                    }
                    else
                    {
                        string name = FindUser(reader.GetInt32(0)).Name;
                        cm = new ChatListModel(reader.GetInt32(2), reader.GetInt32(0), name, reader.GetString(4), reader.GetString(3));
                    }
                    colChatList.Add(cm);
                }
                else
                {
                    ChatListModel cm = new ChatListModel(reader.GetInt32(2), reader.GetInt32(0), reader.GetString(1), reader.GetString(4), reader.GetString(3));
                    colChatList.Add(cm);
                }
            }
        }

        public void RecieveMessage(NetWork.ReceiveMessagePacket rp)
        {
            if (dicChatDB.ContainsKey(rp.RoomNum) == false) OpenDB(rp.RoomNum);

            ChattingWindow c;
            if (dicChatWindows.TryGetValue(rp.RoomNum, out c) == true)
            {
                System.Windows.Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                {
                    c.ReceiveMessage(rp);
                }
                ));
            }

            System.Windows.Application.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                // 내용 갱신
                ChatListModel cm = colChatList.Where(x => x.RoomNum == rp.RoomNum).FirstOrDefault();
                cm.LastDate = DateTime.ParseExact(rp.DateTime, "dd/MM/yyyy HH:mm", null);

                if (rp.Type == 0) cm.LastLog = Encoding.UTF8.GetString(rp.Data);
                else if (rp.Type == 1) cm.LastLog = "이미지";
                else if (rp.Type == 2) cm.LastLog = "파일";

                StringBuilder sb = new StringBuilder();
                sb.Append("UPDATE chattinglist SET ");
                if (c.IsActive == true)
                {
                    cm.NotReadLogs = 0;
                    sb.Append("notreadnum = '");
                    sb.Append(cm.NotReadLogs);
                    sb.Append("', lastlog = '");
                    sb.Append(cm.LastLog);
                    sb.Append("', lastdate = '");
                    sb.Append(rp.DateTime);
                    sb.Append("' WHERE roomnum = ");
                    sb.Append(cm.RoomNum.ToString());
                }
                else
                {
                    ++cm.NotReadLogs;
                    sb.Append("notreadnum = '");
                    sb.Append(cm.NotReadLogs);
                    sb.Append("', lastlog = '");
                    sb.Append(cm.LastLog);
                    sb.Append("', lastdate = '");
                    sb.Append(rp.DateTime);
                    sb.Append("' WHERE roomnum = ");
                    sb.Append(cm.RoomNum.ToString());
                }
                string sbb = sb.ToString();
                    //DB 내용 추가
                    SQLiteCommand ListUpdatecom = new SQLiteCommand(sbb, ChattingListDB);
                ListUpdatecom.ExecuteNonQuery();

                RefreshEvent();
                sb.Clear();

                string ChatlogQuery = "INSERT INTO chatlogs (log, datetime, type, roomnum, sender, filedata) values (@log, @date, @type, @roomnum, @sender, @data)";
                SQLiteCommand AddLogcom = new SQLiteCommand(ChatlogQuery, dicChatDB[rp.RoomNum]);
                AddLogcom.Parameters.Add("@log", System.Data.DbType.Binary).Value = rp.Data;
                AddLogcom.Parameters.Add(new SQLiteParameter("@date", rp.DateTime));
                AddLogcom.Parameters.Add(new SQLiteParameter("@type", rp.Type));
                AddLogcom.Parameters.Add(new SQLiteParameter("@roomnum", rp.RoomNum));
                AddLogcom.Parameters.Add(new SQLiteParameter("@sender", rp.Sender));
                AddLogcom.Parameters.Add(new SQLiteParameter("@data", rp.fileData));
                AddLogcom.ExecuteNonQuery();

                int chatIndex = colChatList.IndexOf(cm);
                if (chatIndex > 0) colChatList.Move(chatIndex, 0);
            }
            ));

            
        }

        void OpenDB(int Roomnum)
        {
            SQLiteCommand findcom = new SQLiteCommand("SELECT roomnum FROM chattinglist WHERE roomnum=" + Roomnum.ToString(), ChattingListDB);
            string logdb = LogsPath + Roomnum.ToString() + ".db";
            if (Convert.ToInt32(findcom.ExecuteScalar()) == 0)
            {
                SQLiteCommand AddRowcom = new SQLiteCommand("INSERT INTO chattinglist (roomnum, notreadnum, lastdate, lastlog, roomimage) values (" + Roomnum.ToString() + ", 0, NULL, NULL, NULL)", ChattingListDB);
                AddRowcom.ExecuteNonQuery();

                System.Windows.Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                {
                    if(Roomnum<1000000)
                        colChatList.Insert(0, new ChatListModel(0, Roomnum, FindUser(Roomnum).Name, "", DateTime.Now));
                    else colChatList.Insert(0, new ChatListModel(0, Roomnum, null, "", DateTime.Now));
                }
                ));

                SQLiteConnection.CreateFile(logdb);
                SQLiteConnection logcon = new SQLiteConnection("Data Source=" + logdb + ";Version=3;");
                logcon.Open();
                SQLiteCommand CreateTablecom = new SQLiteCommand("CREATE TABLE chatlogs(lognum INTEGER PRIMARY KEY AUTOINCREMENT, log BLOB NOT NULL, datetime TEXT NOT NULL, type INT NOT NULL, roomnum INT NOT NULL, sender INT NOT NULL, filedata TEXT, filepath TEXT)", logcon);
                CreateTablecom.ExecuteNonQuery();
                CreateTablecom = new SQLiteCommand("CREATE TABLE userlist(usernum INT, state INT)", logcon);
                CreateTablecom.ExecuteNonQuery();
                dicChatDB.Add(Roomnum, logcon);
            }
            else
            {
                SQLiteConnection log = new SQLiteConnection("Data Source=" + logdb + ";Version=3;");
                log.Open();
                dicChatDB.Add(Roomnum, log);
            }
        }

        public void CreateDB(int num, string name)
        {
            string logdb = LogsPath + num.ToString() + ".db";

            SQLiteCommand AddRowcom = new SQLiteCommand("INSERT INTO chattinglist (roomnum, roomname, notreadnum, lastdate, lastlog, roomimage) values (" + num.ToString() + ", '" + name + "', 0, NULL, NULL, NULL);", ChattingListDB);
            AddRowcom.ExecuteNonQuery();

            SQLiteConnection.CreateFile(logdb);
            SQLiteConnection logcon = new SQLiteConnection("Data Source=" + logdb + ";Version=3;");
            logcon.Open();
            SQLiteCommand CreateTablecom = new SQLiteCommand("CREATE TABLE chatlogs(lognum INTEGER PRIMARY KEY AUTOINCREMENT, log BLOB NOT NULL, datetime TEXT NOT NULL, type INT NOT NULL, roomnum INT NOT NULL, sender INT NOT NULL, filedata TEXT, filepath TEXT)", logcon);
            CreateTablecom.ExecuteNonQuery();
            CreateTablecom = new SQLiteCommand("CREATE TABLE userlist(usernum INT, state INT)", logcon);
            CreateTablecom.ExecuteNonQuery();
            dicChatDB.Add(num, logcon);
        }
        #endregion

        public TreeItems FindUser(int t)
        {
            TreeItems fitem;
            if (DicUser.TryGetValue(t, out fitem))
                return fitem;


            return null;
            //foreach (KeyValuePair<string, TreeItems> i in DicTeam)
            //{
            //    for (int j = 0; j < i.Value.Items.Count; j++)
            //    {
            //        if (i.Value.Items[j].Tag == 0)
            //        {
            //            continue;
            //        }
            //        else
            //        {
            //            if (i.Value.Items[j].Num != t)
            //            {
            //                continue;
            //            }
            //            else
            //            {
            //                return i.Value.Items[j];
            //            }
            //        }
            //    }
            //}

            //return null;
        }

        public void LogOut()
        {
            foreach (var key in dicChatWindows.Keys)
            {
                dicChatWindows[key].Close();
            }
            dicChatWindows.Clear();
        }


    }
}
