using UnityEngine;

namespace SDG.Unturned;

public class LookAtLocalPlayer : MonoBehaviour
{
    private void LateUpdate()
    {
        if (Player.LocalPlayer != null)
        {
            base.transform.LookAt(Player.LocalPlayer.look.aim);
        }
    }
}
