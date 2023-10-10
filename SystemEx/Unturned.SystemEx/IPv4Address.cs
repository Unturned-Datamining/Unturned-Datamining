using System;

namespace Unturned.SystemEx;

public struct IPv4Address : IEquatable<IPv4Address>, IComparable<IPv4Address>
{
    public uint value;

    public bool IsLoopback => value >> 24 == 127;

    public bool IsLocalPrivate
    {
        get
        {
            if (value >> 24 != 10 && value >> 20 != 2753)
            {
                return value >> 16 == 49320;
            }
            return true;
        }
    }

    public bool IsLinkLocal => value >> 16 == 43518;

    public bool IsWideAreaNetwork
    {
        get
        {
            if (!IsLoopback && !IsLocalPrivate)
            {
                return !IsLinkLocal;
            }
            return false;
        }
    }

    public IPv4Address(uint value)
    {
        this.value = value;
    }

    public IPv4Address(string input)
    {
        TryParse(input, out value);
    }

    public override string ToString()
    {
        return $"{value >> 24}.{(value >> 16) & 0xFFu}.{(value >> 8) & 0xFFu}.{value & 0xFFu}";
    }

    public override bool Equals(object rhs)
    {
        if (rhs is IPv4Address)
        {
            return this == (IPv4Address)rhs;
        }
        return false;
    }

    public override int GetHashCode()
    {
        return value.GetHashCode();
    }

    public static bool operator ==(IPv4Address lhs, IPv4Address rhs)
    {
        return lhs.value == rhs.value;
    }

    public static bool operator !=(IPv4Address lhs, IPv4Address rhs)
    {
        return lhs.value != rhs.value;
    }

    public bool Equals(IPv4Address rhs)
    {
        return value == rhs.value;
    }

    public int CompareTo(IPv4Address rhs)
    {
        return value.CompareTo(rhs.value);
    }

    public static bool TryParse(string input, out uint address)
    {
        return TryParse(input, 0, input?.Length ?? 0, out address);
    }

    public static bool TryParse(string input, int startIndex, int length, out uint address)
    {
        address = 0u;
        if (string.IsNullOrEmpty(input) || length < 7)
        {
            return false;
        }
        int num = startIndex + length;
        int num2 = input.IndexOf('.', startIndex);
        if (num2 < startIndex + 1 || num2 + 6 > num)
        {
            return false;
        }
        int num3 = input.IndexOf('.', num2 + 2);
        if (num3 < 0 || num3 + 4 > num)
        {
            return false;
        }
        int num4 = input.IndexOf('.', num3 + 2);
        if (num4 < 0 || num4 + 2 > num)
        {
            return false;
        }
        string s = input.Substring(startIndex, num2 - startIndex);
        string s2 = input.Substring(num2 + 1, num3 - num2 - 1);
        string s3 = input.Substring(num3 + 1, num4 - num3 - 1);
        string s4 = input.Substring(num4 + 1, num - num4 - 1);
        if (!uint.TryParse(s, out var result) || !uint.TryParse(s2, out var result2) || !uint.TryParse(s3, out var result3) || !uint.TryParse(s4, out var result4))
        {
            return false;
        }
        if (result > 255 || result2 > 255 || result3 > 255 || result4 > 255)
        {
            return false;
        }
        address = (result << 24) | (result2 << 16) | (result3 << 8) | result4;
        return true;
    }

    public static bool TryParsePortRange(string input, out ushort minPort, out ushort maxPort)
    {
        return TryParsePortRange(input, 0, input?.Length ?? 0, out minPort, out maxPort);
    }

    public static bool TryParsePortRange(string input, int startIndex, int length, out ushort minPort, out ushort maxPort)
    {
        minPort = 0;
        maxPort = ushort.MaxValue;
        if (string.IsNullOrEmpty(input))
        {
            return false;
        }
        int num = input.IndexOf('-', startIndex, length);
        if (num < 0)
        {
            if (ushort.TryParse(input.Substring(startIndex, length), out var result))
            {
                minPort = result;
                maxPort = result;
                return true;
            }
            return false;
        }
        int num2 = startIndex + length;
        if (num < startIndex + 1 || num + 1 > num2)
        {
            return false;
        }
        string s = input.Substring(startIndex, num - startIndex);
        string s2 = input.Substring(num + 1, num2 - num - 1);
        if (!ushort.TryParse(s, out minPort) || !ushort.TryParse(s2, out maxPort))
        {
            return false;
        }
        if (minPort > maxPort)
        {
            ushort num3 = maxPort;
            maxPort = minPort;
            minPort = num3;
        }
        return true;
    }

    public static bool TryParse(string input, out IPv4Address address)
    {
        return TryParse(input, out address.value);
    }

    public static bool TryParse(string input, int startIndex, int length, out IPv4Address address)
    {
        return TryParse(input, startIndex, length, out address.value);
    }

    public static bool TryParseWithOptionalPort(string input, out uint address, out ushort? optionalPort)
    {
        if (string.IsNullOrEmpty(input))
        {
            address = 0u;
            optionalPort = null;
            return false;
        }
        int num = input.LastIndexOf(':');
        if (num < 0)
        {
            optionalPort = null;
            return TryParse(input, out address);
        }
        if (ushort.TryParse(input.Substring(num + 1), out var result))
        {
            optionalPort = result;
            return TryParse(input, 0, num, out address);
        }
        address = 0u;
        optionalPort = null;
        return false;
    }

    public static bool TryParseWithOptionalPort(string input, out IPv4Address address, out ushort? optionalPort)
    {
        return TryParseWithOptionalPort(input, out address.value, out optionalPort);
    }

    public static bool TryParseWithOptionalPortRange(string input, out uint address, out ushort? optionalMinPort, out ushort? optionalMaxPort)
    {
        if (string.IsNullOrEmpty(input))
        {
            address = 0u;
            optionalMinPort = null;
            optionalMaxPort = null;
            return false;
        }
        int num = input.LastIndexOf(':');
        if (num < 0)
        {
            optionalMinPort = null;
            optionalMaxPort = null;
            return TryParse(input, out address);
        }
        if (TryParsePortRange(input.Substring(num + 1), out var minPort, out var maxPort))
        {
            optionalMinPort = minPort;
            optionalMaxPort = maxPort;
            return TryParse(input, 0, num, out address);
        }
        address = 0u;
        optionalMinPort = null;
        optionalMaxPort = null;
        return false;
    }

    public static bool TryParseWithOptionalPortRange(string input, out IPv4Address address, out ushort? optionalMinPort, out ushort? optionalMaxPort)
    {
        return TryParseWithOptionalPortRange(input, out address.value, out optionalMinPort, out optionalMaxPort);
    }
}
