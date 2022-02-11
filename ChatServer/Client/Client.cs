using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Linq;
using System.IO;

namespace Server
{
    class Client 
    {
        byte[] key;
        byte[] iv;
        Crypto.RSACrypto.KeyPair kp = new Crypto.RSACrypto.KeyPair(2048);

        public Socket clientSocket;
        public FileStream fStream;
        public string fileName;
        public int CurrentUser = 0;
        public int fileSize = 0;
        bool isVersionCheck = false;
        bool isConnect = true;
        int PacketSize = 0;
        byte[] Buffer = new byte[Server.BufferSize];

        public Client(Socket s)
        {
            clientSocket = s;
        }

        ~Client()
        {
            kp = null;
            BufferList = null;

            Console.WriteLine("지움");
        }

        #region RecieveAsync
        List<byte[]> BufferList = new List<byte[]>();

        public void BeginRecieve(SocketAsyncEventArgs e)
        {
            //e.SetBuffer(new byte[8192], 0, 8192);
            e.Completed += new EventHandler<SocketAsyncEventArgs>(RecieveComplete);
            bool ex = clientSocket.ReceiveAsync(e);
            if (ex == false)
            {
                ReceiveSync(clientSocket, e);
            }
        }

        void RecieveComplete(object o, SocketAsyncEventArgs e)
        {
            e.Completed -= this.RecieveComplete;

            try
            {
                if(key==null)
                {
                    if (isVersionCheck == false)
                    {
                        if (e.Buffer[0] == 0) //클라이언트가 최신버전일때
                        {
                            isVersionCheck = true;
                            SocketAsyncEventArgs s = Server.EventPool.Pop();
                            byte[] publickey = new byte[259];
                            Array.Copy(kp.publicKey.Exponent, publickey, 3);
                            Array.Copy(kp.publicKey.Modulus, 0, publickey, 3, 256);
                            s.SetBuffer(publickey,0,259);
                            s.Completed += new EventHandler<SocketAsyncEventArgs>(SendComplete);
                            clientSocket.SendAsync(s);
                        }
                        else //클라이언트가 최신이 아닐때
                        {

                        }
                    }
                    else
                    {
                        byte[] aes = new byte[256];
                        Array.Copy(e.Buffer, aes, 256);
                        byte[] aeskey = Crypto.RSACrypto.Decrypt(kp.privateKey, aes, false);

                        key = new byte[32];
                        iv = new byte[16];

                        Array.Copy(aeskey, 0, key, 0, 32);
                        Array.Copy(aeskey, 32, iv, 0, 16);
                    }
                }
                else
                {
                    
                    int PacketIter = 0;

                    while (true)
                    {
                        if (PacketSize == 0)
                        {
                            PacketSize = BitConverter.ToInt32(e.Buffer, PacketIter);
                            PacketIter += 4;
                        }

                        if (PacketIter + PacketSize > e.BytesTransferred)
                        {
                            int packetbuffersize = e.BytesTransferred - PacketIter;
                            byte[] bufferele = new byte[packetbuffersize];
                            Array.Copy(e.Buffer, PacketIter, bufferele, 0, packetbuffersize);
                            BufferList.Add(bufferele);
                            PacketSize -= packetbuffersize;
                            PacketIter += packetbuffersize;
                        }
                        //else if (PacketIter + PacketSize <= e.BytesTransferred)
                        //{
                        //    byte[] bufferele = new byte[PacketSize];
                        //    Array.Copy(e.Buffer, PacketIter, bufferele, 0, PacketSize);
                        //    BufferList.Add(bufferele);
                        //    byte[] bBuffer = BufferList.SelectMany(a => a).ToArray();
                        //    Serialization.Deserialize(Crypto.AESCrypto.Decrypt(bBuffer, key, iv), this);
                        //    BufferList.Clear();

                        //    PacketIter += PacketSize;
                        //    PacketSize = 0;
                        //}
                        else
                        {
                            byte[] bufferele = new byte[PacketSize];
                            Array.Copy(e.Buffer, PacketIter, bufferele, 0, PacketSize);
                            BufferList.Add(bufferele);
                            byte[] bBuffer = BufferList.SelectMany(a => a).ToArray();
                            Serialization.Deserialize(Crypto.AESCrypto.Decrypt(bBuffer, key, iv), this);
                            BufferList.Clear();

                            PacketIter += PacketSize;
                            PacketSize = 0;
                        }

                        if (PacketIter >= e.BytesTransferred)
                        {
                            break;
                        }
                    }

                    #region 이전 패킷
                    //if (e.BytesTransferred == PacketSize)
                    //{
                    //    PacketSize = 0;
                    //    byte[] b = new byte[e.BytesTransferred];
                    //    Array.Copy(e.Buffer, b, e.BytesTransferred);
                    //    BufferList.Add(b);
                    //    byte[] bBuffer = BufferList.SelectMany(a => a).ToArray();
                    //    // 역직렬화
                    //    Serialization.Deserialize(Crypto.AESCrypto.Decrypt(bBuffer, key, iv), this);
                    //    BufferList.Clear();
                    //}
                    //else if (e.BytesTransferred < PacketSize)
                    //{
                    //    PacketSize -= e.BytesTransferred;
                    //    BufferList.Add(e.Buffer);
                    //}



                    //if (e.BytesTransferred >= e.Count)
                    //{
                    //    BufferList.Add(e.Buffer);
                    //    isBig = true;
                    //}
                    //else
                    //{
                    //    byte[] b = new byte[e.BytesTransferred-4];
                    //    Array.Copy(e.Buffer, 4, b,0, e.BytesTransferred-4);
                    //    if (isBig)
                    //    {
                    //        BufferList.Add(b);
                    //        byte[] bBuffer = BufferList.SelectMany(a => a).ToArray();
                    //        // 역직렬화
                    //        Serialization.Deserialize(Crypto.AESCrypto.Decrypt(bBuffer, key, iv), this);
                    //        BufferList.Clear();
                    //        isBig = false;
                    //    }
                    //    else
                    //    {
                            
                    //        //역직렬화
                    //        Serialization.Deserialize(Crypto.AESCrypto.Decrypt(b, key, iv), this);
                    //    }
                    //}
                    #endregion
                }
            }
            catch
            {
                
            }

            if (isConnect == false)
            {
                Server.EventPool.Push(e);
                return;
            }
            e.SetBuffer(Buffer, 0, Server.BufferSize);
            BeginRecieve(e);
        }

        void ReceiveSync(object o, SocketAsyncEventArgs e)
        {
            e.Completed -= RecieveComplete;

            if (e.BytesTransferred == 0)
            {
                Disconnect();
                return;
            }

            int PacketIter = 0;

            while (true)
            {
                if (PacketSize == 0)
                {
                    PacketSize = BitConverter.ToInt32(e.Buffer, PacketIter);
                    PacketIter += 4;
                }

                if (PacketIter + PacketSize > e.BytesTransferred)
                {
                    int packetbuffersize = e.BytesTransferred - PacketIter;
                    byte[] bufferele = new byte[packetbuffersize];
                    Array.Copy(e.Buffer, PacketIter, bufferele, 0, packetbuffersize);
                    BufferList.Add(bufferele);
                    PacketSize -= packetbuffersize;
                    PacketIter += packetbuffersize;
                }
                else
                {
                    byte[] bufferele = new byte[PacketSize];
                    Array.Copy(e.Buffer, PacketIter, bufferele, 0, PacketSize);
                    BufferList.Add(bufferele);
                    byte[] bBuffer = BufferList.SelectMany(a => a).ToArray();
                    Serialization.Deserialize(Crypto.AESCrypto.Decrypt(bBuffer, key, iv), this);
                    BufferList.Clear();

                    PacketIter += PacketSize;
                    PacketSize = 0;
                }

                if (PacketIter >= e.BytesTransferred)
                {
                    break;
                }
            }


            BeginRecieve(e);
        }
        #endregion

        #region SendAsync
        public void Send(Packet p)
        {
            try
            {
                SocketAsyncEventArgs e= Server.EventPool.Pop();
                byte[] crypt= Crypto.AESCrypto.Encrypt(p.Serialize(), key, iv);
                e.SetBuffer(crypt,0,crypt.Length);
                //e.Completed += new EventHandler<SocketAsyncEventArgs>(SendComplete);
                clientSocket.SendAsync(e);
                Server.EventPool.Push(e);
            }
            catch
            {
                Console.WriteLine("전송 실패");
            }
        }

        public void Send(SocketAsyncEventArgs e)
        {
            try
            {
                //e.Completed += new EventHandler<SocketAsyncEventArgs>(SendComplete);
                clientSocket.SendAsync(e);
            }
            catch
            {
                Console.WriteLine("전송 실패");
            }
            Server.EventPool.Push(e);
        }

        public void Send(byte[] array)
        {
            try
            {
                SocketAsyncEventArgs e = Server.EventPool.Pop();
                byte[] crypt = Crypto.AESCrypto.Encrypt(array, key, iv);
                e.SetBuffer(crypt, 0, crypt.Length);
                //e.Completed += new EventHandler<SocketAsyncEventArgs>(SendComplete);
                clientSocket.SendAsync(e);
                Server.EventPool.Push(e);
            }
            catch
            {
                Console.WriteLine("전송 실패");
            }
        }

        void SendComplete(object o, SocketAsyncEventArgs e)
        {
            //e.Completed -= SendComplete;
            Server.EventPool.Push(e);
        }
        #endregion

        public void Disconnect()
        {
            isConnect = false;
            clientSocket.Close();
            clientSocket.Dispose();
            if (CurrentUser != 0)
            {
                Server.dicConnectUser.Remove(CurrentUser);
            }
        }

    }
}
