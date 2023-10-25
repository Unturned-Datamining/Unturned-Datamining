namespace SDG.Unturned;

public class Useable : PlayerCaller
{
    internal float movementSpeedMultiplier = 1f;

    public virtual bool canInspect => true;

    /// <summary>
    /// Does useable have a menu open?
    /// If so pause menu, dashboard, and other menus cannot be opened.
    /// </summary>
    public virtual bool isUseableShowingMenu => false;

    /// <returns>True if primary action was started and stopPrimary should be called in the future.
    /// Useful to allow input to be held until action executes.</returns>
    public virtual bool startPrimary()
    {
        return false;
    }

    public virtual void stopPrimary()
    {
    }

    /// <returns>True if secondary action was started and stopSecondary should be called in the future.
    /// Useful to allow input to be held until action executes.</returns>
    public virtual bool startSecondary()
    {
        return false;
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
