using UnityEngine;

namespace SDG.Unturned;

public class Messager : MonoBehaviour
{
    public EPlayerMessage message;

    private float lastTrigger;

    private void OnTriggerStay(Collider other)
    {
    }

    private void Update()
    {
        if (Time.realtimeSinceStartup - lastTrigger < 0.5f)
        {
            PlayerUI.hint(null, message);
        }
    }
}
