namespace SDG.Unturned;

public class InteractableObject : InteractablePower
{
    protected ObjectAsset _objectAsset;

    internal LevelObject owningLevelObject;

    public ObjectAsset objectAsset => _objectAsset;

    /// <summary>
    /// True if rubble is not applicable or all sections are alive.
    /// </summary>
    public bool IsRubbleNullOrAllAlive
    {
        get
        {
            if (owningLevelObject != null && !(owningLevelObject.rubble == null))
            {
                return owningLevelObject.rubble.isAllAlive();
            }
            return true;
        }
    }

    public override void updateState(Asset asset, byte[] state)
    {
        base.updateState(asset, state);
        _objectAsset = asset as ObjectAsset;
    }

    private void Start()
    {
        RefreshIsConnectedToPower();
    }
}
