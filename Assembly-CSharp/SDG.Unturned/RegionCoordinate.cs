namespace SDG.Unturned;

public struct RegionCoordinate
{
    public byte x;

    public byte y;

    public RegionCoordinate(byte x, byte y)
    {
        this.x = x;
        this.y = y;
    }

    public override string ToString()
    {
        return "(" + x + ", " + y + ")";
    }
}
