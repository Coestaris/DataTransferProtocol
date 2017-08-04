using System;
using System.Drawing;

namespace CWA.DTP.Plotter
{
    public enum PrintErrorType
    {
        CantFoundFileWithSpecifiedIndex
    }

    public delegate void PrintErrorHandler(PrintErrorType arg);

    public class PrintMaster
    {
        public event PrintErrorHandler OnError;

        private void RaiseErrorEvent(PrintErrorType arg)
        {
            OnError?.Invoke(arg);
        }

        private PlotterPacketHandler ph;

        private DTPMaster Master;

        public float XMM { get; set; }
        public float YMM { get; set; }
        public SizeF ImageSize { get; set; }

        private float XCoef, YCoef;
        private PlotterContent ContentMaster;

        public PrintMaster(DTPMaster master, float Xmm, float Ymm, SizeF imageSize)
        {
            Master = master;
            ph = new PlotterPacketHandler(master.Sender, master.Listener);
            ImageSize = imageSize;
            XMM = Xmm;
            YMM = Ymm;
        }

        private void GetCoefficients(SizeF printSize)
        {
            XCoef = ImageSize.Width / XMM / printSize.Width;
            YCoef = ImageSize.Height / YMM / printSize.Height;
        }

        public void PrintSync(UInt16 Index)
        {
            if(ContentMaster == null)
                ContentMaster = new PlotterContent(Master);

            if (!ContentMaster.ContentTable.VectorAdresses.Contains(Index))
            {
                RaiseErrorEvent(PrintErrorType.CantFoundFileWithSpecifiedIndex);
                return;
            }

            VectorMetaData metaData = ContentMaster.GetVectorMetaData(Index);

            GetCoefficients(new SizeF(metaData.Width, metaData.Height));

            try
            {
                ph.StartPrinting(XCoef, YCoef, Index);
            }
            catch
            {
                Console.WriteLine("cant get respond");
            }
        }
    }
}