using UnityEngine;

namespace SDG.Unturned;

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
