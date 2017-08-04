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

            
            var printMaster = new PrintMaster(master, 0.013f, 0.013f, new System.Drawing.SizeF(100, 100));
            printMaster.PrintSync(1);

            Console.WriteLine("ready to end");
            Console.ReadKey();
        }
    }
}
//printSize.Width / xmm / imageSize.Width
//printSize.Width / xmm / imageSize.Width)