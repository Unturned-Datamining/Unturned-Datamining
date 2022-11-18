namespace SDG.Unturned;

public class Useable : PlayerCaller
{
    internal float movementSpeedMultiplier = 1f;

    public virtual bool canInspect => true;

    public virtual bool isUseableShowingMenu => false;

    public virtual void startPrimary()
    {
    }

    public virtual void stopPrimary()
    {
    }

    public virtual void startSecondary()
    {
    }

    public virtual void stopSecondary()
    {
    }

    public virtual void equip()
    {
    }

    public virtual void dequip()
    {
    }

    public virtual void tick()
    {
    }

    public virtual void simulate(uint simulation, bool inputSteady)
    {
    }

    public virtual void tock(uint clock)
    {
    }

    public virtual void updateState(byte[] newState)
    {
    }
}
