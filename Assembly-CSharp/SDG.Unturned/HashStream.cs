using System.IO;
using System.Security.Cryptography;

namespace SDG.Unturned;

/// <summary>
/// Run hash algorithm for all data passing through a stream.
/// </summary>
public class HashStream : Stream
{
    private Stream underlyingStream;

    private HashAlgorithm hashAlgo;

    public byte[] Hash
    {
        get
        {
            hashAlgo.TransformFinalBlock(new byte[0], 0, 0);
            return hashAlgo.Hash;
        }
    }

    public override bool CanRead => underlyingStream.CanRead;

    public override bool CanSeek => underlyingStream.CanSeek;

    public override bool CanWrite => underlyingStream.CanWrite;

    public override long Length => underlyingStream.Length;

    public override long Position
    {
        get
        {
            return underlyingStream.Position;
        }
        set
        {
            if (value == 0L)
            {
                hashAlgo.Initialize();
            }
            underlyingStream.Position = value;
        }
    }

    public HashStream(Stream underlyingStream, HashAlgorithm hashAlgo)
    {
        this.underlyingStream = underlyingStream;
        this.hashAlgo = hashAlgo;
    }

    public override void Flush()
    {
        underlyingStream.Flush();
    }

    public override int Read(byte[] buffer, int offset, int count)
    {
        int num = underlyingStream.Read(buffer, offset, count);
        hashAlgo.TransformBlock(buffer, offset, num, buffer, offset);
        return num;
    }

    public override long Seek(long offset, SeekOrigin origin)
    {
        if (origin == SeekOrigin.Begin && offset == 0L)
        {
            hashAlgo.Initialize();
        }
        return underlyingStream.Seek(offset, origin);
    }

    public override void SetLength(long value)
    {
        underlyingStream.SetLength(value);
    }

    public override void Write(byte[] buffer, int offset, int count)
    {
        underlyingStream.Write(buffer, offset, count);
        hashAlgo.TransformBlock(buffer, offset, count, buffer, offset);
    }
}
