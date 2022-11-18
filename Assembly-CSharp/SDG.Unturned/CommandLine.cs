using System;
using System.Collections.Generic;
using System.Globalization;

namespace SDG.Unturned;

public class CommandLine
{
    public static GetCommands onGetCommands;

    public static bool TryGetSteamConnect(string line, out uint ip, out ushort queryPort, out string pass)
    {
        ip = 0u;
        queryPort = 0;
        pass = "";
        int num = line.ToLower().IndexOf("+connect ");
        if (num != -1)
        {
            int num2 = line.IndexOf(':', num + 9);
            string text = line.Substring(num + 9, num2 - num - 9);
            if (Parser.checkIP(text))
            {
                ip = Parser.getUInt32FromIP(text);
            }
            else if (!uint.TryParse(text, NumberStyles.Any, CultureInfo.InvariantCulture, out ip))
            {
                return false;
            }
            int num3 = line.IndexOf(' ', num2 + 1);
            if (num3 == -1)
            {
                if (!ushort.TryParse(line.Substring(num2 + 1, line.Length - num2 - 1), NumberStyles.Any, CultureInfo.InvariantCulture, out queryPort))
                {
                    return false;
                }
                int num4 = line.ToLower().IndexOf("+password ");
                if (num4 != -1)
                {
                    pass = line.Substring(num4 + 10, line.Length - num4 - 10);
                }
                return true;
            }
            if (!ushort.TryParse(line.Substring(num2 + 1, num3 - num2 - 1), NumberStyles.Any, CultureInfo.InvariantCulture, out queryPort))
            {
                return false;
            }
            int num5 = line.ToLower().IndexOf("+password ");
            if (num5 != -1)
            {
                pass = line.Substring(num5 + 10, line.Length - num5 - 10);
            }
            return true;
        }
        return false;
    }

    public static bool tryGetLobby(string line, out ulong lobby)
    {
        lobby = 0uL;
        int num = line.ToLower().IndexOf("+connect_lobby ");
        if (num != -1)
        {
            int num2 = line.IndexOf(' ', num + 15);
            if (num2 == -1)
            {
                return ulong.TryParse(line.Substring(num + 15, line.Length - num - 15), NumberStyles.Any, CultureInfo.InvariantCulture, out lobby);
            }
            return ulong.TryParse(line.Substring(num + 15, num2 - num - 15), NumberStyles.Any, CultureInfo.InvariantCulture, out lobby);
        }
        return false;
    }

    public static bool tryGetLanguage(out string local, out string path)
    {
        local = "";
        path = "";
        string[] commandLineArgs = Environment.GetCommandLineArgs();
        for (int i = 0; i < commandLineArgs.Length; i++)
        {
            string text = null;
            if (commandLineArgs[i].Length > 6 && (commandLineArgs[i].StartsWith("-Lang=", StringComparison.InvariantCultureIgnoreCase) || commandLineArgs[i].StartsWith("+Lang=", StringComparison.InvariantCultureIgnoreCase)))
            {
                text = commandLineArgs[i].Substring(6);
            }
            else if (commandLineArgs[i].Length > 5 && (commandLineArgs[i].StartsWith("-Loc=", StringComparison.InvariantCultureIgnoreCase) || commandLineArgs[i].StartsWith("+Loc=", StringComparison.InvariantCultureIgnoreCase)))
            {
                text = commandLineArgs[i].Substring(5);
            }
            else if (commandLineArgs[i].Length > 1 && commandLineArgs[i].StartsWith("+"))
            {
                if (commandLineArgs[i].IndexOf('/') >= 0 || commandLineArgs[i].StartsWith("+connect") || commandLineArgs[i].StartsWith("+password"))
                {
                    continue;
                }
                text = commandLineArgs[i].Substring(1);
            }
            if (string.IsNullOrEmpty(text))
            {
                continue;
            }
            if (Provider.provider.workshopService.ugc != null)
            {
                for (int j = 0; j < Provider.provider.workshopService.ugc.Count; j++)
                {
                    SteamContent steamContent = Provider.provider.workshopService.ugc[j];
                    if (steamContent.type == ESteamUGCType.LOCALIZATION && ReadWrite.folderExists(steamContent.path + "/" + text, usePath: false))
                    {
                        local = text;
                        path = steamContent.path + "/";
                        UnturnedLog.info("Parsed language '{0}' on command-line, and found in workshop item {1}", text, steamContent.publishedFileID);
                        return true;
                    }
                }
            }
            if (ReadWrite.folderExists("/Localization/" + text))
            {
                local = text;
                path = ReadWrite.PATH + "/Localization/";
                UnturnedLog.info("Parsed language '{0}' on command-line, and found in root Localization directory", text);
                return true;
            }
            if (ReadWrite.folderExists("/Sandbox/" + text))
            {
                local = text;
                path = ReadWrite.PATH + "/Sandbox/";
                UnturnedLog.info("Parsed language '{0}' on command-line, and found in Sandbox directory", text);
                return true;
            }
            UnturnedLog.warn("Parsed language '{0}' on command-line, but unable to find related files", text);
        }
        return false;
    }

    public static bool tryGetServer(out ESteamServerVisibility visibility, out string id)
    {
        visibility = ESteamServerVisibility.LAN;
        id = "";
        string commandLine = Environment.CommandLine;
        int num = commandLine.ToLower().IndexOf("+secureserver", StringComparison.OrdinalIgnoreCase);
        if (num != -1)
        {
            visibility = ESteamServerVisibility.Internet;
            id = commandLine.Substring(num + 14, commandLine.Length - num - 14);
            if (id == "Singleplayer")
            {
                return false;
            }
            return true;
        }
        int num2 = commandLine.ToLower().IndexOf("+insecureserver", StringComparison.OrdinalIgnoreCase);
        if (num2 != -1)
        {
            visibility = ESteamServerVisibility.Internet;
            id = commandLine.Substring(num2 + 16, commandLine.Length - num2 - 16);
            if (id == "Singleplayer")
            {
                return false;
            }
            return true;
        }
        int num3 = commandLine.ToLower().IndexOf("+internetserver", StringComparison.OrdinalIgnoreCase);
        if (num3 != -1)
        {
            visibility = ESteamServerVisibility.Internet;
            id = commandLine.Substring(num3 + 16, commandLine.Length - num3 - 16);
            if (id == "Singleplayer")
            {
                return false;
            }
            return true;
        }
        int num4 = commandLine.ToLower().IndexOf("+lanserver", StringComparison.OrdinalIgnoreCase);
        if (num4 != -1)
        {
            visibility = ESteamServerVisibility.LAN;
            id = commandLine.Substring(num4 + 11, commandLine.Length - num4 - 11);
            if (id == "Singleplayer")
            {
                return false;
            }
            return true;
        }
        return false;
    }

    public static bool tryGetVR()
    {
        return Environment.CommandLine.ToLower().IndexOf("-vr") != -1;
    }

    public static string[] getCommands()
    {
        string[] commandLineArgs = Environment.GetCommandLineArgs();
        List<string> list = new List<string>();
        if (onGetCommands != null)
        {
            onGetCommands(list);
        }
        bool flag = false;
        for (int i = 0; i < commandLineArgs.Length; i++)
        {
            if (commandLineArgs[i].Substring(0, 1) == "+")
            {
                flag = true;
            }
            else if (commandLineArgs[i].Substring(0, 1) == "-")
            {
                list.Add(commandLineArgs[i].Substring(1, commandLineArgs[i].Length - 1));
                flag = false;
            }
            else if (list.Count > 0 && !flag)
            {
                List<string> list2 = list;
                int index = list.Count - 1;
                list2[index] = list2[index] + " " + commandLineArgs[i];
            }
        }
        return list.ToArray();
    }

    public static bool TryParseValue(string input, string key, out string value)
    {
        value = null;
        if (string.IsNullOrWhiteSpace(input) || string.IsNullOrWhiteSpace(key))
        {
            return false;
        }
        int num = 0;
        while (num < input.Length)
        {
            int num2 = input.IndexOf(key, num, StringComparison.InvariantCultureIgnoreCase);
            if (num2 < 0)
            {
                return false;
            }
            int num3 = num2 + key.Length;
            if (num3 >= input.Length)
            {
                return false;
            }
            char c = input[num3];
            if (c != '=' && !char.IsWhiteSpace(c))
            {
                num = num3;
                continue;
            }
            int num4 = num3 + 1;
            while (true)
            {
                if (num4 >= input.Length)
                {
                    return false;
                }
                char c2 = input[num4];
                if (c2 != '=' && !char.IsWhiteSpace(c2))
                {
                    break;
                }
                num4++;
            }
            if (input[num4] != '"')
            {
                int num5 = input.IndexOf(' ', num4);
                if (num5 < 0)
                {
                    value = input.Substring(num4);
                }
                else
                {
                    int length = num5 - num4;
                    value = input.Substring(num4, length);
                }
                return true;
            }
            num4++;
            int num6 = num4;
            bool flag = false;
            value = string.Empty;
            while (num6 < input.Length)
            {
                char c3 = input[num6];
                switch (c3)
                {
                case '\\':
                    num6++;
                    flag = true;
                    continue;
                case '"':
                    if (!flag)
                    {
                        return true;
                    }
                    break;
                }
                value += c3;
                num6++;
                flag = false;
            }
            return false;
        }
        return false;
    }

    public static bool TryParseValue(string key, out string value)
    {
        return TryParseValue(Environment.CommandLine, key, out value);
    }
}
