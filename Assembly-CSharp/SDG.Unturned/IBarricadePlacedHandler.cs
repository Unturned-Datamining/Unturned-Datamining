namespace SDG.Unturned;

public interface IBarricadePlacedHandler
{
    /// <summary>
    /// Called on barricade's Interactable after being placed into the world. (Not after loading or replication.)
    /// </summary>
    void OnBarricadePlaced(BarricadeRegion region, BarricadeDrop barricade)
    {
    }
}
