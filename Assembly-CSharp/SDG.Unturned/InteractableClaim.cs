namespace SDG.Unturned;

public class InteractableClaim : Interactable
{
    public ulong owner;

    public ulong group;

    private ClaimBubble bubble;

    private ClaimPlant plant;

    public void updateState(ItemBarricadeAsset asset)
    {
        deregisterClaim();
        registerClaim();
    }

    public override bool checkInteractable()
    {
        return false;
    }

    private void registerClaim()
    {
        if (base.isPlant)
        {
            if (plant == null)
            {
                plant = ClaimManager.registerPlant(base.transform.parent, owner, group);
            }
        }
        else if (bubble == null)
        {
            bubble = ClaimManager.registerBubble(base.transform.position, 32f, owner, group);
        }
    }

    private void deregisterClaim()
    {
        if (bubble != null)
        {
            ClaimManager.deregisterBubble(bubble);
            bubble = null;
        }
        if (plant != null)
        {
            ClaimManager.deregisterPlant(plant);
            plant = null;
        }
    }

    private void OnDisable()
    {
        deregisterClaim();
    }
}
