using System.Text;

namespace SDG.NetPak;

public static class NetPakConst
{
    /// <summary>
    /// Uses "smallest three" optimization described by Glenn Fiedler: https://gafferongames.com/post/snapshot_compression/
    /// Quoting here in case the link moves: "If v is the absolute value of the largest quaternion component,
    /// the next largest possible component value occurs when two components have the same absolute value and the
    /// other two components are zero. The length of that quaternion (v,v,0,0) is 1, therefore v^2 + v^2 = 1,
    /// 2v^2 = 1, v = 1/sqrt(2). This means you can encode the smallest three components in [-0.707107,+0.707107]
    /// instead of [-1,+1] giving you more precision with the same number of bits."
    /// </summary>
    public const float INV_SQRT_OF_TWO = 0.70710677f;

    public const float SQRT_OF_TWO = 1.4142135f;

    /// <summary>
    /// Maximum number of bits to read/write for string byte count without overflowing the string buffer.
    /// </summary>
    public const int MAX_STRING_BYTE_COUNT_BITS = 11;

    /// <summary>
    /// Maximum number of UTF8 bytes for string.
    /// Before the "null or empty" flag was added the length had to be able to represent 0, but now the receiver
    /// can infer that the byte count is at least 1.
    /// </summary>
    public const int MAX_STRING_BYTE_COUNT = 2048;

    /// <summary>
    /// encoderShouldEmitUTF8Identifier enables byte order mark (BOM) which is unnecessary for UTF8.
    /// throwOnInvalidBytes allows reader to discard bad string packets.
    /// </summary>
    internal static UTF8Encoding stringEncoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false, throwOnInvalidBytes: true);

    internal static byte[] STRING_BUFFER = new byte[2048];

    public static int CountBits(uint value)
    {
        int num = 0;
        while (value != 0)
        {
            num++;
            value >>= 1;
        }
        return num;
    }
}
