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

    public static void ToSkillIndices(this EBlueprintSkill skill, out int specialityIndex, out int skillIndex)
    {
        switch (skill)
        {
        case EBlueprintSkill.CRAFT:
            specialityIndex = 2;
            skillIndex = 1;
            break;
        case EBlueprintSkill.COOK:
            specialityIndex = 2;
            skillIndex = 3;
            break;
        case EBlueprintSkill.REPAIR:
            specialityIndex = 2;
            skillIndex = 7;
            break;
        default:
            specialityIndex = -1;
            skillIndex = -1;
            break;
        }
    }
}
