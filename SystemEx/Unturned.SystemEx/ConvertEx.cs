using System;
using System.Text;

namespace Unturned.SystemEx;

public static class ConvertEx
{
    public static bool TryEncodeUtf8StringAsBase64(string input, out string output)
    {
        if (string.IsNullOrEmpty(input))
        {
            output = input;
            return true;
        }
        output = null;
        byte[] bytes;
        try
        {
            bytes = Encoding.UTF8.GetBytes(input);
        }
        catch
        {
            return false;
        }
        string text;
        try
        {
            text = Convert.ToBase64String(bytes);
        }
        catch
        {
            return false;
        }
        output = text;
        return true;
    }

    public static bool TryDecodeBase64AsUtf8String(string input, out string output)
    {
        if (string.IsNullOrEmpty(input))
        {
            output = input;
            return true;
        }
        output = null;
        byte[] bytes;
        try
        {
            bytes = Convert.FromBase64String(input);
        }
        catch
        {
            return false;
        }
        string @string;
        try
        {
            @string = Encoding.UTF8.GetString(bytes);
        }
        catch
        {
            return false;
        }
        output = @string;
        return true;
    }
}
