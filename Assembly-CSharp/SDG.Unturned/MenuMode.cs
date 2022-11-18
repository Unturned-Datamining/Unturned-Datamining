using UnityEngine;

namespace SDG.Unturned;

public class MenuMode : MonoBehaviour
{
    public GameObject desktop;

    public GameObject virtualReality;

    public void Awake()
    {
        desktop.SetActive(!Dedicator.isVR);
        virtualReality.SetActive(Dedicator.isVR);
    }
}
