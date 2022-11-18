namespace SDG.Unturned;

public class EconCraftOption
{
    public string token;

    public int generate;

    public ushort scrapsNeeded;

    public EconCraftOption(string newToken, int newGenerate, ushort newScrapsNeeded)
    {
        token = newToken;
        generate = newGenerate;
        scrapsNeeded = newScrapsNeeded;
    }
}
