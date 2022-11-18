using System;
using SDG.Framework.IO.FormattedFiles;
using UnityEngine;

namespace SDG.Unturned;

public struct RegionCoord : IFormattedFileReadable, IFormattedFileWritable, IEquatable<RegionCoord>
{
    public static RegionCoord ZERO = new RegionCoord(0, 0);

    public byte x;

    public byte y;

    public void read(IFormattedFileReader reader)
    {
        reader = reader.readObject();
        x = reader.readValue<byte>("X");
        y = reader.readValue<byte>("Y");
    }

    public void write(IFormattedFileWriter writer)
    {
        writer.beginObject();
        writer.writeValue("X", x);
        writer.writeValue("Y", y);
        writer.endObject();
    }

    public static bool operator ==(RegionCoord a, RegionCoord b)
    {
        if (a.x == b.x)
        {
            return a.y == b.y;
        }
        return false;
    }

    public static bool operator !=(RegionCoord a, RegionCoord b)
    {
        return !(a == b);
    }

    public override bool Equals(object obj)
    {
        if (obj == null)
        {
            return false;
        }
        RegionCoord regionCoord = (RegionCoord)obj;
        if (x == regionCoord.x)
        {
            return y == regionCoord.y;
        }
        return false;
    }

    public override int GetHashCode()
    {
        return x ^ y;
    }

    public override string ToString()
    {
        return "(" + x + ", " + y + ")";
    }

    public bool Equals(RegionCoord other)
    {
        if (x == other.x)
        {
            return y == other.y;
        }
        return false;
    }

    public void ClampIntoBounds()
    {
        x = (byte)Mathf.Max(x, 0);
        x = (byte)Mathf.Min(x, 63);
        y = (byte)Mathf.Max(y, 0);
        y = (byte)Mathf.Min(y, 63);
    }

    public RegionCoord(byte new_x, byte new_y)
    {
        x = new_x;
        y = new_y;
    }

    public RegionCoord(Vector3 position)
    {
        Regions.tryGetCoordinate(position, out x, out y);
    }
}
