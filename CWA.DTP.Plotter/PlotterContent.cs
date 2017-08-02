using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CWA.DTP.Plotter
{
    public class PlotterContent
    {
        internal DTPMaster Master;

        public PlotterContent(DTPMaster master)
        {
            Master = master;
            Init();
        }

        public UInt16 CountOfVectors { get; private set; }

        private void Init()
        {
            var root = Master.CreateDirectoryHandlerFromRoot();
            var files = root.SubFiles;
            UInt16 i = 0;
            while (files.Select(p=>p.FilePath).Contains(i + ".v")) { i++; };
            CountOfVectors = i;
        }

        public VectorMetaData GetVectorMetaData(UInt16 index)
        {
            if (index > CountOfVectors) throw new OutOfMemoryException();
            var file = Master.CreateFileHandler(index + ".m").Open(false);
            var readRes = file.BinnaryFile.ReadByteArray(file.Length);
            if (!readRes.Succeed) throw new FailOperationException("Ну удалось получить данные");
            return new VectorMetaData(readRes.Result, this);
        }

        public bool UploadVector(Vectors.Vector vector)
        {

        }
    }
}
