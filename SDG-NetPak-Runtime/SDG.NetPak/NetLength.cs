namespace SDG.NetPak;

public struct NetLength
{
    public uint value;

    public int bitCount;

    public NetLength(uint valueInclusive)
    {
        value = valueInclusive;
        bitCount = NetPakConst.CountBits(value);
    }

    public uint Clamp(uint otherValue)
    {
        if (otherValue <= value)
        {
            return otherValue;
        }
        return value;
    }

    public uint Clamp(int otherValue)
    {
        if (otherValue <= value)
        {
            return (uint)otherValue;
        }
        return value;
    }
}
