using System;
using System.Collections.Generic;
using System.Globalization;
using Steamworks;
using Unturned.SystemEx;

namespace SDG.Unturned;

public class CommandLine
{
    public static GetCommands onGetCommands;

    internal static string commandLineOverride;

    /// <summary>
    /// Full argument string. Defaults to Environment.CommandLine.
    ///
    /// Nelson 2025-06-17: By default, Steam shows a warning nowadays when the game is launched with externally-provided
    /// command-line arguments. For example, when joining a friend via rich presence. The solution is to use the arg
    /// string provided by SteamApps.GetLaunchCommandLine, which also supports *changing* the arguments while the app is
    /// running. If the environment-provided command-line doesn't contain it, the game will append Steam's launch options.
    ///
    /// Note: Steam override isn't applied until Steam is initialized. (after Dedicator and ModuleManager) Please refer to
    /// Setup.cs for the full initialization order.
    /// </summary>
    public static string Get()
    {
        return commandLineOverride;
    }

    /// <summary>
    /// Nelson 2025-06-16: Steam doesn't handle "server code" connect URL, but we now support
    /// it for rich presence joins via server code for easier inviting friends to private servers.
    ///
    /// When Steam parses a steam://connect/ip:port URL it requires the query port (e.g. 27015).
    /// </summary>
    public static bool TryGetSteamConnect(string line, out uint ip, out ushort queryPort, out string pass, out CSteamID serverCode)
    {
        ip = 0u;
        queryPort = 0;
        pass = "";
        serverCode = CSteamID.Nil;
        TryParseValue(line, "+password", out pass);
        if (!TryParseValue(line, "+connect", out var value))
        {
            return false;
        }
        if (ulong.TryParse(value, out serverCode.m_SteamID))
        {
            return true;
        }
        if (IPv4Address.TryParseWithOptionalPort(value, out ip, out ushort? optionalPort) && optionalPort.HasValue)
        {
            queryPort = optionalPort.Value;
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
        string text = Get();
        int num = text.ToLower().IndexOf("+secureserver", StringComparison.OrdinalIgnoreCase);
        if (num != -1)
        {
            visibility = ESteamServerVisibility.Internet;
            id = text.Substring(num + 14, text.Length - num - 14);
            if (id == "Singleplayer")
            {
                return false;
            }
            return true;
        }
        int num2 = text.ToLower().IndexOf("+insecureserver", StringComparison.OrdinalIgnoreCase);
        if (num2 != -1)
        {
            visibility = ESteamServerVisibility.Internet;
            id = text.Substring(num2 + 16, text.Length - num2 - 16);
            if (id == "Singleplayer")
            {
                return false;
            }
            return true;
        }
        int num3 = text.ToLower().IndexOf("+internetserver", StringComparison.OrdinalIgnoreCase);
        if (num3 != -1)
        {
            visibility = ESteamServerVisibility.Internet;
            id = text.Substring(num3 + 16, text.Length - num3 - 16);
            if (id == "Singleplayer")
            {
                return false;
            }
            return true;
        }
        int num4 = text.ToLower().IndexOf("+lanserver", StringComparison.OrdinalIgnoreCase);
        if (num4 != -1)
        {
            visibility = ESteamServerVisibility.LAN;
            id = text.Substring(num4 + 11, text.Length - num4 - 11);
            if (id == "Singleplayer")
            {
                return false;
            }
            return true;
        }
        return false;
    }

    public static string[] getCommands()
    {
        string[] commandLineArgs = Environment.GetCommandLineArgs();
        List<string> list = new List<string>();
        onGetCommands?.Invoke(list);
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

    /// <summary>
    /// Handles these cases:
    /// key value -&gt; value
    /// key=value -&gt; value
    /// key = value -&gt; value
    /// key  =  value -&gt; value
    /// key "value with spaces" -&gt; value with spaces
    /// key "value with \" quotation marks" -&gt; value with " quotation marks
    ///
    /// Tested in CommandLineTests.cs
    /// </summary>
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
        return TryParseValue(Get(), key, out value);
    }

    static CommandLine()
    {
        commandLineOverride = Environment.CommandLine;
    }
}
