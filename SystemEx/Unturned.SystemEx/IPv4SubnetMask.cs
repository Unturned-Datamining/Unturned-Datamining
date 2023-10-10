using System;

namespace Unturned.SystemEx;

public struct IPv4SubnetMask : IEquatable<IPv4SubnetMask>, IComparable<IPv4SubnetMask>
{
    public uint value;

    public static IPv4SubnetMask SingleAddress = new IPv4SubnetMask(uint.MaxValue);

    public bool IsSingleAddress => value == uint.MaxValue;

    public int CountNetworkBits
    {
        get
        {
            if (value == 0)
            {
                return 0;
            }
            if (value == uint.MaxValue)
            {
                return 32;
            }
            return 32 - CountHostBits;
        }
    }

    public int CountHostBits
    {
        get
        {
            if (value == 0)
            {
                return 32;
            }
            if (value == uint.MaxValue)
            {
                return 0;
            }
            int num = 0;
            uint num2 = value;
            while (num2 != 0 && (num2 & 1) == 0)
            {
                num++;
                num2 >>= 1;
            }
            return num;
        }
    }

    public IPv4SubnetMask(int networkBits)
    {
        value = SingleAddress.value << 32 - networkBits;
    }

    public IPv4SubnetMask(uint value)
    {
        this.value = value;
    }

    public bool ContainsHost(IPv4Address routingPrefix, IPv4Address hostAddress)
    {
        return (hostAddress.value & value) == routingPrefix.value;
    }

    public IPv4Address MaskRoutingPrefix(IPv4Address address)
    {
        return new IPv4Address(address.value & value);
    }

    public override string ToString()
    {
        return value.ToString();
    }

    public override bool Equals(object rhs)
    {
        if (rhs is IPv4SubnetMask)
        {
            return this == (IPv4SubnetMask)rhs;
        }
        return false;
    }

    public override int GetHashCode()
    {
        return value.GetHashCode();
    }

    public static bool operator ==(IPv4SubnetMask lhs, IPv4SubnetMask rhs)
    {
        return lhs.value == rhs.value;
    }

    public static bool operator !=(IPv4SubnetMask lhs, IPv4SubnetMask rhs)
    {
        return lhs.value != rhs.value;
    }

    public bool Equals(IPv4SubnetMask rhs)
    {
        return value == rhs.value;
    }

    public int CompareTo(IPv4SubnetMask rhs)
    {
        return value.CompareTo(rhs.value);
    }

    public static bool TryParse(string input, out IPv4SubnetMask mask)
    {
        return TryParse(input, 0, input?.Length ?? 0, out mask);
    }

    public static bool TryParse(string input, int startIndex, int length, out IPv4SubnetMask mask)
    {
        if (string.IsNullOrEmpty(input) || length < 1)
        {
            mask = SingleAddress;
            return false;
        }
        if (!int.TryParse(input.Substring(startIndex, length), out var result))
        {
            mask = SingleAddress;
            return false;
        }
        if (result < 1 || result > 31)
        {
            mask = SingleAddress;
            return false;
        }
        mask = new IPv4SubnetMask(result);
        return true;
    }
}
