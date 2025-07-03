//--------------------------------------------------------------------//
//                     EFRAMEWORK LIMITED LICENSE                     //
//  Copyright (C) EFramework Software Co., Ltd. All rights reserved.  //
//                  SEE LICENSE.md FOR MORE DETAILS.                  //
//--------------------------------------------------------------------//

using System;
using System.Collections.Generic;
using System.IO;

/// <summary>
/// Byte stream cache (thread safe)
/// </summary>
public sealed class StreamBuffer : IDisposable
{
    private byte[] mBuffer;
    private MemoryStream mStream;
    private BinaryReader mReader;
    private BinaryWriter mWriter;

    /// <summary>
    /// Byte capacity
    /// </summary>
    public int Capacity
    {
        get => mBuffer.Length;
    }

    /// <summary>
    /// Byte length
    /// </summary>
    public int Length { get; private set; }

    /// <summary>
    /// Index position
    /// </summary>
    public int Position
    {
        get => (int)Stream.Position;
        set => Stream.Position = value;
    }

    /// <summary>
    /// Memory stream
    /// </summary>
    public MemoryStream Stream
    {
        get
        {
            if (mStream == null) mStream = new MemoryStream(mBuffer, 0, mBuffer.Length, true, true);
            return mStream;
        }
    }

    /// <summary>
    /// Stream reader
    /// </summary>
    public BinaryReader Reader
    {
        get
        {
            if (mReader == null) mReader = new BinaryReader(Stream);
            return mReader;
        }
    }

    /// <summary>
    /// Stream writer
    /// </summary>
    public BinaryWriter Writer
    {
        get
        {
            if (mWriter == null) mWriter = new BinaryWriter(Stream);
            return mWriter;
        }
    }

    /// <summary>
    /// Byte array
    /// </summary>
    public byte[] Buffer
    {
        get => mBuffer;
    }

    public StreamBuffer(byte[] buffer, int offset = 0)
    {
        mBuffer = buffer;
        Length = buffer.Length;
        Position = offset;
    }

    public StreamBuffer(int size)
    {
        if (size < 0) throw new Exception("size must >= 0");
        mBuffer = new byte[size];
    }

    /// <summary>
    /// Write array
    /// </summary>
    /// <param name="offset">Offset</param>
    /// <param name="count">Length</param>
    /// <returns></returns>
    public byte[] ToArray(int offset = 0, int count = 0)
    {
        if (count == 0) count = Length;
        byte[] bytes = new byte[count - offset];
        CopyTo(offset, bytes, 0, bytes.Length);
        return bytes;
    }

    /// <summary>
    /// Copy array
    /// </summary>
    /// <param name="srcOffset">Source offset</param>
    /// <param name="dst">Target array</param>
    /// <param name="dstOffset">Target offset</param>
    /// <param name="count">Copy length</param>
    public void CopyTo(int srcOffset, Array dst, int dstOffset, int count)
    {
        System.Buffer.BlockCopy(mBuffer, srcOffset, dst, dstOffset, count);
    }

    /// <summary>
    /// Complete write
    /// </summary>
    public void Flush()
    {
        Length = Position;
        Stream.Seek(0, SeekOrigin.Begin);
    }

    /// <summary>
    /// Reset data
    /// </summary>
    public void Reset()
    {
        Length = -1;
        Stream.Seek(0, SeekOrigin.Begin);
    }

    /// <summary>
    /// Release memory
    /// </summary>
    public void Dispose()
    {
        Length = -1;
        mBuffer = null;
        try
        {
            if (mReader != null) mReader.Close();
        }
        catch
        {
        }

        try
        {
            if (mWriter != null) mWriter.Close();
        }
        catch
        {
        }

        try
        {
            if (mStream != null) mStream.Close();
        }
        catch
        {
        }

        try
        {
            if (mStream != null) mStream.Dispose();
        }
        catch
        {
        }

        mReader = null;
        mWriter = null;
        mStream = null;
    }

    /// <summary>
    /// Cache pool count
    /// </summary>
    public static int POOL_SIZE = 500;

    /// <summary>
    /// Maximum cache byte stream
    /// </summary>
    public static int BYTE_MAX = 60 * 1024;

    /// <summary>
    /// Cache pool
    /// </summary>
    private static List<StreamBuffer> mBuffers = new List<StreamBuffer>();

    /// <summary>
    /// Cache pool hash
    /// </summary>
    private static Dictionary<int, byte> mBuffersHash = new Dictionary<int, byte>();

    /// <summary>
    /// Get byte stream
    /// </summary>
    /// <param name="expected">Expected length</param>
    /// <returns></returns>
    public static StreamBuffer Get(int expected)
    {
        if (expected < 0) throw new Exception("expected size must >= 0");
        StreamBuffer buffer = null;
        if (expected < BYTE_MAX)
        {
            lock (mBuffers)
            {
                for (int i = mBuffers.Count - 1; i >= 0; i--)
                {
                    var tmp = mBuffers[i];
                    if (tmp.Length >= expected)
                    {
                        buffer = tmp;
                        buffer.Reset();
                        mBuffers.RemoveAt(i);
                        mBuffersHash.Remove(buffer.GetHashCode());
                        break;
                    }
                }
            }
        }

        if (buffer == null) buffer = new StreamBuffer(expected);
        return buffer;
    }

    /// <summary>
    /// Cache byte stream
    /// </summary>
    /// <param name="buffer"></param>
    public static void Put(StreamBuffer buffer)
    {
        if (buffer == null || buffer.Length == 0) return;
        if (buffer.Length > BYTE_MAX) return;
        lock (mBuffers)
        {
            buffer.Reset();
            if (!mBuffersHash.ContainsKey(buffer.GetHashCode()))
            {
                if (mBuffers.Count >= POOL_SIZE)
                {
                    var tmp = mBuffers[0];
                    tmp.Dispose();
                    mBuffers.RemoveAt(0);
                    mBuffersHash.Remove(tmp.GetHashCode());
                }

                mBuffers.Add(buffer);
                mBuffersHash.Add(buffer.GetHashCode(), 0);
            }
        }
    }
}