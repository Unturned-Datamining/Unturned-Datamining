using System;
using SDG.Framework.IO.FormattedFiles;
using UnityEngine;

namespace SDG.Framework.Foliage;

public struct FoliageCoord : IFormattedFileReadable, IFormattedFileWritable, IEquatable<FoliageCoord>
{
    public static FoliageCoord ZERO = new FoliageCoord(0, 0);

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

    public static bool operator ==(FoliageCoord a, FoliageCoord b)
    {
        if (a.x == b.x)
        {
            return a.y == b.y;
        }
        return false;
    }

    public static bool operator !=(FoliageCoord a, FoliageCoord b)
    {
        return !(a == b);
    }

    public override bool Equals(object obj)
    {
        if (obj == null)
        {
            return false;
        }
        FoliageCoord foliageCoord = (FoliageCoord)obj;
        if (x == foliageCoord.x)
        {
            return y == foliageCoord.y;
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

    public bool Equals(FoliageCoord other)
    {
        if (x == other.x)
        {
            return y == other.y;
        }
        return false;
    }

    public FoliageCoord(int new_x, int new_y)
    {
        x = new_x;
        y = new_y;
    }

    public FoliageCoord(Vector3 position)
    {
        x = Mathf.FloorToInt(position.x / FoliageSystem.TILE_SIZE);
        y = Mathf.FloorToInt(position.z / FoliageSystem.TILE_SIZE);
    }
}
