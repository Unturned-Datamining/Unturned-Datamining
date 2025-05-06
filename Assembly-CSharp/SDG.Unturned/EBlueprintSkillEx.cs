namespace SDG.Unturned;

public static class EBlueprintSkillEx
{
    public static string ToStringPascalCase(this EBlueprintSkill skill)
    {
        return skill switch
        {
            EBlueprintSkill.NONE => "None", 
            EBlueprintSkill.CRAFT => "Craft", 
            EBlueprintSkill.COOK => "Cook", 
            EBlueprintSkill.REPAIR => "Repair", 
            _ => skill.ToString(), 
        };
    }
}
