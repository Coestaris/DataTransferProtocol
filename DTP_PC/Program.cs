using Compresser;
using CWA.DTP;
using CWA.DTP.Plotter;
using CWA.Vectors;
using System;
using System.IO;

namespace TestsForLib
{
    public class Program
    {
        static unsafe void Main(string[] args)
        {
            /*   if (!SerialPacketReader.FirstAvailable(5000, out var reader, out var writer))
               { }
               var master = new DTPMaster(reader, writer);

               var plotterContent = new PlotterContent(master);
               FlFormat file1 = new FlFormat();
               file1.Elements.AddRange(new FlFormatElement[]
               {
                   new FlFormatElement(100,200,300,400),
                   new FlFormatElement(200,300,400,500),
                   new FlFormatElement(300,400,500,600),
                   new FlFormatElement(400,500,600,700),
                   new FlFormatElement(500,600,700,800),
                   new FlFormatElement(600,700,800,900),
                   new FlFormatElement(700,800,900,1000),
                   new FlFormatElement(800,900,1000,1100),
               });
               FlFormat file2 = new FlFormat();
               file2.Elements.AddRange(new FlFormatElement[]
               {
                   new FlFormatElement(100,200,300,400),
                   new FlFormatElement(200,300,400,500),
                   new FlFormatElement(300,400,500,600),
                   new FlFormatElement(400,500,600,700),
                   new FlFormatElement(500,600,700,800),
                   new FlFormatElement(600,700,800,900),
                   new FlFormatElement(700,800,900,1000),
                   new FlFormatElement(800,900,1000,1100),
               });
               FlFormat file3 = new FlFormat();
               file3.Elements.AddRange(new FlFormatElement[]
               {
                   new FlFormatElement(100,200,300,400),
                   new FlFormatElement(200,300,400,500),
                   new FlFormatElement(300,400,500,600),
                   new FlFormatElement(400,500,600,700),
                   new FlFormatElement(500,600,700,800),
                   new FlFormatElement(600,700,800,900),
                   new FlFormatElement(700,800,900,1000),
                   new FlFormatElement(800,900,1000,1100),
               });

               plotterContent.UploadFlFormatFiles(new FlFormat[] { file1, file2, file3 }, true);
               Console.ReadKey();*/
            // foreach (var item in Directory.GetFiles("d:\\CODING\\CnC_WFA\\PlotterControl\\bin\\Debug\\Data\\Vect\\", "*.pcv"))
            //{
            //var a = new Vector(item);
            //a.Save(item, VectorFileFormat.OPCV);
            //}

            //Compresser.Compresser.DeCompress("d:\CODING\CnC_WFA\PlotterControl\bin\\Debug\\Data\\Vect\\")

            var master = DTPMaster.CreateFromSerial(1000, new Sender("1234567"), false);
            if (master == null)
                 throw new ArgumentNullException(nameof(master));

            master.SecurityManager.Validate(new SecurityKey("key123"));

            var a = new MovingControl(master);
            a.TurnOnEngines();

            /*
            if (!master.SecurityManager.IsValidationRequired)
            {
                Console.WriteLine("Validation is not required");
                Console.ReadKey();
                return;
            }

            Console.WriteLine("Нажмите любую кнопку устройства в течении 3 секунд, после нажатия на любую клавишу (убедитесь, что питание включено)...");
            Console.ReadKey();
            if(master.SecurityManager.ResetKey()) Console.WriteLine("Ok");
            else Console.WriteLine("Fail");*/
            Console.ReadKey();
        }
    }
}
