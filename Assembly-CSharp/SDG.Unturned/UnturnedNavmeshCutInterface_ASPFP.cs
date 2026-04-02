using Pathfinding;

namespace SDG.Unturned;

public class UnturnedNavmeshCutInterface_ASPFP : IUnturnedNavmeshCutInterface
{
    private NavmeshCut cutComponent;

    private float initialHeight;

    private bool isActive;

    public bool IsActive
    {
        get
        {
            return isActive;
        }
        set
        {
            if (isActive != value)
            {
                isActive = value;
                if (isActive)
                {
                    cutComponent.height = initialHeight;
                }
                else
                {
                    cutComponent.height = 0f;
                }
                cutComponent.ForceUpdate();
            }
        }
    }

    public UnturnedNavmeshCutInterface_ASPFP(NavmeshCut cutComponent)
    {
        this.cutComponent = cutComponent;
        initialHeight = cutComponent.height;
        isActive = true;
    }
}
