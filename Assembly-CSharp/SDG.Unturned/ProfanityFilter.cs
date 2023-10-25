using System.Collections.Generic;
using System.IO;
using Steamworks;

namespace SDG.Unturned;

public class ProfanityFilter
{
    internal delegate void FilterDelegate(ref string message);

    private static string[] curseWords = null;

    internal static FilterDelegate filter = NaiveDefaultFilter;

    public static CommandLineFlag shouldInitSteamTextFiltering = new CommandLineFlag(defaultValue: true, "-NoSteamTextFiltering");

    /// <summary>
    /// 2023-04-17: suggestion is to have a hardcoded list of hate speech that gets filtered
    /// regardless of whether profanity filter is enabled. (https://forum.smartlydressedgames.com/t/22477)
    /// </summary>
    private static readonly string[] hardcodedBannedWords = new string[18]
    {
        "nigger", "niggers", "niger", "nigers", "jew", "jews", "fag", "fags", "faggot", "faggots",
        "fagot", "fagots", "faggit", "faggits", "fagit", "fagits", "rape", "raped"
    };

    public static string[] getCurseWords()
    {
        if (curseWords == null)
        {
            LoadCurseWords();
        }
        return curseWords;
    }

    internal static void InitSteam()
    {
        if ((bool)shouldInitSteamTextFiltering)
        {
            if (SteamUtils.InitFilterText())
            {
                filter = SteamFilter;
            }
            else
            {
                UnturnedLog.info("Unable to initialize Steam text filtering");
            }
        }
        else
        {
            UnturnedLog.info("Not initializing Steam text filtering");
        }
    }

    private static void SteamFilter(ref string message)
    {
        if (SteamUtils.FilterText(ETextFilteringContext.k_ETextFilteringContextUnknown, CSteamID.Nil, message, out var pchOutFilteredText, (uint)(message.Length * 2 + 1)) > 0)
        {
            message = pchOutFilteredText;
        }
    }

    private static void NaiveDefaultFilter(ref string message)
    {
        filterOutCurseWords(ref message);
    }

    internal static void ApplyFilter(bool enableProfanityFilter, ref string message)
    {
        if (NaiveContainsHardcodedBannedWord(message))
        {
            message = "<3";
        }
        else if (enableProfanityFilter)
        {
            filter(ref message);
        }
    }

    public static bool filterOutCurseWords(ref string text, char replacementChar = '#')
    {
        bool result = false;
        if (text.Length > 0)
        {
            string[] array = getCurseWords();
            foreach (string text2 in array)
            {
                int num = indexOfCurseWord(text, text2, 0);
                while (num != -1)
                {
                    if ((num == 0 || !char.IsLetterOrDigit(text[num - 1])) && (num == text.Length - text2.Length || !char.IsLetterOrDigit(text[num + text2.Length])))
                    {
                        replaceCurseWord(ref text, num, text2.Length, replacementChar);
                        num = indexOfCurseWord(text, text2, num);
                        result = true;
                    }
                    else
                    {
                        num = indexOfCurseWord(text, text2, num + 1);
                    }
                }
            }
        }
        return result;
    }

    public static bool NaiveContainsHardcodedBannedWord(string message)
    {
        if (string.IsNullOrEmpty(message))
        {
            return false;
        }
        string[] array = hardcodedBannedWords;
        foreach (string text in array)
        {
            for (int num = indexOfCurseWord(message, text, 0); num != -1; num = indexOfCurseWord(message, text, num + 1))
            {
                if ((num == 0 || !char.IsLetterOrDigit(message[num - 1])) && (num == message.Length - text.Length || !char.IsLetterOrDigit(message[num + text.Length])))
                {
                    return true;
                }
            }
        }
        return false;
    }

    private static int indexOfCurseWord(string userText, string curseWord, int startIndex)
    {
        int num = userText.Length - curseWord.Length;
        for (int i = startIndex; i <= num; i++)
        {
            bool flag = true;
            for (int j = 0; j < curseWord.Length; j++)
            {
                char c = char.ToLower(userText[i + j]);
                char c2 = curseWord[j];
                bool flag2 = c == c2;
                if (!flag2)
                {
                    switch (c2)
                    {
                    case 'a':
                        flag2 = c == '4' || c == '@';
                        break;
                    case 'e':
                        flag2 = c == '3';
                        break;
                    case 'h':
                        flag2 = c == '#';
                        break;
                    case 'i':
                        flag2 = c == '1';
                        break;
                    case 'l':
                        flag2 = c == '1';
                        break;
                    case 'o':
                        flag2 = c == '0';
                        break;
                    case 's':
                        flag2 = c == '$' || c == '5';
                        break;
                    case 't':
                        flag2 = c == '7';
                        break;
                    }
                    if (!flag2)
                    {
                        flag = false;
                        break;
                    }
                }
            }
            if (flag)
            {
                return i;
            }
        }
        return -1;
    }

    private static void replaceCurseWord(ref string text, int startIndex, int curseWordLength, char replacementChar)
    {
        string text2 = text.Substring(0, startIndex);
        for (int i = 0; i < curseWordLength; i++)
        {
            text2 += replacementChar;
        }
        text2 += text.Substring(startIndex + curseWordLength, text.Length - startIndex - curseWordLength);
        text = text2;
    }

    private static void LoadCurseWords()
    {
        if (string.IsNullOrEmpty(Provider.localizationRoot))
        {
            curseWords = File.ReadAllLines(ReadWrite.PATH + "/Localization/English/Curse_Words.txt");
        }
        else
        {
            string path = Provider.localizationRoot + "/Curse_Words.txt";
            if (File.Exists(path))
            {
                curseWords = File.ReadAllLines(path);
            }
            else
            {
                string path2 = Provider.path + "/English/Curse_Words.txt";
                if (File.Exists(path2))
                {
                    curseWords = File.ReadAllLines(path2);
                }
                else
                {
                    curseWords = new string[0];
                }
            }
        }
        if (curseWords == null || curseWords.Length < 1)
        {
            UnturnedLog.error("Failed to load list of curse words for profanity filter!");
            curseWords = new string[0];
        }
        else
        {
            ProcessLoadedCurseWords();
        }
    }

    private static void ProcessLoadedCurseWords()
    {
        List<string> list = new List<string>();
        for (int num = curseWords.Length - 1; num >= 0; num--)
        {
            string text = curseWords[num];
            if (!string.IsNullOrEmpty(text) && !text.StartsWith("#"))
            {
                list.Add(text);
            }
        }
        curseWords = list.ToArray();
    }
}
