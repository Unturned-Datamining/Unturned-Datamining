using UnityEngine;

namespace SDG.Unturned;

public class LookAtLocalPlayer : MonoBehaviour
{
    private void LateUpdate()
    {
        if (Player.player != null)
        {
            base.transform.LookAt(Player.player.look.aim);
        }
    }
}
