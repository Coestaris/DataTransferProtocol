#define SimpleCRC

using System;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;

namespace CWA.DTP
{
    public class CantInitException : Exception
    {
        //TODO CantInitException
    }

    public class WrongPacketInputException : Exception
    {
        public enum ExceptionType
        {
            WrongDataLenInput,
            WrongDataInput,
            TimeOut,
            WrongSum,
        }

        Exception baseException;

        public ExceptionType Type { get; private set; }

        //TimeOut
        public WrongPacketInputException(int timeOut)
        {
            Type = ExceptionType.TimeOut;
        }

        //WrongdataInput
        public WrongPacketInputException(Exception baseException)
        {
            Type = ExceptionType.WrongDataInput;
        }

        //wrongDataLenInput
        public WrongPacketInputException(int EnteredLen, int BytesToRead)
        {
            Type = ExceptionType.WrongDataLenInput;
        }

        //TODO Конкретную дату под конкретный случай, а лучше конечно же отдельные классы, леньтяй
        //WrongSum
        public WrongPacketInputException(Packet basePacket, Tuple<byte,byte> exceptedCRC, Tuple<byte,byte> packetCRC)
        {
            Type = ExceptionType.WrongSum;
        }
    }

    public interface IPacketReader
    {
        void Reset();
        byte[] Read();
    }

    public interface IPacketWriter
    {
        bool Write(byte[] packet);
    }

    public class SerialPacketWriter : IPacketWriter
    {
        private SerialPort _port;

        public SerialPacketWriter(SerialPort port)
        {
            _port = port;
            if (!port.IsOpen) port.Open();
        }

        public bool Write(byte[] packet)
        {
            _port.Write(packet, 0, packet.Length);
            return true; //Always okay
        }
    }

    public class SerialPacketReader : IPacketReader
    {
        private byte[] _result;
        private bool _getAnsw = false;
        private SerialPort _port;

        public int TimeOutInterval { get; set; } = 5000;
        
        public SerialPacketReader(SerialPort port, int TimeOutInterval)
        {
            this.TimeOutInterval = TimeOutInterval;
            _port = port;
            port.DataReceived += AsyncGetData;
            if (!port.IsOpen) port.Open();
        }

        public void Reset()
        {
            _port.Close();
            _port.Open();
        }

        public SerialPacketReader(SerialPort port)
        {
            _port = port;
            port.DataReceived += AsyncGetData;
            if (!port.IsOpen) port.Open();
        }

        public byte[] Read()
        {
            _getAnsw = false;
            int counter = 0;
            while (counter <= TimeOutInterval)
            {
                counter += 1;
                Thread.Sleep(1);
                if (_getAnsw)
                {
                    _getAnsw = false;
                    return _result;
                }
            }
            throw new WrongPacketInputException(TimeOutInterval);
        }

        private byte[] ReadAsync()
        {
            /* var a = (byte)_port.ReadByte();
             var b = (byte)_port.ReadByte();
             var len = DtpHelper.GetNumber(a, b) - 255;
             if (len < 12) throw new WrongPacketInputException(len, _port.BytesToRead + 2);
             var buffer = new byte[len];
             int total = 0;
             while (total != len - 2)
             {
                 var buffer_ = new byte[len - total - 2];
                 int newLen = _port.Read(buffer_, 0, len - total - 2);
                 Buffer.BlockCopy(buffer_, 0, buffer, 2 + total, newLen);
                 total += newLen;
             }
             return buffer;*/

            var a = (byte)_port.ReadByte();
            var b = (byte)_port.ReadByte();


            var len = HelpMethods.GetNumber(a, b) - 255;
            var buffer = new byte[len];
            buffer[0] = a;
            buffer[1] = b;

            //Console.WriteLine("LEN: {0}. LEN: {1} {2}", len, a, b);
            
            /*byte[] inBuff = new byte[len - 2];
            port.Read(inBuff, 0, len - 2);
            var e = inBuff.ToList();
            e.Insert(0, b);
            e.Insert(0, a);
            */
            for (int i = 0; i <= len - 3; i++)
            {
                buffer[i + 2] = (byte)_port.ReadByte();
            }
            _port.ReadExisting();
            return buffer;
        }

        private void AsyncGetData(object sender, SerialDataReceivedEventArgs e)
        {
            _result = ReadAsync();
            _getAnsw = true;
        }
    }

    public enum CommandType
    {
        RunFile = 0x101, //+
        File_GetFileTree = 0x103, //+
        File_GetFileData = 0x105,
        File_GetFileLength = 0x10e,
        File_WriteDataToFile = 0x106, //+
        File_DeleteFile = 0x107, //+
        File_CreateFile = 0x114, //+
        File_RenameFile = 0x108,
        File_GetFileInfo = 0x109, //+
        File_AppendDataToFile = 0x10a, //+
        Folder_Create = 0x10b, //+
        Folder_Delete = 0x10c, //+
        Folder_Rename = 0x10d,
        Test = 0x110, //+
        DataTest = 0x111, //+
        GetSDInfo = 0x112, //+
        Answer = 0x113, //+
        GetDateTime = 0x115, //+
        SetTime = 0x116, //+
        FILE_GetHashSumOfFile = 0x117, //+
        SET_DIGITAL_PIN = 0x118,
        SET_ANALOG_PIN = 0x119,
        SPEAKER_BEEP = 0x11a,
        GetInfo = 0x11b, //+
        File_Open = 0x11c, //+
        File_Close = 0x11d, //+
    };

    public enum AnswerStatus
    {
        OK = 0x20,
	    Warning = 0x40,
	    Error = 0x60
    };

    public enum AnswerDataType
    {
        CODE = 0x20,
    	DATA = 0x40,
    	NONE = 0x60
    };

    public enum SenderType
    {
        UnNamedByteMask = 0x20,
    	SevenByteName = 0x40
    };

    public class PacketAnswer
    {
        public CommandType Command { get; set; }
        public Sender Sender { get; set; }
        public AnswerStatus Status { get; set; }
        public AnswerDataType DataType { get; set; }
        public byte Code { get; set; }
        public byte[] Data { get; set; }
        private bool isEmpty;

        public PacketAnswer(Packet base_)
        {
            if (base_.IsEmpty) { IsEmpty = true; return; };
            if (base_.Data == null || base_.Data.Length < 4) { IsEmpty = true; return; };
            Command = (CommandType)HelpMethods.GetNumber(base_.Data[0], base_.Data[1]);
            Status = (AnswerStatus)base_.Data[2];
            DataType = ((AnswerDataType)base_.Data[3]);
            Sender = base_.Sender;
            if (DataType == AnswerDataType.CODE)
                Code = base_.Data[4];
            else if (DataType == AnswerDataType.DATA)
                Data = base_.Data.ToList().GetRange(4, base_.Data.Length - 4).ToArray();
        }

        public bool IsEmpty
        {
            get { return isEmpty; }
            private set { isEmpty = value; }
        }

        public override string ToString()
        {
            switch (DataType)
            {
                case AnswerDataType.CODE:
                    return string.Format("Answer:\n   -Answer to Command: {0};\n   -Sender of Answer: {1};\n   -Answer Status: {2};\n   -ErrorCode Type: {3};\n   -Data (Byte): {4};",
                           Command.ToString(), Sender.ToString(), Status.ToString(), DataType.ToString(), Code);
                case AnswerDataType.DATA:
                    return string.Format("Answer:\n   -Answer to Command: {0}\n   -Sender of Answer: {1}\n   -Answer Status: {2}\n   -ErrorCode Type: {3}\n   -Data (Byte array. {4} Byte(s)): {5}",
                           Command.ToString(), Sender.ToString(), Status.ToString(), DataType.ToString(), Data.Length, string.Join(",", Data) + " or \"" + string.Join("", Data.Select(p => (char)p)) + "\"");
                case AnswerDataType.NONE:
                    return string.Format("Answer:\n   -Answer to Command: {0}\n   -Sender of Answer: {1}\n\t-Answer Status: {2}\n   -ErrorCode Type: {3}\n   -Sender dont send any data.",
                           Command.ToString(), Sender.ToString(), Status.ToString(), DataType.ToString());
                default:
                    return null;
            }
        }
    }

    public class PacketListener
    {
        public IPacketReader PacketReader { get; set; }

        public IPacketWriter PacketWriter { get; set; }

        public PacketListener(IPacketReader reader, IPacketWriter writer)
        {
            PacketReader = reader;
            PacketWriter = writer;
        }
        
        public PacketAnswer SendAndListenPacket(Packet packet)
        {
            if (packet == null || packet.IsEmpty || packet.TotalData == null) throw new ArgumentException(nameof(packet));
            PacketWriter.Write(packet.TotalData);
            //Console.WriteLine("Sended packet");
            var result = PacketReader.Read();
            //Console.WriteLine("Readed packet");
            return new PacketAnswer(Packet.ParsePacket(result, result.Length));
        }
    }
    
    public static class HelpMethods
    {
        public static Tuple<byte, byte> SplitNumber(int num)
        {
            byte low = (byte)(num & 0xFF);
            byte high = (byte)((num >> 8) & 0xFF);
            return new Tuple<byte, byte>(low, high);
        }

        public static short GetNumber(byte low, byte high)
        {
            return (short)(low | (high << 8));
        }

#if SimpleCRC
        public static unsafe int ComputeChecksum(byte* data_p, int length) 
        {
            byte x;
            ushort crc = 0xFFFF;
            while (length-- != 0)
            {
                x = (byte)(crc >> 8 ^ *data_p++);
                x ^= (byte)(x >> 4);
                crc = (ushort)((crc << 8) ^ ((ushort)(x << 12)) ^ ((ushort)(x << 5)) ^ ((ushort)x));
            }
            return crc;
        }
#endif 

    }

    public class CrCHandler
    {
#if !SimpleCRC
        const ushort poly = 4129;
        ushort[] table = new ushort[256];
        ushort initialValue = 0;
#endif
        public unsafe ushort ComputeChecksum(byte[] bytes)
        {
#if SimpleCRC
            fixed (byte* bytes_ = bytes)
            {
                return (ushort)HelpMethods.ComputeChecksum(bytes_, bytes.Length);
            }
#else
            ushort crc = initialValue;
            for (int i = 0; i < bytes.Length; ++i)
            {
                crc = (ushort)((crc << 8) ^ table[((crc >> 8) ^ (0xff & bytes[i]))]);
            }
            return crc;
#endif
        }

        public byte[] ComputeChecksumBytes(byte[] bytes)
        {
            ushort crc = ComputeChecksum(bytes);
            return BitConverter.GetBytes(crc);
        }
#if !SimpleCRC
        public DtpCrcHandler()
        {
            initialValue = 0xffff;
            ushort temp, a;
            for (int i = 0; i < table.Length; ++i)
            {
                temp = 0;
                a = (ushort)(i << 8);
                for (int j = 0; j < 8; ++j)
                {
                    if (((temp ^ a) & 0x8000) != 0) temp = (ushort)((temp << 1) ^ poly);
                    else temp <<= 1;
                    a <<= 1;
                }
                table[i] = temp;
            }
        }
#endif
    }

    public class Sender
    {
        public byte[] Mask { get; set; }
        public SenderType Type { get; set; }
        private string _name;

        public string Name
        {
            get { return _name; }
            set
            {
                if (value.Length != 7) throw new ArgumentException();
                if (Mask == null) Mask = new byte[7];
                for (int i = 0; i <= 6; i++) Mask[i] = (byte)value[i];
                _name = value;
            }
        }

        public Sender(SenderType type)
        {
            Type = type;
            if (type == SenderType.UnNamedByteMask) Mask = RandomGenerateSenderMask();
        }

        public Sender(SenderType type, string Name)
        {
            Type = type;
            if (type == SenderType.UnNamedByteMask) Mask = RandomGenerateSenderMask();
            else this.Name = Name;
        }

        public static byte[] RandomGenerateSenderMask()
        {
            Random r = new Random();
            var result = new byte[7];
            r.NextBytes(result);
            return result;
        }

        public override string ToString()
        {
            if (Type == SenderType.UnNamedByteMask)
                return string.Format("Sender[{0}]", string.Join(",", Mask));
            else return string.Format("Sender[{0}]", Name);
        }

        public static bool operator !=(Sender first, Sender second)
        {
            if (first == second) return false;
            else return true;
        }

        public static bool operator ==(Sender first, Sender second)
        {
            if (first.Type == second.Type &&
                Enumerable.SequenceEqual(first.Mask, second.Mask)) return true;
            else return false;
        }

        public override bool Equals(object obj)
        {
            if (typeof(Sender) != obj.GetType()) return false;
            return this == (Sender)obj;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }

    public struct Packet
    {
        public short Size { get; set; }
        public CommandType Command { get; set; }
        public Sender Sender { get; set; }
        public byte[] Data { get; set; }
        public byte[] Crc { get; set; }

        public bool IsEmpty { get; set; }

        public byte[] TotalData { get; set; }

        public static Packet NULL
        {
            get { return new Packet() { IsEmpty = true }; }
        }

        public Packet(byte[] data, CommandType command, Sender sender)
        {
            Data = data;
            Command = command;
            Sender = sender;
            CrCHandler crc = new CrCHandler();
            Crc = crc.ComputeChecksumBytes(data);
            Size = (short)(data.Length + 14);
            TotalData = new byte[0];
            IsEmpty = false;
        }

        public override string ToString()
        {
            if (TotalData != null) return string.Format("Packet[{0}. Data: {1}]", Sender.ToString(), string.Join("", Data.Select(p => (char)p)));
            else return string.Format("Packet[{0}. Data: {1}]", Sender.ToString(), string.Join("", TotalData.Select(p => (char)p)));
        }

        public static bool operator !=(Packet first, Packet second)
        {
            if (first == second) return false;
            else return true;
        }

        public static bool operator ==(Packet first, Packet second)
        {
            if (first.Command == second.Command &&
                Enumerable.SequenceEqual(first.Data, second.Data) &&
                first.Sender == second.Sender) return true;
            else return false;
        }

        public override bool Equals(object obj)
        {
            if (typeof(Packet) != obj.GetType()) return false;
            return this == (Packet)obj;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public string ToString(bool v)
        {
            if (TotalData != null) return string.Format("Packet[{0}. Data: {1}]", Sender.ToString(), string.Join(",", TotalData.Select(p => p.ToString())));
            else return string.Format("Packet[{0}. Data: {1}]", Sender.ToString(), string.Join(",", Data.Select(p => p.ToString())));
        }

        public static Packet ParsePacket(byte[] data, int len)
        {
            try
            {
                var a = new Packet()
                {
                    TotalData = data
                };
                var command = (CommandType)HelpMethods.GetNumber(data[2], data[3]);
                var sender = new Sender(SenderType.UnNamedByteMask);
                var sendertype = (SenderType)data[4];
                sender.Type = sendertype;
                Buffer.BlockCopy(data, 5, sender.Mask, 0, 7);
                if (sendertype == SenderType.SevenByteName) sender.Name = Encoding.Default.GetString(sender.Mask);
                a.Sender = sender;
                a.Size = (short)len;
                a.Command = command;
                a.Data = new byte[len - 14];
                Buffer.BlockCopy(data, 12, a.Data, 0, len - 14);
                CrCHandler crc = new CrCHandler();
                a.Crc = new byte[2] { data[data.Length - 2], data[data.Length - 1] };
                var newCrc = crc.ComputeChecksumBytes(a.Data);
                if (a.Crc[0] != newCrc[0] || a.Crc[1] != newCrc[1]) throw new WrongPacketInputException(a, new Tuple<byte, byte>(newCrc[0], newCrc[1]), new Tuple<byte, byte>(a.Crc[0], a.Crc[1]));
                return a;
            }
            catch(Exception e)
            {
                throw new WrongPacketInputException(e);
            }
        }

        static public Packet GetPacket(CommandType type, byte[] data, Sender sender)
        {
            var result = new Packet(data, type, sender);
            var resdata = new byte[result.Size];
            var sizeBytes = HelpMethods.SplitNumber(result.Size + 255);
            resdata[0] = sizeBytes.Item1;
            resdata[1] = sizeBytes.Item2;
            var commandBytes = HelpMethods.SplitNumber((int)type);
            resdata[2] = commandBytes.Item1;
            resdata[3] = commandBytes.Item2;
            resdata[4] = (byte)sender.Type;
            Buffer.BlockCopy(sender.Mask, 0, resdata, 5, 7);
            Buffer.BlockCopy(data, 0, resdata, 12, data.Length);
            Buffer.BlockCopy(result.Crc, 0, resdata, 12 + data.Length, 2);
            result.TotalData = resdata;
            return result;
        }
    }
}