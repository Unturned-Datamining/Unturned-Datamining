using System;
using SDG.Framework.IO.FormattedFiles;
using UnityEngine;

namespace SDG.Framework.Landscapes;

public struct LandscapeCoord : IFormattedFileReadable, IFormattedFileWritable, IEquatable<LandscapeCoord>
{
    public static LandscapeCoord ZERO = new LandscapeCoord(0, 0);

    public int x;

    public int y;

    public void read(IFormattedFileReader reader)
    {
        reader = reader.readObject();
        x = reader.readValue<int>("X");
        y = reader.readValue<int>("Y");
    }

    public void write(IFormattedFileWriter writer)
    {
        writer.beginObject();
        writer.writeValue("X", x);
        writer.writeValue("Y", y);
        writer.endObject();
    }

    public static bool operator ==(LandscapeCoord a, LandscapeCoord b)
    {
        if (a.x == b.x)
        {
            return a.y == b.y;
        }
        return false;
    }

    public static bool operator !=(LandscapeCoord a, LandscapeCoord b)
    {
        return !(a == b);
    }

    public override bool Equals(object obj)
    {
        if (obj == null)
        {
            return false;
        }
        LandscapeCoord landscapeCoord = (LandscapeCoord)obj;
        if (x.Equals(landscapeCoord.x))
        {
            return y.Equals(landscapeCoord.y);
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

    public bool Equals(LandscapeCoord other)
    {
        if (x == other.x)
        {
            return y == other.y;
        }
        return false;
    }

    public LandscapeCoord(int new_x, int new_y)
    {
        x = new_x;
        y = new_y;
    }

    public LandscapeCoord(Vector3 position)
    {
        x = Mathf.FloorToInt(position.x / Landscape.TILE_SIZE);
        y = Mathf.FloorToInt(position.z / Landscape.TILE_SIZE);
    }
}
