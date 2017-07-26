using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CWA.DTP.FileTransfer
{
    public class FileReceiver
    {
        private bool CheckSum { get; set; } = true;

        private bool CheckLen { get; set; } = true;

        public int PacketLength { get; set; } = 3200;

        internal PacketHandler BaseHandler;

        //
        internal FileReceiver(int _packetLength, FileTransferSecurityFlags flags)
        {
            CheckSum = flags.HasFlag(FileTransferSecurityFlags.VerifyCheckSum);
            CheckLen = flags.HasFlag(FileTransferSecurityFlags.VerifyLengh);
            PacketLength = _packetLength;
        }

        //
        internal FileReceiver(FileTransferSecurityFlags flags)
        {
            CheckSum = (flags & FileTransferSecurityFlags.VerifyCheckSum) != 0;
            CheckLen = (flags & FileTransferSecurityFlags.VerifyLengh) != 0;
        }

        public event FileTrasferProcessHandler ReceiveProcessChanged;

        public event FileTrasferEndHandler ReceivingEnd;

        public event FileRecieverErrorHandler ReceiveError;

        private void RaiseProcessEvent(FileTransferProcessArgs arg)
        {
            Counter = (int)arg.PacketTrasfered;
            Total = arg.PacketTrasfered + arg.PacketsLeft;
            ReceiveProcessChanged?.Invoke(arg);
        }

        private void RaiseEndEvent(FileTransferEndArgs arg)
        {
            TimerThread.Abort();
            ReceivingEnd?.Invoke(arg);
        }

        private void RaiseErrorEvent(FileReceiverErrorArgs arg)
        {
            ReceiveError?.Invoke(arg);
            if (arg.IsCritical)
            {
                SenderThread?.Abort();
                TimerThread.Abort();
            }
        }

        public void StopAsync()
        {
            if (SenderThread == null)
                throw new InvalidOperationException("Отправка либо не запущена, либо запущена как синхронный процесс (если так, то я хз как ты вызвал этот метод -_-)");
            SenderThread?.Abort();
            TimerThread.Abort();
        }

        private int Counter, CountOfData, LasProgress, OnseSecondProgress;
        private long Total;
        private double Speed, lSpeed, LeftTime, LastLeftTime;

        private void TimerThreadMethod()
        {
            while (true)
            {
                OnseSecondProgress = Counter - LasProgress;
                LasProgress = Counter;
                CountOfData += OnseSecondProgress;
                if (lSpeed == 0) Speed = (double)OnseSecondProgress * PacketLength / 1024 * 2;
                else Speed = (lSpeed + (double)OnseSecondProgress * PacketLength / 1024 * 2) / 2;
                lSpeed = Speed;
                if (LastLeftTime == 0 || LastLeftTime == float.PositiveInfinity)
                {
                    if (OnseSecondProgress == 0) LeftTime = float.PositiveInfinity;
                    else LeftTime = (Total - CountOfData) / OnseSecondProgress / 2;
                }
                else
                {
                    if (OnseSecondProgress == 0) LeftTime = float.PositiveInfinity;
                    else LeftTime = (LastLeftTime + (Total - CountOfData) / OnseSecondProgress / 2) / 2;
                }
                LastLeftTime = LeftTime;
                Thread.Sleep(500);
            }
        }

        private Thread TimerThread, SenderThread;

        public bool ReceiveFileSync(string pcName, string DeviceName)
        {
            TimerThread = new Thread(TimerThreadMethod);
            TimerThread.Start();
            DateTime startTime = DateTime.Now;
            var a = new SdCardFile(DeviceName, BaseHandler);
            try
            {
                a.Open(false);
            }
            catch
            {
                RaiseErrorEvent(new FileReceiverErrorArgs(FileReceiverError.CantOpenFile, true));
                return false;
            }
            var bf = a.BinnaryFile;
            bf.CursorPos = 0;
            byte[] buffer = new byte[a.Length];
            UInt32 len = 0;
            try
            {
                len = a.Length;
            }
            catch
            {
                RaiseErrorEvent(new FileReceiverErrorArgs(FileReceiverError.CantGetFileSize, true));
                return false;
            }
            UInt32 currentPacket = 0;
            UInt32 totalPackets = (UInt32)(len / PacketLength);
            UInt32 currIndex = 0, delta = 0, index = 0;
            while (currIndex < len)
            {
                if (currIndex + PacketLength > len)
                {
                    delta = len - currIndex;
                    currIndex = len;
                }
                else
                {
                    currIndex += (UInt32)PacketLength;
                    delta = (UInt32)PacketLength;
                }
                SdCardBinnaryFileReadResult<byte[]> res;
                try
                {
                    //Console.WriteLine("{0} {1}", currIndex, delta);
                    res = bf.ReadByteArray(delta);
                }
                catch
                {
                    RaiseErrorEvent(new FileReceiverErrorArgs(FileReceiverError.CantGetPacket, true));
                    return false;
                }
                if (!res.Succeed)
                {
                    RaiseErrorEvent(new FileReceiverErrorArgs(FileReceiverError.CantGetPacket, true));
                    return false;
                }
                else
                {
                    currentPacket++;
                    RaiseProcessEvent(new FileTransferProcessArgs((long)(DateTime.Now - startTime).TotalSeconds, LeftTime, totalPackets - currentPacket, currentPacket, Speed, PacketLength));
                    Buffer.BlockCopy(res.Result, 0, buffer, (int)index, (int)delta);
                    index += delta;
                }
            }
            File.Create(pcName).Close();
            File.WriteAllBytes(pcName, buffer);
            a.Close();
            RaiseEndEvent(new FileTransferEndArgs((DateTime.Now - startTime).TotalSeconds));
            return true;
        }


        public void ReceiveFileAsync(string pcName, string DeviceName)
        {
            SenderThread = new Thread(p =>
            {
                ReceiveFileSync(pcName, DeviceName);
            });
            SenderThread.Start();
        }

    }
}
