using Command;
using NetWork;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace Client
{
    class FileDropWindowViewModel : ViewModelBase
    {
        List<string> DropFiles = new List<string>();
        int roomNum;

        public FileDropWindowViewModel(string[] Files, int room)
        {
            StringBuilder sb = new StringBuilder();
            StringBuilder csb = new StringBuilder();
            FileInfo fi;
            for (int i = 0; i < Files.Length; i++)
            {
                fi = new FileInfo(Files[i]);

                if (fi.Length <= 104857600 || fi.Length > 0)
                {
                    if (sb.Length != 0)
                    {
                        sb.AppendLine();
                        sb.Append(fi.Name);
                    }
                    else
                    {
                        sb.Append(fi.Name);
                    }
                    DropFiles.Add(Files[i]);
                }
                else
                {
                    if (csb.Length != 0)
                    {
                        csb.AppendLine();
                        csb.Append(fi.Name);
                    }
                    else
                    {
                        csb.Append(fi.Name);
                    }
                }
            }

            CanSendFiles = sb.ToString();
            CantSendFiles = csb.ToString();

            SendFilesCommand = new DelegateCommand(FileSend, CSendFiles);
            CloseCommand = new DelegateCommand(CloseButtonClick);
            roomNum = room;
        }

        ~FileDropWindowViewModel()
        {
            SendFilesCommand = null;
            CloseCommand = null;
        }

        public string CanSendFiles { get; set; }

        public string CantSendFiles { get; set; }

        public DelegateCommand SendFilesCommand { get; set; }

        //void SendFiles(object o)
        //{
        //    lock (NetWork.NetWorkManager.Instance.client)
        //    {
        //        for (int i = 0; i < DropFiles.Count; i++)
        //        {
        //            //파일 정보 보내기
        //            FileStream fs = new FileStream(DropFiles[i], FileMode.Open);

        //            string splits = DropFiles[i].Split('\\').Last();
        //            string format = splits.Split('.').Last();
        //            int fileSize = (int)fs.Length;
        //            byte[] filename = Encoding.UTF8.GetBytes(splits);

        //            byte[] ba = new byte[filename.Length + 4];
        //            Array.Copy(BitConverter.GetBytes(fileSize), ba, 4);
        //            Array.Copy(filename, 0, ba, 4, filename.Length);

        //            NetWork.SendMessagePacket smp = new NetWork.SendMessagePacket(roomNum, 2, ba);
        //            NetWork.NetWorkManager.Instance.Send(smp);

        //            byte[] file = new byte[fs.Length];
        //            fs.Read(file, 0, (int)fs.Length);

        //            FilePacket f = new FilePacket(file);
        //            NetWorkManager.Instance.Send(f);
        //            fs.Close();
        //        }
        //    }
        //}

        bool CSendFiles(object o)
        {
            if (string.IsNullOrEmpty(CanSendFiles)) return false;

            return true;
        }

        public DelegateCommand CloseCommand { get; set; }

        void CloseButtonClick(object o) { CloseAction(); }


        public Action CloseAction { get; set; }


        //TEST
        void FileSend(object o)
        {
            NetWorkManager.Instance.isSend = true;
            for(int i = 0; i < DropFiles.Count; i++)
            {
                using (FileStream fs = new FileStream(DropFiles[i], FileMode.Open))
                {
                    int fileSize = (int)fs.Length;
                    NetWorkManager.Instance.SendSync(Serialization.Serialize(new PrevFilePacket(fileSize)));

                    byte[] buffer = new byte[NetWorkManager.Instance.buffersize - 5];
                    int currentOffset = 0;
                    while (fileSize > currentOffset)
                    {
                        if (fileSize - currentOffset > buffer.Length)
                        {
                            fs.Read(buffer, 0, buffer.Length);
                            currentOffset += buffer.Length;
                        }
                        else
                        {
                            buffer = new byte[fileSize - currentOffset];
                            fs.Read(buffer, 0, fileSize - currentOffset);
                            currentOffset = fileSize;
                        }

                        FilePacket fp = new FilePacket(buffer);

                        NetWorkManager.Instance.SendSync(fp.fileData);
                        Array.Clear(buffer, 0, buffer.Length);
                    }

                    string format = fs.Name.Split('.').Last().ToUpper();
                    if (format == "JPG" || format == "JPEG" || format == "PNG" || format == "BMP" || format == "GIF" || format == "TIFF")
                    {
                        if (fs.Length < 10737418236)
                        {
                            MemoryStream ms = new MemoryStream();
                            BitmapFrame bf = BitmapFrame.Create(fs);
                            if (bf.PixelWidth > 200 || bf.PixelHeight > 300)
                            {
                                float ratio;
                                if (bf.PixelWidth > 200) ratio = (float)200 / (float)bf.PixelWidth;
                                else ratio = (float)300 / (float)bf.PixelHeight;
                                TransformedBitmap tb = new TransformedBitmap(bf, new System.Windows.Media.ScaleTransform(ratio, ratio));
                                JpegBitmapEncoder je = new JpegBitmapEncoder();
                                je.Frames.Add(BitmapFrame.Create(tb));
                                je.QualityLevel = 50;
                                je.Save(ms);
                            }
                            else
                            {
                                JpegBitmapEncoder je = new JpegBitmapEncoder();
                                je.Frames.Add(bf);
                                je.QualityLevel = 50;
                                je.Save(ms);
                            }
                            byte[] baa = new byte[ms.Length + 4];
                            Array.Copy(BitConverter.GetBytes(ms.Length), baa, 4);
                            Array.Copy(ms.ToArray(), 0, baa, 4, ms.Length);
                            SendMessagePacket sm = new SendMessagePacket(roomNum, 1, baa);
                            NetWorkManager.Instance.SendSync(Serialization.Serialize(sm));
                            continue;
                        }
                    }

                    // 1. 파일 용량 전송 
                    // 2. 파일 전송
                    // 3. 메시지에 표시될 파일 정보 전송 (용량, 파일 이름, 썸네일)

                    //파일 이름, 파일 크기 데이터에 넣기

                    string splits = DropFiles[i].Split('\\').Last();
                    byte[] filename = Encoding.UTF8.GetBytes(splits);

                    byte[] ba = new byte[filename.Length + 4];
                    Array.Copy(BitConverter.GetBytes(fileSize), ba, 4);
                    Array.Copy(filename, 0, ba, 4, filename.Length);
                    NetWork.SendMessagePacket smp = new SendMessagePacket(roomNum, 2, ba);
                    NetWorkManager.Instance.SendSync(Serialization.Serialize(smp));
                }
            }
            NetWorkManager.Instance.isSend = false;
            CloseAction();
        }
    }

    [ValueConversion(typeof(bool), typeof(System.Windows.Visibility))]
    public class ListTextVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return string.IsNullOrEmpty((string)value) ? Visibility.Collapsed : Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
