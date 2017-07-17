using CWA.DTP.FileTransfer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CWA.DTP
{
    public sealed class DTPMaster
    {
        internal PacketHandler ph;

        public DTPMaster(IPacketReader reader, IPacketWriter writer)
        {
            ph = new PacketHandler(new Sender(SenderType.SevenByteName), new PacketListener(reader, writer));
            Device = new DeviceControl() { ParentMaster = this };
        }

        public DTPMaster(IPacketReader reader, IPacketWriter writer, string SenderName)
        {
            ph = new PacketHandler(new Sender(SenderType.SevenByteName, SenderName), new PacketListener(reader, writer));
            Device = new DeviceControl() { ParentMaster = this };
        }

        public DTPMaster(Sender sender, PacketListener listener)
        {
            ph = new PacketHandler(sender, listener);
            Device = new DeviceControl() { ParentMaster = this };
        }

        public DeviceControl Device { get; private set; }

        public FileSender CreateFileSender(FileSenderSecurityFlags flags)
        {
            return new FileSender(flags)
            {
                BaseHandler = ph
            };
        }

        public SdCardDirectory CreateDirectoryHandler(string Path)
        {
            return new SdCardDirectory(Path, ph);
        }

        public SdCardDirectory CreateDirectoryHandlerFromRoot()
        {
            return new SdCardDirectory("/", ph);
        }

        public SdCardFile CreateFileHandler(string Path)
        {
            return new SdCardFile(Path, ph);
        }
    }
}
