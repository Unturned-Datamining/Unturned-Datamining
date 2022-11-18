namespace SDG.Unturned;

public class SpecialitySkillPair
{
    public int speciality { get; private set; }

    public int skill { get; private set; }

    public SpecialitySkillPair(int newSpeciality, int newSkill)
    {
        speciality = newSpeciality;
        skill = newSkill;
    }
}
