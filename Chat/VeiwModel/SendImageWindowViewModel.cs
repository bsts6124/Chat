using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Media.Imaging;
using NetWork;

namespace Client
{
    class SendImageWindowViewModel : ViewModelBase
    {
        int roomNum;
        public SendImageWindowViewModel(int room)
        {
            //IDataObject clipboardData = Clipboard.GetDataObject();
            try
            {
                //clipImage = Clipboard.GetImage();
                clipImage = ImageFromClipboardDib();
            }
            catch
            {
                MessageBox.Show("사진이 이상한대요?");
                return;
            }
            roomNum = room;
            SendCommand = new Command.DelegateCommand(ImageSend);
        }

        ~SendImageWindowViewModel()
        {
            SendCommand = null;
            clipImage = null;
            GC.Collect();
        }
        bool canSend = true;
        public BitmapSource clipImage { get; set; }

        private BitmapSource ImageFromClipboardDib()
        {
            using (MemoryStream ms = Clipboard.GetData("DeviceIndependentBitmap") as MemoryStream)
            {
                if (ms != null)
                {
                    byte[] dibBuffer = new byte[ms.Length];
                    ms.Read(dibBuffer, 0, dibBuffer.Length);

                    BITMAPINFOHEADER infoHeader = BinaryStructConverter.FromByteArray<BITMAPINFOHEADER>(dibBuffer);

                    int fileHeaderSize = Marshal.SizeOf(typeof(BITMAPFILEHEADER));
                    int infoHeaderSize = infoHeader.biSize;
                    int fileSize = fileHeaderSize + infoHeader.biSize + infoHeader.biSizeImage;

                    BITMAPFILEHEADER fileHeader = new BITMAPFILEHEADER();
                    fileHeader.bfType = BITMAPFILEHEADER.BM;
                    fileHeader.bfSize = fileSize;
                    fileHeader.bfReserved1 = 0;
                    fileHeader.bfReserved2 = 0;
                    fileHeader.bfOffBits = fileHeaderSize + infoHeaderSize + infoHeader.biClrUsed * 4;

                    byte[] fileHeaderBytes =
                        BinaryStructConverter.ToByteArray<BITMAPFILEHEADER>(fileHeader);

                    MemoryStream msBitmap = new MemoryStream();
                    msBitmap.Write(fileHeaderBytes, 0, fileHeaderSize);
                    msBitmap.Write(dibBuffer, 0, dibBuffer.Length);
                    msBitmap.Seek(0, SeekOrigin.Begin);

                    if (msBitmap.Length >= 10737418236) canSend = false;

                    return BitmapFrame.Create(msBitmap);
                }
            }

            return null;
        }

        public Command.DelegateCommand SendCommand { get; set; }

        void ImageSend(object o)
        {
            /*PngBitmapEncoder ec = new PngBitmapEncoder();
           using (MemoryStream ms = new MemoryStream())
           {
               ec.Frames.Add(BitmapFrame.Create(clipImage));
               ec.Save(ms);
               List<byte[]> bl = new List<byte[]>();

               byte[] bitmaparray = ms.ToArray();
               bl.Add(BitConverter.GetBytes(bitmaparray.Length));
               bl.Add(bitmaparray);

               byte[] format = Encoding.UTF8.GetBytes("png");
               bl.Add(BitConverter.GetBytes(format.Length));
               bl.Add(format);

               byte[] data = bl.SelectMany(a => a).ToArray();
               NetWork.SendMessagePacket m = new NetWork.SendMessagePacket(roomNum, 1, data);
               NetWork.NetWorkManager.Instance.Send(m);
           }
            */
            using(MemoryStream ms = new MemoryStream())
            {
                BitmapFrame bf = BitmapFrame.Create(clipImage);

                PngBitmapEncoder pe = new PngBitmapEncoder();
                pe.Frames.Add(bf);
                pe.Save(ms);
                int fileSize = (int)ms.Length;
                NetWorkManager.Instance.SendSync(Serialization.Serialize(new PrevFilePacket(fileSize)));

                byte[] buffer = new byte[NetWorkManager.Instance.buffersize - 5];
                int currentOffset = 0;
                while (fileSize > currentOffset)
                {
                    if (fileSize - currentOffset > buffer.Length)
                    {
                        ms.Read(buffer, 0, buffer.Length);
                        currentOffset += buffer.Length;
                    }
                    else
                    {
                        buffer = new byte[fileSize - currentOffset];
                        ms.Read(buffer, 0, fileSize - currentOffset);
                        currentOffset = fileSize;
                    }

                    FilePacket fp = new FilePacket(buffer);

                    NetWorkManager.Instance.SendSync(fp.fileData);
                    Array.Clear(buffer, 0, buffer.Length);
                }
                
                if (canSend == true)
                {
                    ms.SetLength(0);
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
                } 
                else
                {
                    byte[] filename = Encoding.UTF8.GetBytes(DateTime.Now.ToBinary().ToString());

                    byte[] ba = new byte[filename.Length + 4];
                    Array.Copy(BitConverter.GetBytes(fileSize), ba, 4);
                    Array.Copy(filename, 0, ba, 4, filename.Length);
                    NetWork.SendMessagePacket smp = new SendMessagePacket(roomNum, 2, ba);
                    NetWorkManager.Instance.SendSync(Serialization.Serialize(smp));
                }
            }

            CloseAction();
        }

        public Action CloseAction { get; set; }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 2)]
    struct BITMAPFILEHEADER
    {
        public static readonly short BM = 0x4d42; // BM

        public short bfType;
        public int bfSize;
        public short bfReserved1;
        public short bfReserved2;
        public int bfOffBits;
    }

    [StructLayout(LayoutKind.Sequential)]
    struct BITMAPINFOHEADER
    {
        public int biSize;
        public int biWidth;
        public int biHeight;
        public short biPlanes;
        public short biBitCount;
        public int biCompression;
        public int biSizeImage;
        public int biXPelsPerMeter;
        public int biYPelsPerMeter;
        public int biClrUsed;
        public int biClrImportant;
    }

    public class BinaryStructConverter
    {
        public static T FromByteArray<T>(byte[] bytes) where T : struct
        {
            IntPtr ptr = IntPtr.Zero;
            try
            {
                int size = Marshal.SizeOf(typeof(T));
                ptr = Marshal.AllocHGlobal(size);
                Marshal.Copy(bytes, 0, ptr, size);
                object obj = Marshal.PtrToStructure(ptr, typeof(T));
                return (T)obj;
            }
            finally
            {
                if (ptr != IntPtr.Zero)
                    Marshal.FreeHGlobal(ptr);
            }
        }

        public static byte[] ToByteArray<T>(T obj) where T : struct
        {
            IntPtr ptr = IntPtr.Zero;
            try
            {
                int size = Marshal.SizeOf(typeof(T));
                ptr = Marshal.AllocHGlobal(size);
                Marshal.StructureToPtr(obj, ptr, true);
                byte[] bytes = new byte[size];
                Marshal.Copy(ptr, bytes, 0, size);
                return bytes;
            }
            finally
            {
                if (ptr != IntPtr.Zero)
                    Marshal.FreeHGlobal(ptr);
            }
        }
    }
}

// 출처 https://thomaslevesque.com/2009/02/05/wpf-paste-an-image-from-the-clipboard/