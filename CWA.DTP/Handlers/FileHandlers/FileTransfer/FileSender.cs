﻿/*
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

namespace CWA.DTP.FileTransfer
{
    public class FileSender
    {
        private byte[] _data;

        private bool CheckSum { get; set; } = true;

        private bool CheckLen { get; set; } = true;

        public int PacketLength { get; set; } = 3200;

        internal PacketHandler BaseHandler;

        internal FileSender(int _packetLength, FileSenderSecurityFlags flags)
        {
            CheckSum = flags.HasFlag(FileSenderSecurityFlags.VerifyCheckSum);
            CheckLen = flags.HasFlag(FileSenderSecurityFlags.VerifyLengh);
            PacketLength = _packetLength;
        }

        internal FileSender(FileSenderSecurityFlags flags)
        {
            CheckSum = (flags & FileSenderSecurityFlags.VerifyCheckSum) != 0;
            CheckLen = (flags & FileSenderSecurityFlags.VerifyLengh) != 0;
        }

        public event FileTrasferProcessHandler SendingProcessChanged;

        public event FileTrasferEndHandler SendingEnd;

        public event FileSenderErrorHandler SendingError;

        private void RaiseProcessEvent(FileTransferProcessArgs arg)
        {
            Counter = (int)arg.PacketSended;
            Total = arg.PacketSended + arg.PacketsLeft;
            SendingProcessChanged?.Invoke(arg);
        }

        private void RaiseEndEvent(FileTransferEndArgs arg)
        {
            TimerThread.Abort();
            SendingEnd?.Invoke(arg);
        }

        private void RaiseErrorEvent(FileSenderErrorArgs arg)
        {
            SendingError?.Invoke(arg);
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

        private bool HandleFiles(string NewName)
        {
            var res = BaseHandler.File_Create(NewName);
            if (res == PacketHandler.FileDirHandleResult.OK) { return true; }
            else if (res == PacketHandler.FileDirHandleResult.Fail)
            {
                RaiseErrorEvent(new FileSenderErrorArgs(FileSenderError.CantCreateFile, true));
                return false;
            }
            else
            {
                if (BaseHandler.File_Delete(NewName) == PacketHandler.FileDirHandleResult.Fail)
                {
                    RaiseErrorEvent(new FileSenderErrorArgs(FileSenderError.CantDeleteFile, true));
                    return false;
                }
                if (BaseHandler.File_Create(NewName) == PacketHandler.FileDirHandleResult.Fail)
                {
                    RaiseErrorEvent(new FileSenderErrorArgs(FileSenderError.CantCreateFile, true));
                    return false;
                }
            }
            if (BaseHandler.File_Open(NewName, true) != PacketHandler.WriteReadFileHandleResult.OK)
            {
                RaiseErrorEvent(new FileSenderErrorArgs(FileSenderError.CantOpenFile, true));
                return false;
            };
            return true;
        }
        
        private bool CompareLength()
        {
            var sizeResult = BaseHandler.File_GetLength();
            if (sizeResult.Status == PacketHandler.FileDirHandleResult.Fail)
            {
                RaiseErrorEvent(new FileSenderErrorArgs(FileSenderError.CantGetFileSize, true));
                return false;
            }
            if (sizeResult.Length != _data.Length)
            {
                RaiseErrorEvent(new FileSenderErrorArgs(FileSenderError.NotEqualSizes, true));
                return false;
            }
            return true;
        }

        private bool CompareHash()
        {
            var crcres = BaseHandler.File_GetCrC16(PacketHandler.HashAlgorithm.CRC32);
            if (crcres.Status != PacketHandler.WriteReadFileHandleResult.OK)
            {
                RaiseErrorEvent(new FileSenderErrorArgs(FileSenderError.CantGetHashOfFile, true));
                return false;
            }
            var localcrc = new CrCHandler().ComputeChecksumBytes(_data);
            if (crcres.Result[0] != localcrc[0] || crcres.Result[1] != localcrc[1]) RaiseErrorEvent(new FileSenderErrorArgs(FileSenderError.HashesNotEqual, false));
            return true;
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

        public bool SendFileSync(string pcName, string NewName)
        {
            TimerThread = new Thread(TimerThreadMethod);
            TimerThread.Start();
            DateTime startTime = DateTime.Now;
            if (!HandleFiles(NewName)) return false;
            _data = File.ReadAllBytes(pcName);
            var b = _data.Split(PacketLength);
            int totalCount = b.Count();
            int Current = 0;
            foreach (var c in b)
            {
                Current++;
                if (!BaseHandler.File_Append(c.ToArray()))
                {
                    RaiseErrorEvent(new FileSenderErrorArgs(FileSenderError.CantSendPacket, true));
                    return false;
                }
                RaiseProcessEvent(new FileTransferProcessArgs((long)(DateTime.Now - startTime).TotalSeconds, LeftTime, totalCount - Current, Current, Speed, PacketLength));
            }
            if (CheckLen)
            {
                if (!CompareLength()) return false;
                if (!BaseHandler.File_Close())
                {
                    RaiseErrorEvent(new FileSenderErrorArgs(FileSenderError.CantCloseFile, true));
                    return false;
                };
            }
            if (CheckSum)
            {
                if (!CompareHash()) return false;
            }
            RaiseEndEvent(new FileTransferEndArgs((DateTime.Now - startTime).TotalSeconds));
            _data = null;
            return true;
        }

        public void SendFileAsync(string pcName, string NewName)
        {
            SenderThread = new Thread(p =>
            {
                SendFileSync(pcName, NewName);
            });
            SenderThread.Start();
        }
    }
}