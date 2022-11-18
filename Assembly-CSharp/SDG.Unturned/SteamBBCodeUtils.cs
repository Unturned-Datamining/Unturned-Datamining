namespace SDG.Unturned;

public static class SteamBBCodeUtils
{
    public static void removeYouTubePreviews(ref string bbcode)
    {
        int num = 0;
        while (num < bbcode.Length)
        {
            int num2 = bbcode.IndexOf("[previewyoutube=", num);
            if (num2 < 0)
            {
                break;
            }
            int num3 = bbcode.IndexOf("[/previewyoutube]", num2 + "[previewyoutube=".Length);
            if (num3 < 0)
            {
                break;
            }
            bbcode = bbcode.Remove(num2, num3 + "[/previewyoutube]".Length - num2);
            num = num2;
        }
    }

    public static void removeCodeFormatting(ref string bbcode)
    {
        bbcode = bbcode.Replace("[code]", string.Empty);
        bbcode = bbcode.Replace("[/code]", string.Empty);
    }
}
