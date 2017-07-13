using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace CWA.DTP
{
    public enum CardType
    {
        SD1 = 0x10,
        SD2 = 0x20,
        SDHC = 0x30,
        Unknown = 0x40
    };

    public enum Board
    {
        Yun,
        Uno,
        DueMulanove,
        Nano,
        Mega,
        Adk,
        Leonardo,
        LeonardoEth,
        Micro,
        Esplora,
        Mini,
        Ethernet,
        Fio,
        BT,
        LilypadUSB,
        Lilypad,
        Pro,
        NG,
        RobotControl,
        RobotMotor,
        Gemma,
        CircuitPlay,
        YunMini,
        Industrial101,
        LinioOne,
        UnoWifi,
        Unknown
    };

    public enum BoardArchitecture
    {
        AVR,
        ARM,
        I686,
        I586,
        Unknown
    };

    public enum SdCardFatType
    {
        FAT12 = 12,
        FAT16 = 16,
        FAT32 = 32,
    };

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
                if (!DTP_Test()) throw new Exception("Cant init"); //TODO: Exception
            }
            if (AutoSyncTime) DTP_SyncTime();
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

        public class GetFilesOrDirsRequestResult
        {
            public FileDirHandleResult Status { get; internal set; } = FileDirHandleResult.Fail;
            public List<string> ResultFiles { get; internal set; } = new List<string>();
            public List<string> ResultDirs { get; internal set; } = new List<string>();
        }

        #endregion

        public bool DTP_Test()
        {
            var result = GetResult(CommandType.Test);
            return !result.IsEmpty;
        }

        public bool DTP_DataTest(byte[] data)
        {
            var result = GetResult(CommandType.DataTest, data);
            return (!result.IsEmpty && result.Status == AnswerStatus.OK && result.Data.ToList().SequenceEqual(data));
        }

        public long DTP_SyncTime()
        {
            if (!DTP_GetDateTime(out DateTime deviceTime)) //TODO чтото
                ;
            double diff = Math.Abs((DateTime.Now - deviceTime).TotalSeconds);
            if (diff > 20)
            {
                if (!DTP_SetTime(DateTime.Now)) //TODO чтото
                    ;
                return (long)diff;
            }
            else return 0;
        }

        public FileDirHandleResult DTP_CreateFile(string FileName)
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

        public FileDirHandleResult DTP_DeleteFile(string FileName)
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

        public FileDirHandleResult DTP_CreateDirectory(string DirectoryName, bool CreateNecessary)
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

        public FileDirHandleResult DTP_DeleteDirectory(string DirectoryName, bool DeleteSubItems)
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

        public WriteReadFileHandleResult DTP_OpenFile(string FileName, bool ClearFile)
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

        public DataRequestResult DTP_GetCRC16OfFile(HashAlgorithm Algorithm)
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

        public PacketAnswerCardInfo DTP_GetCardInfo()
        {
            return new PacketAnswerCardInfo(GetResult(CommandType.GetSDInfo));
        }

        public PacketAnswerFileInfo DTP_GetFileInfo(string Name)
        {
            return new PacketAnswerFileInfo(GetResult(CommandType.File_GetFileInfo, Encoding.Default.GetBytes(Name)), Name);
        }

        public PacketAnswerTotalInfo DTP_GetInfo()
        {
            return new PacketAnswerTotalInfo(GetResult(CommandType.GetInfo));
        }

        public DataRequestResult DTP_GetBytesOfFile(int offset, int length)
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
            if (result.Data.Length == 1) switch(result.Data[0])
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

        public GetFilesOrDirsRequestResult DTP_GetFiles(string DirectoryName)
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
        
        public GetFilesOrDirsRequestResult DTP_GetDirectories(string DirectoryName)
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

        public GetFilesOrDirsRequestResult DTP_GetDirectoriesAndFiles(string DirectoryName)
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

        public bool DTP_GetLenOfFile(out int Length)
        {
            Length = 0;
            var result = GetResult(CommandType.File_GetFileLength);
            if (result.IsEmpty) return false;
            if (result.Status == AnswerStatus.Error) return false;
            Length = BitConverter.ToInt32(result.Data, 0);
            return true;
        }

        public bool DTP_CloseFile()
        {
            var result = GetResult(CommandType.File_Close);
            return !result.IsEmpty;
        }
        
        public bool DTP_WriteBytesToFile(byte[] bytes)
        {
            var result = GetResult(CommandType.File_WriteDataToFile, bytes);
            if (result.IsEmpty) return false;
            return result.Code == 0;
        }

        public bool DTP_AppendBytesToFile(byte[] bytes)
        {
            var result = GetResult(CommandType.File_AppendDataToFile, bytes);
            if (result.IsEmpty) return false;
            return result.Code == 0;
        }

        public bool DTP_SetTime(DateTime dt)
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

        public bool DTP_GetDateTime(out DateTime result_)
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
            var res = BaseHandler.DTP_CreateFile(NewName);
            if (res == PacketHandler.FileDirHandleResult.OK) { return true; }
            else if (res == PacketHandler.FileDirHandleResult.Fail)
            {
                RaiseErrorEvent(new ErrorArgs(SendError.CantCreateFile, true));
                return false;
            }
            else
            {
                if (BaseHandler.DTP_DeleteFile(NewName) == PacketHandler.FileDirHandleResult.Fail)
                {
                    RaiseErrorEvent(new ErrorArgs(SendError.CantDeleteFile, true));
                    return false;
                }
                if (BaseHandler.DTP_CreateFile(NewName) == PacketHandler.FileDirHandleResult.Fail)
                {
                    RaiseErrorEvent(new ErrorArgs(SendError.CantCreateFile, true));
                    return false;
                }
            }
            if (BaseHandler.DTP_OpenFile(NewName, true) != PacketHandler.WriteReadFileHandleResult.OK)
            {
                RaiseErrorEvent(new ErrorArgs(SendError.CantOpenFile, true));
                return false;
            };
            return true;
        }
        
        private bool CompareLength()
        {
            var sizeResult = BaseHandler.DTP_GetLenOfFile(out int size);
            if (!sizeResult)
            {
                RaiseErrorEvent(new ErrorArgs(SendError.CantGetFileSize, true));
                return false;
            }
            if (size != _data.Length)
            {
                RaiseErrorEvent(new ErrorArgs(SendError.NotEqualSizes, true));
                return false;
            }
            return true;
        }

        private bool CompareHash()
        {
            var crcres = BaseHandler.DTP_GetCRC16OfFile(PacketHandler.HashAlgorithm.CRC32);
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
            _data = File.ReadAllBytes(pcName);
            var b = _data.Split(PacketLength);
            int totalCount = b.Count();
            int Current = 0;
            foreach (var c in b)
            {
                Current++;
                if (!BaseHandler.DTP_AppendBytesToFile(c.ToArray()))
                {
                    RaiseErrorEvent(new ErrorArgs(SendError.CantSendPacket, true));
                    return false;
                }
                RaiseProcessEvent(new ProcessArgs((long)(DateTime.Now - startTime).TotalSeconds, 0, totalCount - Current, Current, 0, PacketLength));
            }
            if (CheckLen)
            {
                if (!CompareLength()) return false;
                if (!BaseHandler.DTP_CloseFile())
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
    
    internal static class ExtentionMethod
    {
        internal static IEnumerable<IEnumerable<T>> Split<T>(this T[] array, int size)
        {
            for (var i = 0; i < (float)array.Length / size; i++)
            {
                yield return array.Skip(i * size).Take(size);
            }
        }
    }
}
