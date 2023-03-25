using System.Text;

namespace SDG.NetPak;

public static class NetPakConst
{
    public const float INV_SQRT_OF_TWO = 0.70710677f;

    public const float SQRT_OF_TWO = 1.4142135f;

    public const int MAX_STRING_BYTE_COUNT_BITS = 11;

    public const int MAX_STRING_BYTE_COUNT = 2048;

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
