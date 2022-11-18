namespace SDG.Unturned;

public class Interactable2SalvageStructure : Interactable2
{
    public Interactable2HP hp;

    public override bool checkHint(out EPlayerMessage message, out float data)
    {
        message = EPlayerMessage.SALVAGE;
        if (hp != null)
        {
            data = (float)(int)hp.hp / 100f;
        }
        else
        {
            data = 0f;
        }
        if (!base.hasOwnership)
        {
            return false;
        }
        return true;
    }

    public override void use()
    {
        StructureManager.salvageStructure(base.transform);
    }
}
