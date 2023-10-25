using UnityEngine;

namespace SDG.Unturned;

/// <summary>
/// Ideally component Awake/Start order should not matter, but Unturned's menu is a mess.
/// For most players the default order was fine, but it seems it was not deterministic so it would break for some players.
/// </summary>
internal class MenuStartup : MonoBehaviour
{
    [SerializeField]
    public Characters charactersComponent;

    [SerializeField]
    public MenuUI uiComponent;

    private void Start()
    {
        charactersComponent.customStart();
        uiComponent.customStart();
    }
}
