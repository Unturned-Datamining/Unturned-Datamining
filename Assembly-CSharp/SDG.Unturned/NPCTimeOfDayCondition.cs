namespace SDG.Unturned;

public class NPCTimeOfDayCondition : NPCLogicCondition
{
    public int second { get; protected set; }

    public override bool isConditionMet(Player player)
    {
        float num;
        if (LightingManager.day < LevelLighting.bias)
        {
            num = LightingManager.day / LevelLighting.bias;
            num /= 2f;
        }
        else
        {
            num = (LightingManager.day - LevelLighting.bias) / (1f - LevelLighting.bias);
            num = 0.5f + num / 2f;
        }
        num += 0.25f;
        if (num >= 1f)
        {
            num -= 1f;
        }
        int a = (int)(num * 86400f);
        return doesLogicPass(a, second);
    }

    public override string formatCondition(Player player)
    {
        if (string.IsNullOrEmpty(text))
        {
            return null;
        }
        int num = second / 3600;
        int num2 = second / 60 - num * 60;
        int num3 = second - num * 3600 - num2 * 60;
        string arg = $"{num:D2}:{num2:D2}:{num3:D2}";
        return string.Format(text, arg);
    }

    public NPCTimeOfDayCondition(int newSecond, ENPCLogicType newLogicType, string newText, bool newShouldReset)
        : base(newLogicType, newText, newShouldReset)
    {
        second = newSecond;
    }
}
