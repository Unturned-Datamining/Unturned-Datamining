using System.IO;
using System.Security.Cryptography;

namespace SDG.Unturned;

public class SHA1Stream : HashStream
{
    public SHA1Stream(Stream underlyingStream)
        : base(underlyingStream, new SHA1Managed())
    {
    }
}
