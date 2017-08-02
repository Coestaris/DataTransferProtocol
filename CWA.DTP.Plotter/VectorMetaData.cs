using System;
using CWA.Vectors;
using System.Drawing;
using System.Linq;
using System.IO;

namespace CWA.DTP.Plotter
{
    public class VectorMetaData
    {
        private static int CountOfPreviews = 0;

        public string Name { get; private set; }
        public VectType Type  { get; private set; }
        public UInt16 Height { get; private set; }
        public UInt16 Width { get; private set; }
        public Bitmap Preview { get; private set; }

        internal UInt16 Index;

        private PlotterContent Parrent;

        public void UploadPreview()
        {
            if (!Directory.Exists("Temp")) Directory.CreateDirectory("Temp");
            var fileSender = Parrent.Master.CreateFileReceiver(FileTransfer.FileTransferSecurityFlags.VerifyLengh);
            string FileName = string.Format("Temp//preview{0}.png", CountOfPreviews++);
            fileSender.ReceivingEnd += (e) =>
            {
                Preview = new Bitmap(FileName);
            };
            fileSender.ReceiveFileSync(FileName, Index + ".p");
        }

        internal VectorMetaData(byte[] data, PlotterContent parrent)
        {
            Parrent = parrent;
            UInt16 stringLen = (UInt16)(data[0] | (data[1] << 8));
            Type = (VectType)data[stringLen + 2];
            Height = (UInt16)(data[stringLen + 3] | (data[stringLen + 4] << 8));
            Width = (UInt16)(data[stringLen + 5] | (data[stringLen + 6] << 8));
            Name = new string(data.Skip(2).Take(stringLen).Select(p=>(char)p).ToArray());
        }

        internal byte[] ToByteArray()
        {
            var arr = new byte[Name.Length + 7];
            arr[0] = (byte)(Name.Length & 0xFF);
            arr[1] = (byte)((Name.Length >> 8) & 0xFF);
            Buffer.BlockCopy(Name.ToCharArray(), 0, arr, 2, Name.Length);
            arr[Name.Length + 2] = (byte)Type;
            arr[Name.Length + 3] = (byte)(Height & 0xFF);
            arr[Name.Length + 4] = (byte)((Height >> 8) & 0xFF);
            arr[Name.Length + 5] = (byte)(Height & 0xFF);
            arr[Name.Length + 6] = (byte)((Height >> 8) & 0xFF);
            return arr;
        }
    }
}
