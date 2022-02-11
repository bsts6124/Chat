using Command;
using NetWork;
using System;
using System.Net.Sockets;
using System.Security;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace Client
{
    class LoginWindowViewModel : ViewModelBase
    {

        int port = 10000;
        string ip = "192.168.60.160"; //"192.168.100.222";
        public bool isPassword = false;

        public LoginWindowViewModel()
        {
            NetWorkManager.Instance.client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            //try
            //{
            //   TryConnect();
            //}
            //catch
            //{
            //    if (NetWorkManager.Instance.client.Connected)
            //        NetWorkManager.Instance.client.Disconnect(true);

            //    MessageBox.Show("인터넷 연결을 확인해주세요.");
            //}

            MainManager.Instance.isLogOn = false;

            NetWork.Serialization.LogInEvent += new Serialization.delLogIn(LoginResult);
            LoginCommand = new DelegateCommand(LogIn, CanLogin);
        }

        #region 유저정보
        private string _userName;
        public string UserName
        {
            get { return _userName; }
            set
            {
                _userName = value;
                OnPropertyChanged();
            }
        }

        private SecureString _passWord;
        public SecureString Password
        {
            get { return _passWord; }
            set
            {
                _passWord = value;
                OnPropertyChanged();
            }
        }
        #endregion

        #region Login Command
        public DelegateCommand LoginCommand { private get; set; }

        public void LogIn(object obj)
        {
            if (NetWorkManager.Instance.client.Connected == false)
            {
                try
                {
                    TryConnect();
                }
                catch
                {
                    if (NetWorkManager.Instance.client.Connected)
                        NetWorkManager.Instance.client.Disconnect(true);

                    MessageBox.Show("인터넷 연결을 확인해주세요.");
                    return;
                }
            }

            NetWork.LoginPacket data = new LoginPacket(UserName, GetPassword(Password));
            try
            {
                NetWorkManager.Instance.Send(data);
            }
            catch
            {
                MessageBox.Show("인터넷 상태 확인");
            }
            PasswordBox pswbox = (PasswordBox)obj;
            pswbox.Clear();
        }

        void LoginResult(ResultArgs e)
        {
            switch (e.Result)
            {
                case 0:
                    NetWork.Serialization.LogInEvent -= LoginResult;
                    LoginCommand = null;
                    System.Windows.Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        MainManager.Instance.CurrentUserNum = e.UserNum;
                        MainManager.Instance.DBpath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\Chat\\" + MainManager.Instance.CurrentUserNum.ToString() + "\\Chatlist.sqlite";
                        MainManager.Instance.LogsPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\Chat\\" + MainManager.Instance.CurrentUserNum.ToString() + "\\Logs\\";
                        MainManager.Instance.ReadChatList();
                        MainWindow win = new MainWindow();
                        win.Show();
                        this.HideAction();
                    }
                    ));
                    break;
                case 1: // 뭔가 틀림
                    MessageBox.Show("잘못 입력하셨습니다");
                    break;
            }
        }

        //public void LogIn(object obj)
        //{
        //    MainWindow win = new MainWindow();
        //    win.Show();
        //    HideAction();
        //}

        bool CanLogin(object obj)
        {
            if ((String.IsNullOrEmpty(_userName) == true) || (isPassword == false)) return false;

            return true;
        }

        string GetPassword(SecureString s)
        {
            IntPtr unmanagedString = IntPtr.Zero;

            unmanagedString = System.Runtime.InteropServices.Marshal.SecureStringToGlobalAllocUnicode(s);

            return System.Runtime.InteropServices.Marshal.PtrToStringUni(unmanagedString);
        }

        #endregion


        void TryConnect()
        {
            NetWorkManager.Instance.client.Connect(ip, port);
            //IAsyncResult result = NetWorkManager.Instance.client.BeginConnect(ip, port, null, null);

            //bool success = result.AsyncWaitHandle.WaitOne(5000, true);

            //if (NetWorkManager.Instance.client.Connected)
            //{
            //    NetWorkManager.Instance.client.EndConnect(result);
            //}

            #region updatecheck
            byte[] ver = new byte[8192];
            NetWorkManager.Instance.client.Receive(ver);

            byte[] Result = new byte[1];
            if (MainManager.Instance.Version != Encoding.UTF8.GetString(ver).TrimEnd('\0'))
            {
                Result[0] = 1;
                NetWorkManager.Instance.client.Send(Result);
                //대충 업데이트 프로그램 실행
                MessageBox.Show("업데이트해야댐");
                return;
            }
            else
            {
                Result[0] = 0;
                NetWorkManager.Instance.client.Send(Result);
            }
            #endregion

            byte[] keyb = new byte[8192];
            NetWorkManager.Instance.client.Receive(keyb);

            byte[] exp = new byte[3];
            byte[] mod = new byte[256];
            Array.Copy(keyb, 0, exp, 0, 3);
            Array.Copy(keyb, 3, mod, 0, 256);
            Crypto.RSACrypto.PublicKey key = new Crypto.RSACrypto.PublicKey(mod, exp);

            System.Security.Cryptography.AesCryptoServiceProvider crypto = new System.Security.Cryptography.AesCryptoServiceProvider();
            crypto.KeySize = 256;
            crypto.BlockSize = 128;
            crypto.GenerateKey();

            NetWorkManager.Instance.AESKey = crypto.Key;
            NetWorkManager.Instance.AESIV = crypto.IV;

            byte[] b = new byte[48];
            Array.Copy(crypto.Key, 0, b, 0, 32);
            Array.Copy(crypto.IV, 0, b, 32, 16);
            byte[] aeskey = Crypto.RSACrypto.Encrypt(key, b, false);
            NetWorkManager.Instance.client.Send(aeskey);
            NetWorkManager.Instance.BeginReceive();
        }


        public Action HideAction { get; set; }
    }

    class ResultArgs : EventArgs
    {
        public byte Result;
        public int UserNum;
        public string UserName;
        public ResultArgs(byte r, int userNum, string name)
        {
            Result = r;
            UserNum = userNum;
            UserName = name;
        }
    }
}
