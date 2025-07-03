using System;
using System.IO;

namespace Framework
{
    public class BytesStream
    {
        static int DefaultSize = 64;
        static int packSize = 64;

        public byte[] Buf;
        public int Pos;
        public int Used;
        public int Capacity;
        private MemoryStream stream;

        public BytesStream()
        {
            RestBytes(DefaultSize);
        }

        public BytesStream(byte[] bytes)
        {
            RestBytes(bytes);
        }

        public BytesStream(int size)
        {
            RestBytes(size);
        }

        public BytesStream(byte[] bytes, int pos, int Used)
        {
            Buf = bytes;
            Pos = 0;
            Used = Buf.Length;
            Capacity = Used;
        }

        public BytesStream(MemoryStream stream)
        {
            this.stream = stream;
        }

        public byte[] GetUsedBytes()
        {
            byte[] bytes = new byte[Used];
            Buffer.BlockCopy(Buf, 0, bytes, 0, Used);
            return bytes;
        }

        public int GetUsed()
        {
            return Used;
        }

        public void Clear()
        {
            Pos = 0;
            Used = 0;
        }

        public void RestBytes(byte[] bytes)
        {
            Buf = bytes;
            Pos = 0;
            Used = Buf.Length;
            Capacity = Used;
        }

        public void RestBytes(int size)
        {
            Buf = new byte[size];
            Pos = 0;
            Used = 0;
            Capacity = DefaultSize;
        }

        public void Reset(byte[] data)
        {
            Buf = data;
            Pos = 0;
            Used = 0;
        }
        public void Reset()
        {
            Pos = 0;
            Used = 0;
        }

        public void SetPos(int pos)
        {
            this.Pos = pos;
        }

        public int GetPos()
        {
            return this.Pos;
        }

        public void MovePos(int pos)
        {
            this.Pos += pos;
        }

        public int bytesAvailable
        {
            get
            {
                return Used - Pos;
            }
        }

        public int read()
        {
            return 0;
        }

        public void Grow(int size)
        {
            int left = Capacity - Pos;
            if (left < size)
            {
                int need = size - left;
                //float p = (float)need / packSize;
                //int pa = Mathf.CeilToInt(p);
                //Capacity += (int)(Math.Ceiling((float)need / packSize)) * packSize;

                Capacity += (need / packSize) * packSize;
                if (need % packSize != 0)
                {
                    Capacity += packSize;
                }
                byte[] newBuf = new byte[Capacity];
                if (Used > 0)
                {
                    Array.Copy(Buf, newBuf, Used);
                }
                Buf = newBuf;
            }
        }

        void Move(int size)
        {
            Pos += size;
            if (Pos > Used)
            {
                Used = Pos;
            }
        }
        public void WriteBool(bool b)
        {
            Grow(1);
            Buf[Pos] = (byte)(b ? 1 : 0);
            Move(1);
        }
        public void WriteByte(byte b1)
        {
            Grow(1);
            Buf[Pos] = b1;
            Move(1);
        }

        public void WriteBytes(byte[] p)
        {
            int len = p.Length;
            WriteUInt16((ushort)len);
            WriteBytes(p, len);
        }

        public void WriteBytes(byte[] p, int len)
        {
            if (len == 0)
                return;

            Grow(len);
            Buffer.BlockCopy(p, 0, Buf, Pos, len);
            Move(len);
        }

        public void WriteInt16(short n)
        {
            Grow(2);
            Buf[Pos + 0] = (byte)n;
            Buf[Pos + 1] = (byte)(n >> 8);
            Move(2);
        }

        public void WriteUInt16(ushort n)
        {
            Grow(2);
            Buf[Pos + 0] = (byte)n;
            Buf[Pos + 1] = (byte)(n >> 8);
            Move(2);
        }

        public void WriteInt32(int n)
        {
            Grow(4);
            Buf[Pos + 0] = (byte)(n & 0xff);
            Buf[Pos + 1] = (byte)((n >> 8) & 0xff);
            Buf[Pos + 2] = (byte)((n >> 16) & 0xff);
            Buf[Pos + 3] = (byte)((n >> 24) & 0xff);
            Move(4);
        }

        public void WriteUInt32(uint n)
        {
            Grow(4);
            Buf[Pos + 0] = (byte)(n & 0xff);
            Buf[Pos + 1] = (byte)((n >> 8) & 0xff);
            Buf[Pos + 2] = (byte)((n >> 16) & 0xff);
            Buf[Pos + 3] = (byte)((n >> 24) & 0xff);
            Move(4);
        }

        public void WriteInt64(long n)
        {
            Grow(8);
            Buf[Pos + 0] = (byte)((n >> 54) & 0xff);
            Buf[Pos + 1] = (byte)((n >> 48) & 0xff);
            Buf[Pos + 2] = (byte)((n >> 40) & 0xff);
            Buf[Pos + 3] = (byte)((n >> 32) & 0xff);
            Buf[Pos + 4] = (byte)((n >> 24) & 0xff);
            Buf[Pos + 5] = (byte)((n >> 16) & 0xff);
            Buf[Pos + 6] = (byte)((n >> 8) & 0xff);
            Buf[Pos + 7] = (byte)(n & 0xff);
            Move(8);
        }

        public void WriteUInt64(UInt64 n)
        {
            WriteBytes(BitConverter.GetBytes(n), 8);
        }

        public void WriteFloat(float n)
        {
            WriteBytes(BitConverter.GetBytes(n), 4);
        }

        public void Write16BitFloat(float n)
        {
            short uintN = (short)(n * 100);
            WriteInt16(uintN);
        }

        public void WriteDouble(double n)
        {
            WriteBytes(BitConverter.GetBytes(n), 8);
        }

        public void WriteString(string str)
        {
            byte[] bytes = System.Text.Encoding.UTF8.GetBytes(str);
            WriteBytes(bytes);
        }

        public bool ReadBoolean()
        {
            Pos++;
            return Convert.ToBoolean(Buf[Pos - 1]);
        }
        public byte ReadByte()
        {
            Pos++;
            return Buf[Pos - 1];
        }
        public byte[] ReadBytes(int count)
        {
            byte[] bytes = new byte[count];
            Buffer.BlockCopy(Buf, Pos, bytes, 0, count);
            Pos += count;
            return bytes;
        }

        public byte[] ReadBytes()
        {
            var len = ReadUInt16();
            return ReadBytes(len);
        }

        public char ReadChar()
        {
            return (char)ReadByte();
        }

        public float ReadFloat()
        {
            byte[] bytes = ReadBytes(4);
            return BitConverter.ToSingle(bytes, 0);
        }

        public float Read16BitFloat()
        {
            float bytes = (float)ReadInt16();

            bytes = bytes / 100.0f;
            return bytes;
        }

        public double ReadDouble()
        {
            byte[] bytes = ReadBytes(8);
            return BitConverter.ToDouble(bytes, 0);
        }

        public short ReadInt16()
        {
            int a = (int)ReadByte();
            int b = (int)ReadByte();
            return (short)(a | (b << 8));
        }
        public ushort ReadUInt16()
        {
            int a = (int)ReadByte();
            int b = (int)ReadByte();
            return (ushort)((b << 8) | a);
        }

        public int ReadInt32()
        {
            int c1 = (int)ReadByte();
            int c2 = (int)ReadByte();
            int c3 = (int)ReadByte();
            int c4 = (int)ReadByte();
            ////Debug.Log("pos:" + Pos + "read int:" + c1 + ", " + c2 + ", " + c3 + ", " + c4);
            return (c4 << 24) | (c3 << 16) | (c2 << 8) | c1;
        }
        public int ReadInt24()
        {
            int c1 = (int)ReadByte();
            int c2 = (int)ReadByte();
            int c3 = (int)ReadByte();
            ////Debug.Log("pos:" + Pos + "read int:" + c1 + ", " + c2 + ", " + c3 + ", " + c4);
            return c1 | (c2 << 8) | (c3 << 16);

        }
        public uint ReadUInt32()
        {
            uint c1 = (uint)ReadByte();
            uint c2 = (uint)ReadByte();
            uint c3 = (uint)ReadByte();
            uint c4 = (uint)ReadByte();
            return (c4 << 24) | (c3 << 16) | (c2 << 8) | c1;
        }
        public long ReadInt64()
        {
            long c1 = (long)ReadByte();
            long c2 = (long)ReadByte();
            long c3 = (long)ReadByte();
            long c4 = (long)ReadByte();
            long c5 = (long)ReadByte();
            long c6 = (long)ReadByte();
            long c7 = (long)ReadByte();
            long c8 = (long)ReadByte();

            return (c8 << 56) | (c7 << 48) | (c6 << 40) | (c5 << 32) | (c4 << 24) | (c3 << 16) | (c2 << 8) | c1;
        }
        public ulong ReadUInt64()
        {
            byte[] bytes = ReadBytes(8);
            ulong c1 = (ulong)bytes[0];
            ulong c2 = (ulong)bytes[1];
            ulong c3 = (ulong)bytes[2];
            ulong c4 = (ulong)bytes[3];
            ulong c5 = (ulong)bytes[4];
            ulong c6 = (ulong)bytes[5];
            ulong c7 = (ulong)bytes[6];
            ulong c8 = (ulong)bytes[7];
            return (c8 << 56) | (c7 << 48) | (c6 << 40) | (c5 << 32) | (c4 << 24) | (c3 << 16) | (c2 << 8) | c1;

        }

        public string ReadString()
        {
            int len = (int)ReadUInt16();
            byte[] bytes = ReadBytes(len);
            return System.Text.Encoding.UTF8.GetString(bytes);
        }

        public string ReadString(int len)
        {
            byte[] bytes = ReadBytes(len);
            return System.Text.Encoding.UTF8.GetString(bytes);
            //return System.Text.Encoding.Default.GetString(bytes);
        }

        public void Append(BytesStream recvBuf)
        {
            Grow(recvBuf.Used);

            int left = Capacity - Used;
            if (left < recvBuf.Used)
            {
                Capacity = Used + recvBuf.Used;
                byte[] newBuf = new byte[Capacity];
                if (Used > 0)
                {
                    Array.Copy(Buf, newBuf, Used);
                }
                Array.Copy(recvBuf.Buf, 0, newBuf, Used, recvBuf.Used);
                Buf = null;
                Buf = newBuf;
            }
            else
            {
                Array.Copy(recvBuf.Buf, 0, Buf, Used, recvBuf.Used);
            }

            Used += recvBuf.Used;
        }

        public bool IsEOF()
        {
            return Pos == Used;
        }
    }
}

