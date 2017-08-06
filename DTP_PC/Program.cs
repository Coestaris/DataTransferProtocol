using CWA.DTP;
using CWA.DTP.Plotter;
using CWA.Vectors;
using System;
using System.IO;
using System.Linq;

namespace TestsForLib
{
    public class Program
    {
        static unsafe void Main(string[] args)
        {
            if (!SerialPacketReader.FirstAvailable(5000, out var reader, out var writer))
            {
                Console.WriteLine("cant find any device");
                return;
            }
            var master = new DTPMaster(reader, writer);
            var contentMaster = new PlotterContent(master);

            var vector = new Vector("Nvidia_logo.pcv");
            //var clvector = vector.ClearThisVector(5);

            contentMaster.UploadVector(vector, "NVidia Logo");

            //var config = new PlotterConfig(master);
            //config.Options.IdleDelay = 100;
            //config.Options.WorkDelay = 100;
            //config.Upload();
            
           
            var printMaster = new PrintMaster(master, 0.013f, 0.013f, 5000);
            printMaster.SetXSize(150);
            printMaster.BeginPrinting(4);
         
            Console.WriteLine("ready to end");
            Console.ReadKey();
        }
    }
}
//printSize.Width / xmm / imageSize.Width
//printSize.Width / xmm / imageSize.Width)