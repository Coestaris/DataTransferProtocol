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
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CWA.DTP
{
    public class PacketHandler
    {
        private static readonly byte[] EmptyData = { 1 };

        private PacketAnswer GetResult(CommandType command)
        {
            return Listener.SendAndListenPacket(Packet.GetPacket(command, EmptyData, Sender));
        }

        private PacketAnswer GetResult(CommandType command, byte[] data)
        {
            return Listener.SendAndListenPacket(Packet.GetPacket(command, data, Sender));
        }

        public static bool AutoSyncTime { get; set; } = true;

        public static bool StartTest { get; set; } = true;

        public Sender Sender { get; set; }

        public PacketListener Listener { get; set; }

        public PacketHandler(Sender sender, PacketListener listener)
        {
            Listener = listener;
            Sender = sender;
            if (StartTest)
            {
                if (!DeviceTest()) throw new Exception("Cant init"); //TODO: Exception
            }
            if (AutoSyncTime) Device_SyncTime();
        }

        #region Classes

        public abstract class PacketAnswerSpecialData
        {
            public PacketAnswer BaseAnswer { get; private set; }

            public CommandType Command { get; private set; }

            protected ArgumentException WrongTypeException = new ArgumentException("Wrong Type or Nullale answer");

            public bool IsEmpty { get; private set; } = true;

            protected bool Init(CommandType defType, PacketAnswer answer)
            {
                Command = defType;
                BaseAnswer = answer;
                IsEmpty = !answer.IsEmpty && answer.Command == defType && answer.Status != AnswerStatus.Error;
                return IsEmpty;
            }
        }

        public class PacketAnswerTotalInfo : PacketAnswerSpecialData
        {
            public Board Board { get; private set; }
            public BoardArchitecture BoardArchitecture { get; private set; }
            public int StackFreeMemory { get; private set; }
            public long CPU_F { get; private set; }
            public int GCC_verison { get; private set; }
            public int ARD_version { get; private set; }
            public int DTP_version { get; private set; }
            public bool IsConnectSDModule { get; private set; }
            public bool IsConnectTimeModule { get; private set; }
            public int FlashMemorySize { get; private set; }
            public int SRAMMemorySize { get; private set; }

            public PacketAnswerTotalInfo(PacketAnswer answer)
            {
                if (!Init(CommandType.GetInfo, answer)) throw WrongTypeException;

                Board = (Board)(answer.Data[0]);
                BoardArchitecture = (BoardArchitecture)(answer.Data[1]);
                StackFreeMemory = BitConverter.ToInt32(answer.Data, 2);
                CPU_F = BitConverter.ToInt64(answer.Data, 6);
                GCC_verison = BitConverter.ToInt32(answer.Data, 14);
                ARD_version = BitConverter.ToInt32(answer.Data, 18);
                DTP_version = BitConverter.ToInt32(answer.Data, 22);
                IsConnectSDModule = answer.Data[26] == 1;
                IsConnectTimeModule = answer.Data[27] == 1;
                FlashMemorySize = BitConverter.ToInt32(answer.Data, 28);
                SRAMMemorySize = BitConverter.ToInt32(answer.Data, 32);
            }

            public override string ToString()
            {
                string res = "";
                res += string.Format("Board: {0}\n", Board);
                res += string.Format("BoardArchitecture: {0}\n", BoardArchitecture);
                res += string.Format("StackFreeMemory: {0}\n", StackFreeMemory);
                res += string.Format("FlashMemorySize: {0}\n", FlashMemorySize);
                res += string.Format("SRAMMemorySize: {0}\n", SRAMMemorySize);
                res += string.Format("CPU_F: {0}\n", CPU_F);
                res += string.Format("GCC_verison: {0}\n", GCC_verison);
                res += string.Format("ARD_version: {0}\n", ARD_version);
                res += string.Format("DTP_version: {0}\n", DTP_version);
                res += string.Format("isConnectSDModule: {0}\n", IsConnectSDModule);
                res += string.Format("isConnectTimeModule: {0}\n", IsConnectTimeModule);
                return res;
            }
        }

        public class PacketAnswerCardInfo : PacketAnswerSpecialData
        {
            public int DataStartBlock { get; private set; }
            public int RootDirStart { get; private set; }
            public int BlocksPerFat { get; private set; }
            public int FatCount { get; private set; }
            public int FatStartBlock { get; private set; }
            public int FreeSpace { get; private set; }
            public int FreeClusters { get; private set; }
            public int ClusterCount { get; private set; }
            public byte BlocksPerCluster { get; private set; }
            public SdCardFatType FatType { get; private set; }
            public bool EraseSingleBlock { get; private set; }
            public byte FlashEraseSize { get; private set; }
            public int CardSize { get; private set; }
            public CardType Type { get; private set; }
            public byte ManufacturingDateMonth { get; private set; }
            public short ManufacturingDateYear { get; private set; }
            public int SerialNumber { get; private set; }
            public byte MinorVersion { get; private set; }
            public byte MajorVersion { get; private set; }
            public byte[] ProductVersion { get; private set; }
            public string OEMID { get; private set; }
            public byte ManufacturerID { get; private set; }

            public PacketAnswerCardInfo(PacketAnswer answer)
            {
                if (!Init(CommandType.GetSDInfo, answer)) throw WrongTypeException;
                ManufacturerID = answer.Data[0];
                OEMID = new string(new char[] { (char)answer.Data[1], (char)answer.Data[2] });
                ProductVersion = new byte[5];
                ProductVersion[0] = answer.Data[3];
                ProductVersion[1] = answer.Data[4];
                ProductVersion[2] = answer.Data[5];
                ProductVersion[3] = answer.Data[6];
                ProductVersion[4] = answer.Data[7];
                MajorVersion = answer.Data[8];
                MinorVersion = answer.Data[9];
                SerialNumber = BitConverter.ToInt32(answer.Data, 10);
                ManufacturingDateMonth = answer.Data[14];
                ManufacturingDateYear = (short)(BitConverter.ToInt16(answer.Data, 15) + 2000);
                CardSize = BitConverter.ToInt32(answer.Data, 17);
                FlashEraseSize = answer.Data[21];
                EraseSingleBlock = answer.Data[22] == 1;
                FatType = (SdCardFatType)answer.Data[23];
                BlocksPerCluster = answer.Data[24];
                ClusterCount = BitConverter.ToInt32(answer.Data, 25);
                FreeClusters = BitConverter.ToInt32(answer.Data, 29);
                FreeSpace = BitConverter.ToInt32(answer.Data, 33);
                FatStartBlock = BitConverter.ToInt32(answer.Data, 37);
                FatCount = answer.Data[41];
                BlocksPerFat = BitConverter.ToInt32(answer.Data, 42);
                RootDirStart = BitConverter.ToInt32(answer.Data, 46);
                DataStartBlock = BitConverter.ToInt32(answer.Data, 50);
                Type = (CardType)answer.Data[54];
            }

            public override string ToString()
            {
                string res = "";

                res += string.Format("SD Type: {0}.\n", Type);
                res += string.Format("Manufacturer ID: 0x{0}.\n", ManufacturerID.ToString("X"));
                res += string.Format("OEM ID: {0}.\n", OEMID);
                res += string.Format("Product: {0}.\n", string.Join("", ProductVersion.Select(p => (char)p)));
                res += string.Format("Version: {0}.{1}.\n", MajorVersion, MinorVersion);
                res += string.Format("Serial number: 0x{0}.\n", SerialNumber.ToString("X"));
                res += string.Format("Manufacturing date: {0}/{1}.\n", ManufacturingDateMonth, ManufacturingDateYear);
                res += string.Format("CardSize: {0} MB.\n", CardSize);
                res += string.Format("FlashEraseSize: {0}.\n", FlashEraseSize);
                res += string.Format("EraseSingleBlock: {0}.\n", EraseSingleBlock);
                res += string.Format("Volume is FAT{0}.\n", (int)FatType);
                res += string.Format("BlocksPerCluster: {0}.\n", BlocksPerCluster);
                res += string.Format("ClusterCount: {0}.\n", ClusterCount);
                res += string.Format("FreeClusters: {0}.\n", FreeClusters);
                res += string.Format("FreeSpace: {0} MB.\n", FreeSpace);
                res += string.Format("FatStartBlock: {0}.\n", FatStartBlock);
                res += string.Format("FatCount: {0}.\n", FatCount);
                res += string.Format("BlocksPerFat: {0}.\n", BlocksPerFat);
                res += string.Format("RootDirStart: {0}.\n", RootDirStart);
                res += string.Format("DataStartBlock: {0}.\n", DataStartBlock);

                return res;
            }
        }

        public class PacketAnswerFileInfo : PacketAnswerSpecialData
        {
            public int FileSize { get; private set; }
            public DateTime CreationTime { get; private set; }
            public bool IsHidden { get; private set; }
            public bool IsLFN { get; private set; }
            public bool IsReadOnly { get; private set; }
            public bool IsSystem { get; private set; }
            public string Name { get; private set; }

            public PacketAnswerFileInfo(PacketAnswer answer, string name)
            {
                if (!Init(CommandType.File_GetFileInfo, answer)) throw WrongTypeException;

                FileSize = BitConverter.ToInt32(answer.Data, 0);

                CreationTime = new DateTime(
                    HelpMethods.GetNumber(answer.Data[9], answer.Data[10]),
                    answer.Data[8],
                    answer.Data[7],
                    answer.Data[4],
                    answer.Data[5],
                    answer.Data[6]
                 );

                IsHidden = answer.Data[11] == 1;
                IsLFN = answer.Data[12] == 1;
                IsReadOnly = answer.Data[13] == 1;
                IsSystem = answer.Data[14] == 1;

                Name = name;
            }

            public override string ToString()
            {
                string res = "";
                res += string.Format("File: {0}\n", Name);
                res += string.Format("Size: {0}\n", FileSize);
                res += string.Format("CreationTime: {0}\n", CreationTime.ToString());
                res += string.Format("IsHidden: {0}\n", IsHidden);
                res += string.Format("IsLFN: {0}\n", IsLFN);
                res += string.Format("IsReadOnly: {0}\n", IsReadOnly);
                res += string.Format("IsSystem: {0}\n", IsSystem);

                return res;
            }
        }

        public enum FileExistsResult
        {
            Fail,
            Exists,
            NotExists
        }

        public enum FileDirHandleResult
        {
            OK,
            Fail,
            FileOrDirJustExist,
            FileOrDirNotExists
        }

        public enum WriteReadFileHandleResult
        {
            OK,
            Fail,
            FileNotExists,
            CantOpenFile,
            FileNotOpened,
            CantReadData
        }

        public enum HashAlgorithm
        {
            CRC16,
            CRC32,
            CRC64,
            CRC_CCITT
        }

        public class DataRequestResult
        {
            public WriteReadFileHandleResult Status { get; internal set; } = WriteReadFileHandleResult.Fail;
            public byte[] Result { get; internal set; } = new byte[0];
            public int ErrorByteIndex { get; internal set; }
        }

        public class FileLengthRequestResult
        {
            public FileDirHandleResult Status { get; internal set; } = FileDirHandleResult.Fail;
            public long Length { get; internal set; } = 0;
        }

        public class GetFilesOrDirsRequestResult
        {
            public FileDirHandleResult Status { get; internal set; } = FileDirHandleResult.Fail;
            public List<string> ResultFiles { get; internal set; } = new List<string>();
            public List<string> ResultDirs { get; internal set; } = new List<string>();
        }

        #endregion

        public bool DeviceTest()
        {
            var result = GetResult(CommandType.Test);
            return !result.IsEmpty;
        }

        public bool DeviceDataTest(byte[] data)
        {
            var result = GetResult(CommandType.DataTest, data);
            return (!result.IsEmpty && result.Status == AnswerStatus.OK && result.Data.ToList().SequenceEqual(data));
        }

        public long Device_SyncTime()
        {
            if (!Device_GetTime(out DateTime deviceTime)) //TODO чтото
                return 0;
            double diff = Math.Abs((DateTime.Now - deviceTime).TotalSeconds);
            if (diff > 20)
            {
                if (!Device_SetTime(DateTime.Now))
                    return 0;
                return (long)diff;
            }
            else return -1;
        }

        public FileDirHandleResult File_Create(string FileName)
        {
            var result = GetResult(CommandType.File_CreateFile, Encoding.Default.GetBytes(FileName));
            if (result.IsEmpty) return FileDirHandleResult.Fail;
            switch (result.Code)
            {
                case (0):
                    return FileDirHandleResult.OK;
                case (1):
                    return FileDirHandleResult.Fail;
                case (2):
                    return FileDirHandleResult.FileOrDirJustExist;
                default:
                    return FileDirHandleResult.Fail;
            }
        }

        public FileExistsResult File_Exists(string FileName)
        {
            var result = GetResult(CommandType.File_Exists, Encoding.Default.GetBytes(FileName));
            if (result.IsEmpty) return FileExistsResult.Fail;
            if (result.Code == 1) return FileExistsResult.NotExists;
            else return FileExistsResult.Exists;
           
        }

        public FileDirHandleResult File_Delete(string FileName)
        {
            var result = GetResult(CommandType.File_DeleteFile, Encoding.Default.GetBytes(FileName));
            if (result.IsEmpty) return FileDirHandleResult.Fail;
            switch (result.Code)
            {
                case (0):
                    return FileDirHandleResult.OK;
                case (1):
                    return FileDirHandleResult.Fail;
                case (2):
                    return FileDirHandleResult.FileOrDirNotExists;
                default:
                    return FileDirHandleResult.Fail;
            }
        }

        public FileDirHandleResult Dir_Create(string DirectoryName, bool CreateNecessary)
        {
            var data = new List<byte>();
            data.AddRange(Encoding.Default.GetBytes(DirectoryName));
            data.Add(CreateNecessary ? (byte)1 : (byte)0);
            var result = GetResult(CommandType.Folder_Create, data.ToArray());
            if (result.IsEmpty) return FileDirHandleResult.Fail;
            switch (result.Code)
            {
                case (0):
                    return FileDirHandleResult.OK;
                case (1):
                    return FileDirHandleResult.Fail;
                case (2):
                    return FileDirHandleResult.FileOrDirJustExist;
                case (3):
                    return FileDirHandleResult.FileOrDirJustExist;
                default:
                    return FileDirHandleResult.Fail;
            }
        }

        public FileDirHandleResult Dir_Delete(string DirectoryName, bool DeleteSubItems)
        {
            var data = new List<byte>();
            data.AddRange(Encoding.Default.GetBytes(DirectoryName));
            data.Add(DeleteSubItems ? (byte)1 : (byte)0);
            var result = GetResult(CommandType.Folder_Delete, data.ToArray());
            if (result.IsEmpty) return FileDirHandleResult.Fail;
            switch (result.Code)
            {
                case (0):
                    return FileDirHandleResult.OK;
                case (1):
                    return FileDirHandleResult.Fail;
                case (2):
                    return FileDirHandleResult.FileOrDirNotExists;
                case (3):
                    return FileDirHandleResult.FileOrDirNotExists;
                default:
                    return FileDirHandleResult.Fail;
            }
        }

        public WriteReadFileHandleResult File_Open(string FileName, bool ClearFile)
        {
            var e = Encoding.Default.GetBytes(FileName).ToList();
            e.Add(ClearFile ? (byte)1 : (byte)0);
            var result = GetResult(CommandType.File_Open, e.ToArray());
            if (result.IsEmpty) return WriteReadFileHandleResult.Fail;
            switch(result.Code)
            {
                case (0):
                    return WriteReadFileHandleResult.OK;
                case (1):
                    return WriteReadFileHandleResult.FileNotExists;
                case (2):
                    return WriteReadFileHandleResult.CantOpenFile;
                default:
                    return WriteReadFileHandleResult.Fail;
            }
        }

        public DataRequestResult File_GetCrC16(HashAlgorithm Algorithm)
        {
            var result_ = new DataRequestResult();
            var result = GetResult(CommandType.FILE_GetHashSumOfFile);
            if (result.IsEmpty)
            {
                result_.Status = WriteReadFileHandleResult.Fail;
            }
            if (result.Data[0] != 0)
            {
                switch (result.Data[0])
                {
                    case (1):
                        result_.Status = WriteReadFileHandleResult.CantReadData;
                        result_.ErrorByteIndex = BitConverter.ToInt32(result.Data, 1);
                        return result_;
                    default:
                        result_.Status =  WriteReadFileHandleResult.Fail;
                        return result_;
                }
            }
            result_.Result = new byte[result.Data.Length-1];
            Buffer.BlockCopy(result.Data, 1, result_.Result, 0, result.Data.Length - 1);
            result_.Status = WriteReadFileHandleResult.OK;
            return result_;
        }

        public PacketAnswerCardInfo Device_GetCardInfo()
        {
            return new PacketAnswerCardInfo(GetResult(CommandType.GetSDInfo));
        }

        public PacketAnswerFileInfo File_GetInfo(string Name)
        {
            return new PacketAnswerFileInfo(GetResult(CommandType.File_GetFileInfo, Encoding.Default.GetBytes(Name)), Name);
        }

        public PacketAnswerTotalInfo Device_GetGlobalInfo()
        {
            return new PacketAnswerTotalInfo(GetResult(CommandType.GetInfo));
        }

        public DataRequestResult File_Read(int offset, int length)
        {
            var result_ = new DataRequestResult();
            byte[] data_ = new byte[8];
            Buffer.BlockCopy(BitConverter.GetBytes(offset), 0, data_, 0, 4);
            Buffer.BlockCopy(BitConverter.GetBytes(length), 0, data_, 4, 4);
            var result = GetResult(CommandType.File_GetFileData, data_);
            if (result.IsEmpty)
            {
                result_.Status = WriteReadFileHandleResult.Fail;
                return result_;
            }
            if (result.Data.Length == 1 && result.Status == AnswerStatus.Error)
                switch (result.Data[0])
                {
                    case (1):
                        result_.Status = WriteReadFileHandleResult.FileNotOpened;
                        return result_;
                    case (2):
                        result_.Status = WriteReadFileHandleResult.CantReadData;
                        return result_;
                    default:
                        result_.Status = WriteReadFileHandleResult.Fail;
                        return result_;
                }
            result_.Result = result.Data;
            result_.Status = WriteReadFileHandleResult.OK;
            return result_;
        }

        public GetFilesOrDirsRequestResult Dir_GetFiles(string DirectoryName)
        {
            var files = new List<string>();
            var result = GetResult(CommandType.File_GetFileTree, Encoding.Default.GetBytes(DirectoryName));
            if (result.IsEmpty || result.Status == AnswerStatus.Error) return new GetFilesOrDirsRequestResult();
            string s = Encoding.Default.GetString(result.Data);
            files = s.Split((char)1).ToList().FindAll(p => !p.StartsWith("/") && p.Trim() != "").OrderBy(p => p).ToList();
            return new GetFilesOrDirsRequestResult()
            {
                Status = FileDirHandleResult.OK,
                ResultFiles = files
            };
        }
        
        public GetFilesOrDirsRequestResult Dir_GetSubDirs(string DirectoryName)
        {
           var dirs = new List<string>();
            var result = GetResult(CommandType.File_GetFileTree, Encoding.Default.GetBytes(DirectoryName));
            if (result.IsEmpty || result.Status == AnswerStatus.Error) return new GetFilesOrDirsRequestResult();
            string s = Encoding.Default.GetString(result.Data);
            var a = s.Split((char)1);
            dirs = a.ToList().FindAll(p => p.StartsWith("/") && p.Trim() != "").Select(p => p.Replace("/", "")).ToList().OrderBy(p => p).ToList();
            return new GetFilesOrDirsRequestResult()
            {
                Status = FileDirHandleResult.OK,
                ResultDirs = dirs
            };
        }

        public GetFilesOrDirsRequestResult Dir_GetFilesAndSubDirs(string DirectoryName)
        {
            var dirs = new List<string>();
            var files = new List<string>();
            var result = GetResult(CommandType.File_GetFileTree, Encoding.Default.GetBytes(DirectoryName));
            if (result.IsEmpty || result.Status == AnswerStatus.Error) return new GetFilesOrDirsRequestResult();
            string s = Encoding.Default.GetString(result.Data);
            var a = s.Split((char)1);
            dirs = a.ToList().FindAll(p => p.StartsWith("/") && p.Trim() != "").Select(p => p.Replace("/", "")).ToList().OrderBy(p => p).ToList();
            files = a.ToList().FindAll(p => !p.StartsWith("/") && p.Trim() != "").OrderBy(p => p).ToList();
            return new GetFilesOrDirsRequestResult()
            {
                Status = FileDirHandleResult.OK,
                ResultFiles = files,
                ResultDirs = dirs
            };
        }

        public FileLengthRequestResult File_GetLength()
        {
            var Length = 0;
            var result = GetResult(CommandType.File_GetFileLength);
            if (result.IsEmpty) return new FileLengthRequestResult();
            if (result.Status == AnswerStatus.Error) return new FileLengthRequestResult();
            Length = BitConverter.ToInt32(result.Data, 0);
            return new FileLengthRequestResult()
            {
                Length = Length,
                Status = FileDirHandleResult.OK
            };
        }

        public bool File_Close()
        {
            var result = GetResult(CommandType.File_Close);
            return !result.IsEmpty;
        }
        
        public bool File_Rewrite(byte[] bytes)
        {
            var result = GetResult(CommandType.File_WriteDataToFile, bytes);
            if (result.IsEmpty) return false;
            return result.Code == 0;
        }

        public bool File_Append(byte[] bytes)
        {
            var result = GetResult(CommandType.File_AppendDataToFile, bytes);
            if (result.IsEmpty) return false;
            return result.Code == 0;
        }

        public bool Device_SetTime(DateTime dt)
        {
            DateTime now = dt;
            byte[] data = new byte[7] {
                (byte)now.Hour,
                (byte)now.Minute,
                (byte)now.Second,
                (byte)now.Day,
                (byte)now.Month,
                HelpMethods.SplitNumber(now.Year).Item1, HelpMethods.SplitNumber(now.Year).Item2
            };
            var result = GetResult(CommandType.SetTime, data);
            return !result.IsEmpty;
        }

        public bool Device_GetTime(out DateTime result_)
        {
            result_ = new DateTime();
            var result = GetResult(CommandType.GetDateTime);
            if (result.IsEmpty) return false;
            result_ = new DateTime(
                    HelpMethods.GetNumber(result.Data[5], result.Data[6]),
                    result.Data[4],
                    result.Data[3],
                    result.Data[0],
                    result.Data[1],
                    result.Data[2]
                 );
            return true;
        }
    }
}
