/*
    The MIT License(MIT)

    Copyright (c) 2016 - 2017 Kurylko Maxim Igorevich

    Permission is hereby granted, free of charge, to any person obtaining a copy
    of this software and associated documentation files (the "Software"), to deal
    in the Software without restriction, including without limitation the rights
    to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
    copies of the Software, and to permit persons to whom the Software is
    furnished to do so, subject to the following conditions:
    
    The above copyright notice and this permission notice shall be included in
    all copies or substantial portions of the Software.
    
    THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
    IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
    FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
    AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
    LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
    OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
    THE SOFTWARE.

*/

using System;
using System.IO;
using System.Linq;
using System.Threading;

namespace CWA.DTP
{
    public class FileSender
    {
        private byte[] _data;

        private bool CheckSum { get; set; } = true;

        [Flags]
        public enum SecurityFlags
        {
            VerifyCheckSum = 0,
            VerifyLengh = 1
        }

        private bool CheckLen { get; set; } = true;

        public int PacketLength { get; set; } = 3200;

        public PacketHandler BaseHandler { get; private set; }
        
        public FileSender(PacketHandler base_, int _packetLength, SecurityFlags flags)
        {
            CheckSum = flags.HasFlag(SecurityFlags.VerifyCheckSum);
            CheckLen = flags.HasFlag(SecurityFlags.VerifyLengh);
            BaseHandler = base_;
            PacketLength = _packetLength;
        }

        public FileSender(PacketHandler base_, SecurityFlags flags)
        {
            CheckSum = (flags & SecurityFlags.VerifyCheckSum) != 0;
            CheckLen = (flags & SecurityFlags.VerifyLengh) != 0;
            BaseHandler = base_;
        }

        public class ProcessArgs : EventArgs
        {
            public long TimeSpend { get; private set; }
            public long TimeLeft { get; private set; }
            public long PacketsLeft { get; private set; }
            public long PacketSended { get; private set; }

            public double Speed { get; private set; } //bytes per sec
            public int PacketsLength { get; private set; }

            public ProcessArgs(long timeSpend, long timeLeft, long packetLeft, long packetSended, double speed, int packetLength)
            {
                this.TimeSpend = timeSpend;
                this.TimeLeft = timeLeft;
                this.PacketsLeft = packetLeft;
                this.PacketSended = packetSended;
                this.Speed = speed;
                this.PacketsLength = packetLength;
            }
        }

        public class EndArgs : EventArgs
        {
            public double TimeSpend { get; private set; }

            public EndArgs(double timeSpend)
            {
                this.TimeSpend = timeSpend;
            }
        }

        public class ErrorArgs : EventArgs
        {
            public SendError Error { get; private set; }
            public bool IsCritical { get; private set; }

            public ErrorArgs(SendError error, bool isCritical)
            {
                this.Error = error;
                IsCritical = isCritical;
            }
        }

        public delegate void ProcessHandler(ProcessArgs arg);

        public delegate void EndHandler(EndArgs arg);

        public delegate void ErrorHandler(ErrorArgs arg);

        public event ProcessHandler SendingProcessChanged;

        public event EndHandler SendingEnd;

        public event ErrorHandler SendingError;

        private void RaiseProcessEvent(ProcessArgs arg)
        {
            SendingProcessChanged?.Invoke(arg);
        }

        private void RaiseEndEvent(EndArgs arg)
        {
            SendingEnd?.Invoke(arg);
        }

        private void RaiseErrorEvent(ErrorArgs arg)
        {
            SendingError?.Invoke(arg);
        }

        public enum SendError
        {
            CantSendPacket,
            CantCreateFile,
            CantDeleteFile,
            CantOpenFile,
            CantCloseFile,
            NotEqualSizes,
            CantGetHashOfFile,
            HashesNotEqual,
            CantGetFileSize,
        }

        private bool HandleFiles(string NewName)
        {
            var res = BaseHandler.File_Create(NewName);
            if (res == PacketHandler.FileDirHandleResult.OK) { return true; }
            else if (res == PacketHandler.FileDirHandleResult.Fail)
            {
                RaiseErrorEvent(new ErrorArgs(SendError.CantCreateFile, true));
                return false;
            }
            else
            {
                if (BaseHandler.File_Delete(NewName) == PacketHandler.FileDirHandleResult.Fail)
                {
                    RaiseErrorEvent(new ErrorArgs(SendError.CantDeleteFile, true));
                    return false;
                }
                if (BaseHandler.File_Create(NewName) == PacketHandler.FileDirHandleResult.Fail)
                {
                    RaiseErrorEvent(new ErrorArgs(SendError.CantCreateFile, true));
                    return false;
                }
            }
            if (BaseHandler.File_Open(NewName, true) != PacketHandler.WriteReadFileHandleResult.OK)
            {
                RaiseErrorEvent(new ErrorArgs(SendError.CantOpenFile, true));
                return false;
            };
            return true;
        }
        
        private bool CompareLength()
        {
            var sizeResult = BaseHandler.File_GetLength();
            if (sizeResult.Status == PacketHandler.FileDirHandleResult.Fail)
            {
                RaiseErrorEvent(new ErrorArgs(SendError.CantGetFileSize, true));
                return false;
            }
            if (sizeResult.Length != _data.Length)
            {
                RaiseErrorEvent(new ErrorArgs(SendError.NotEqualSizes, true));
                return false;
            }
            return true;
        }

        private bool CompareHash()
        {
            var crcres = BaseHandler.File_GetCrC16(PacketHandler.HashAlgorithm.CRC32);
            if (crcres.Status != PacketHandler.WriteReadFileHandleResult.OK)
            {
                RaiseErrorEvent(new ErrorArgs(SendError.CantGetHashOfFile, true));
                return false;
            }
            var localcrc = new CrCHandler().ComputeChecksumBytes(_data);
            if (crcres.Result[0] != localcrc[0] || crcres.Result[1] != localcrc[1]) RaiseErrorEvent(new ErrorArgs(SendError.HashesNotEqual, false));
            return true;
        }

        public bool SendFileSync(string pcName, string NewName)
        {
            DateTime startTime = DateTime.Now;
            if (!HandleFiles(NewName)) return false;
            _data = System.IO.File.ReadAllBytes(pcName);
            var b = _data.Split(PacketLength);
            int totalCount = b.Count();
            int Current = 0;
            foreach (var c in b)
            {
                Current++;
                if (!BaseHandler.File_Append(c.ToArray()))
                {
                    RaiseErrorEvent(new ErrorArgs(SendError.CantSendPacket, true));
                    return false;
                }
                RaiseProcessEvent(new ProcessArgs((long)(DateTime.Now - startTime).TotalSeconds, 0, totalCount - Current, Current, 0, PacketLength));
            }
            if (CheckLen)
            {
                if (!CompareLength()) return false;
                if (!BaseHandler.File_Close())
                {
                    RaiseErrorEvent(new ErrorArgs(SendError.CantCloseFile, true));
                    return false;
                };
            }
            if (CheckSum)
            {
                if (!CompareHash()) return false;
            }
            RaiseEndEvent(new EndArgs((DateTime.Now - startTime).TotalSeconds));
            _data = null;
            return true;
        }

        public void SendFileAsync(string pcName, string NewName)
        {
           new Thread(p => 
           {
               SendFileSync(pcName, NewName);
           }).Start();
        }
    }
}
