using CWA.DTP;
using CWA.DTP.Plotter;
using System;

namespace TestsForLib
{
    public class Program
    {
        static void Main(string[] args)
        {

            if (!SerialPacketReader.FirstAvailable(5000, out var reader, out var writer))
            {
                Console.WriteLine("cant find any device");
                return;
            }
            var master = new DTPMaster(reader, writer);
            var config = new PlotterConfig(master);
            Console.WriteLine(config.Options);

            config.Options.PinMappingYStep = 66;
            config.Options.PinMappingXStep = 20;
            config.Options.PinMappingZDirection = 128;

            config.Upload();

            Console.ReadKey();

            config = new PlotterConfig(master);

            Console.WriteLine(config.Options);

            Console.ReadKey();
        }
    }
}
