using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Windows;

namespace NetWork
{
    class NetWorkManager
    {
        #region Singleton Decla
        protected static NetWorkManager _instance = null;

        public static NetWorkManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new NetWorkManager();
                }

                return _instance;
            }
        }

        #endregion

        public NetWorkManager()
        {
        }

        public bool isSend = false;
        public Socket client;
        public byte[] AESKey;
        public byte[] AESIV;
        public int buffersize = 8192;
        SocketAsyncEventArgsPool argsPool = new SocketAsyncEventArgsPool();
        bool isBig = false;
        List<byte[]> BufferList = new List<byte[]>();

        #region Send
        public void Send(Packet obj)
        {
            try
            {
                SocketAsyncEventArgs s = argsPool.Pop();
                byte[] cryptData = Crypto.AESCrypto.Encrypt(Serialization.Serialize(obj), AESKey, AESIV);
                byte[] sendBuffer = new byte[cryptData.Length + 4];
                Array.Copy(BitConverter.GetBytes(cryptData.Length), sendBuffer, 4);
                Array.Copy(cryptData, 0, sendBuffer, 4, cryptData.Length);

                s.SetBuffer(sendBuffer, 0, sendBuffer.Length);
                s.Completed += new EventHandler<SocketAsyncEventArgs>(SendComplete);
                client.SendAsync(s);
            }
            catch
            {
                MessageBox.Show("서버 확인");
            }
        }

        public void SendSync(byte[] b)
        {
            byte[] a = Crypto.AESCrypto.Encrypt(b, AESKey, AESIV);
            byte[] sendBuffer = new byte[a.Length + 4];
            Array.Copy(BitConverter.GetBytes(a.Length), sendBuffer, 4);
            Array.Copy(a, 0, sendBuffer, 4, a.Length);
            client.Send(sendBuffer, sendBuffer.Length, SocketFlags.None);
        }

        public byte[] Encrypt(byte[] b)
        {
            return Crypto.AESCrypto.Encrypt(b, AESKey, AESIV);
        }

        void SendComplete(object o, SocketAsyncEventArgs e)
        {
            e.Completed -= SendComplete;
            e.SetBuffer(new byte[8192], 0, 8192);
            argsPool.Push(e);
        }
        #endregion

        #region Receive
        public void BeginReceive()
        {
            SocketAsyncEventArgs s = argsPool.Pop();
            s.Completed += new EventHandler<SocketAsyncEventArgs>(ReceiveArgsCompleted);
            client.ReceiveAsync(s);
        }

        public void ReceiveArgsCompleted(object obj, SocketAsyncEventArgs args)
        {
            args.Completed -= this.ReceiveArgsCompleted;


            if (((Socket)obj).Connected == false)
            {
                //서버 연결 끊김 이벤트
                MessageBox.Show("서버랑 연결 끊겼어");
                return;
            }

            try
            {
                if (args.BytesTransferred >= args.Count)
                {
                    BufferList.Add(args.Buffer);
                    isBig = true;
                }
                else
                {
                    byte[] b = new byte[args.BytesTransferred];
                    Array.Copy(args.Buffer, b, args.BytesTransferred);
                    if (isBig)
                    {
                        BufferList.Add(b);
                        byte[] bBuffer = BufferList.SelectMany(a => a).ToArray();
                        byte[] crypt = Crypto.AESCrypto.Decrypt(bBuffer, AESKey, AESIV);
                        Serialization.DeSerialize(crypt);
                        BufferList.Clear();
                        isBig = false;
                    }
                    else
                    {
                        byte[] crypt = Crypto.AESCrypto.Decrypt(b, AESKey, AESIV);
                        Serialization.DeSerialize(crypt);
                    }
                }
            }
            catch
            {
                MessageBox.Show("인터넷 상태를 확인하여주세요.");
                return;
            }

            argsPool.Push(args);
            BeginReceive();
        }
        #endregion

    }

    class SocketAsyncEventArgsPool
    {
        Stack<SocketAsyncEventArgs> ArgsStack = new Stack<SocketAsyncEventArgs>();
        const int bufferSize = 8192;

        public SocketAsyncEventArgsPool()
        {
            for (int i = 0; i < 10; i++)
            {
                SocketAsyncEventArgs s = new SocketAsyncEventArgs();
                s.SetBuffer(new byte[bufferSize], 0, bufferSize);
                ArgsStack.Push(s);
            }
        }

        public SocketAsyncEventArgs Pop()
        {
            if (ArgsStack.Count == 0)
            {
                SocketAsyncEventArgs s = new SocketAsyncEventArgs();
                s.SetBuffer(new byte[bufferSize], 0, bufferSize);
                return s;
            }
            else
            {
                SocketAsyncEventArgs s = ArgsStack.Pop();
                s.SetBuffer(new byte[bufferSize], 0, bufferSize);
                return s;
            }
        }

        public void Push(SocketAsyncEventArgs e)
        {
            ArgsStack.Push(e);
        }

        ~SocketAsyncEventArgsPool()
        {
            ArgsStack.Clear();
        }
    }


}

// 1. 서버와 연결
// 2. 업데이트 체크
// 3. RSA 공개 키 받기
// 4. AES 키 만들고 전달
// 5. ID,PW AES 암호화 후 전달
// 6. 결과 확인
// 7. 현재 유저 정보 및 조직도 데이터 전달
// 8. 마지막 연결 시간 전송 후 미수신 데이터 수신