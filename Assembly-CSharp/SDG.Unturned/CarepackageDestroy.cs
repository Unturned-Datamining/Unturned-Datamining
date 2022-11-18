using System.Collections;
using Steamworks;
using UnityEngine;

namespace SDG.Unturned;

public class CarepackageDestroy : MonoBehaviour
{
    private IEnumerator cleanup()
    {
        yield return new WaitForSeconds(600f);
        BarricadeManager.damage(base.transform, 65000f, 1f, armor: false, default(CSteamID), EDamageOrigin.Carepackage_Timeout);
    }

    private void Start()
    {
        StartCoroutine("cleanup");
    }
}
