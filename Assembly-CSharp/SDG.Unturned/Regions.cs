using System.Collections.Generic;
using SDG.NetTransport;
using UnityEngine;

namespace SDG.Unturned;

public class Regions
{
    internal const byte CONST_WORLD_SIZE = 64;

    public static readonly byte WORLD_SIZE = 64;

    internal const byte CONST_REGION_SIZE = 128;

    public static readonly byte REGION_SIZE = 128;

    public static void getRegionsInRadius(Vector3 center, float radius, List<RegionCoordinate> result)
    {
        Vector3 point = new Vector3(center.x - radius, center.y, center.z - radius);
        Vector3 point2 = new Vector3(center.x + radius, center.y, center.z + radius);
        getUnsafeCoordinates(point, out var x, out var y);
        getUnsafeCoordinates(point2, out var x2, out var y2);
        if (x >= WORLD_SIZE || y >= WORLD_SIZE || x2 < 0 || y2 < 0)
        {
            return;
        }
        x = Mathf.Max(x, 0);
        x2 = Mathf.Min(x2, WORLD_SIZE - 1);
        y = Mathf.Max(y, 0);
        y2 = Mathf.Min(y2, WORLD_SIZE - 1);
        for (int i = x; i <= x2; i++)
        {
            for (int j = y; j <= y2; j++)
            {
                result.Add(new RegionCoordinate((byte)i, (byte)j));
            }
        }
    }

    private static void getUnsafeCoordinates(Vector3 point, out int x, out int y)
    {
        x = Mathf.FloorToInt((point.x + 4096f) / (float)(int)REGION_SIZE);
        y = Mathf.FloorToInt((point.z + 4096f) / (float)(int)REGION_SIZE);
    }

    public static bool tryGetCoordinate(Vector3 point, out byte x, out byte y)
    {
        x = byte.MaxValue;
        y = byte.MaxValue;
        if (checkSafe(point))
        {
            x = (byte)((point.x + 4096f) / (float)(int)REGION_SIZE);
            y = (byte)((point.z + 4096f) / (float)(int)REGION_SIZE);
            return true;
        }
        return false;
    }

    public static bool tryGetPoint(int x, int y, out Vector3 point)
    {
        point = Vector3.zero;
        if (checkSafe(x, y))
        {
            point.x = x * REGION_SIZE - 4096;
            point.z = y * REGION_SIZE - 4096;
            return true;
        }
        return false;
    }

    internal static float HorizontalDistanceFromCenterSquared(int x, int y, Vector3 position)
    {
        return MathfEx.HorizontalDistanceSquared(new Vector3((float)(x * REGION_SIZE) + -4032f, 0f, (float)(y * REGION_SIZE) + -4032f), position);
    }

    public static bool checkSafe(Vector3 point)
    {
        if (point.x >= -4096f && point.z >= -4096f && point.x < 4096f && point.z < 4096f)
        {
            return true;
        }
        return false;
    }

    public static bool clampPositionIntoBounds(ref Vector3 position)
    {
        bool result = false;
        if (position.x < -4095.9f)
        {
            position.x = -4095.9f;
            result = true;
        }
        else if (position.x > 4095.9f)
        {
            position.x = 4095.9f;
            result = true;
        }
        if (position.y < -1023.9f)
        {
            position.y = -1023.9f;
            result = true;
        }
        else if (position.y > 1023.9f)
        {
            position.y = 1023.9f;
            result = true;
        }
        if (position.z < -4095.9f)
        {
            position.z = -4095.9f;
            result = true;
        }
        else if (position.z > 4095.9f)
        {
            position.z = 4095.9f;
            result = true;
        }
        return result;
    }

    public static bool checkSafe(int x, int y)
    {
        if (x >= 0 && y >= 0 && x < WORLD_SIZE && y < WORLD_SIZE)
        {
            return true;
        }
        return false;
    }

    public static bool checkArea(byte x_0, byte y_0, byte x_1, byte y_1, byte area)
    {
        if (x_0 < x_1 - area || y_0 < y_1 - area)
        {
            return false;
        }
        if (x_0 > x_1 + area || y_0 > y_1 + area)
        {
            return false;
        }
        return true;
    }

    public static IEnumerable<ITransportConnection> EnumerateClients(byte x, byte y, byte distance)
    {
        foreach (SteamPlayer client in Provider.clients)
        {
            if (!(client.player == null) && checkArea(x, y, client.player.movement.region_x, client.player.movement.region_y, distance))
            {
                yield return client.transportConnection;
            }
        }
    }

    public static IEnumerable<ITransportConnection> EnumerateClients_Remote(byte x, byte y, byte distance)
    {
        foreach (SteamPlayer client in Provider.clients)
        {
            if (!(client.player == null) && checkArea(x, y, client.player.movement.region_x, client.player.movement.region_y, distance))
            {
                yield return client.transportConnection;
            }
        }
    }
}
