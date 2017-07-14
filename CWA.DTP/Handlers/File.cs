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

using System.Collections.Generic;
using System.Linq;
using System;

namespace CWA.DTP
{
    public sealed class SdCardBinnaryFile
    {
        internal PacketHandler ph;
        internal SdCardFile ParentFile;
        private int cacheLength;

        internal SdCardBinnaryFile(SdCardFile ParentFile, PacketHandler ph)
        {
            this.ph = ph;
            this.ParentFile = ParentFile;
            cacheLength = (int)ParentFile.Length;
        }

        public int CursorPos { get; set; }

        public bool Write(byte val)
        {
            if(ph.File_Append(new byte[1] { val }))
            {
                cacheLength += 1;
                CursorPos += 1;
                return true;
            }
            return false;
        }

        public bool Write(short val)
        {
            if (ph.File_Append(new byte[2]
                {
                    (byte)(val & 0xFF),
                    (byte)((val >> 8) & 0xFF),
                }))
            {
                cacheLength += 2;
                CursorPos += 2;
                return true;
            }
            return false;
        }

        public bool Write(int val)
        {
            if (ph.File_Append(new byte[4]
               {
                    (byte)(val & 0xFF),
                    (byte)((val >> 8) & 0xFF),
                    (byte)((val >> 16) & 0xFF),
                    (byte)((val >> 24) & 0xFF),
               }))
            {
                cacheLength += 4;
                CursorPos += 4;
                return true;
            }
            return false;
        }

        public bool Write(long val)
        {
            if (ph.File_Append(new byte[8]
                {
                    (byte)(val & 0xFF),
                    (byte)((val >> 8) & 0xFF),
                    (byte)((val >> 16) & 0xFF),
                    (byte)((val >> 24) & 0xFF),
                    (byte)((val >> 48) & 0xFF),
                    (byte)((val >> 96) & 0xFF),
                    (byte)((val >> 192) & 0xFF),
                    (byte)((val >> 384) & 0xFF),
                }))
            {
                cacheLength += 8;
                CursorPos += 8;
                return true;
            }
            return false;
        }

        public bool Write(float val)
        {
            int lenOfType = sizeof(float);
            if (ph.File_Append(BitConverter.GetBytes(val)))
            {
                CursorPos += lenOfType;
                cacheLength += 4;
                return true;
            }
            return false;
        }

        public bool Write(double val)
        {
            int lenOfType = sizeof(double);
            if (ph.File_Append(BitConverter.GetBytes(val)))
            {
                CursorPos += lenOfType;
                cacheLength += 8;
                return true;
            }
            return false;
        }

        public bool Write(bool val)
        {
            if (ph.File_Append(new byte[1] { val ? (byte)1 : (byte)0 }))
            {
                CursorPos += 1;
                return true;
            }
            return false;
        }

        public bool Write(char val)
        {
            if (ph.File_Append( new byte[1] { (byte)val }))
            {
                cacheLength += 1;
                CursorPos += 1;
                return true;
            }
            return false;
        }
        
        public byte Read(out bool status)
        {
            if (CursorPos + 1 > cacheLength)
                throw new ArgumentOutOfRangeException();
            var res = ph.File_Read(CursorPos, 1);
            if(res.Status == PacketHandler.WriteReadFileHandleResult.OK)
            {
                CursorPos += 1;
                status = true;
                return res.Result[0];
            }
            status = false;
            return 0;
        }
    }

    public class FileHandlerException : Exception
    {
        private string _Message;

        public override string Message
        {
            get { return _Message; }
        }

        public FileHandlerException(string Message)
        {
            _Message = Message;
        }

        public FileHandlerException() { }
    }


    public class FailOperationException : Exception
    {
        public override string Message
        {
            get { return _Message; }
        }

        private string _Message;

        public object EnumStatus { get; }

        public FailOperationException() { }

        public FailOperationException(string Message)
        {
            _Message = Message;
        }

        public FailOperationException(object EnumMessage)
        {
            EnumStatus = EnumMessage;
        }

        public FailOperationException(string Message, object EnumMessage)
        {
            _Message = Message;
            EnumStatus = EnumMessage;
        }
    }

    public class SdCardFile
    {
        private static bool IsGlobalOpenedFiles = false;

        private PacketHandler ph;
        
        public string FilePath { get; set; }

        public SdCardFile(string path, PacketHandler ph)
        {
            FilePath = path;
            this.ph = ph;
        }

        public bool IsOpen { get; private set; }

        public bool IsExists
        {
            get
            {
                var res = ph.File_Exists(FilePath);
                if (res == PacketHandler.FileExistsResult.Fail)
                    throw new FailOperationException(res);
                return res == PacketHandler.FileExistsResult.Exists;
            }
        }

        public SdCardFile Open()
        {
            try
            {
                if(!IsExists)
                    throw new FileHandlerException("Файл не существует");
                if (IsOpen)
                    throw new FileHandlerException("Файл уже был открыть");
                if (IsGlobalOpenedFiles)
                    throw new FailOperationException("На этом домене уже есть открытый файл. Невозможно отрыть более однго файлов");
                var res = ph.File_Open(FilePath, false);
                if (res != PacketHandler.WriteReadFileHandleResult.OK)
                    throw new FailOperationException("Не удалось открыть файл", res);
                IsOpen = true;
                IsGlobalOpenedFiles = true;
                return this;
            }
            catch(Exception e)
            {
                throw e;
            }
        }

        public void Close()
        {
            try
            {
                if(!IsOpen)
                    throw new FileHandlerException("Файл закрыт");
                var res = ph.File_Close();
                if (!res)
                    throw new FailOperationException("Не удалось закрыть файл");
                IsOpen = false;
                IsGlobalOpenedFiles = false;
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public long Length
        {
            get
            {
                try
                {
                    if (!IsOpen)
                        throw new FileHandlerException("Файл закрыт");
                    var res = ph.File_GetLength();
                    if(res.Status != PacketHandler.FileDirHandleResult.OK)
                        throw new FailOperationException("Не удалось получить длину (размер) файла", res);
                    return res.Length;
                }
                catch (Exception e)
                {
                    throw e;
                }
            }
        }

        public void Create()
        {
            try
            {
                var res = ph.File_Create(FilePath);
                if(res != PacketHandler.FileDirHandleResult.OK)
                    throw new FailOperationException("Не удалось создать файл", res);
            }
            catch(Exception e)
            {
                throw e;
            }
        }

        public void ClearAllBytes()
        {
            try
            {
                if(IsOpen) if(!ph.File_Close()) throw new FailOperationException("Не удалось закрыть файл");
                var res = ph.File_Open(FilePath, true);
                if(res != PacketHandler.WriteReadFileHandleResult.OK)
                    throw new FailOperationException("Не удалось открыть файл", res);
                if (!IsOpen) if (!ph.File_Close()) throw new FailOperationException("Не удалось закрыть файл");
            }
            catch(Exception e)
            {
                throw e;
            }
        }

        public void Delete()
        {
            try
            {
                var res = ph.File_Delete(FilePath);
            if (res != PacketHandler.FileDirHandleResult.OK)
                throw new FailOperationException(res);
            }
            catch(Exception e)
            {
                throw e;
            }
        }

        public PacketHandler.PacketAnswerFileInfo FileInfo
        {
            get
            {
                try
                {
                    var res = ph.File_GetInfo(FilePath);
                    return res;
                }
                catch(Exception e)
                {
                    throw e;
                }
            }
        }
        
        public SdCardBinnaryFile GetBinnaryFile()
        {
            if (IsOpen)
                return new SdCardBinnaryFile(this, ph);
            else throw new FileHandlerException("Файл закрыт");
        }
        
    }
}
