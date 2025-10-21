using TMPro;
using UnityEngine;

namespace SDG.Unturned;

public class StatTracker : MonoBehaviour
{
    public GetStatTrackerValueHandler statTrackerCallback;

    protected int oldStatValue = -1;

    private static StaticResourceRef<GameObject> statTrackerRef = new StaticResourceRef<GameObject>("Economy/Attachments/Stat_Tracker");

    public TextMeshPro statTrackerText { get; protected set; }

    public Transform statTrackerHook { get; protected set; }

    public void updateStatTracker(bool viewmodel)
    {
        InstantiateParameters instantiateParameters = default(InstantiateParameters);
        instantiateParameters.parent = statTrackerHook;
        instantiateParameters.worldSpace = false;
        InstantiateParameters parameters = instantiateParameters;
        GameObject gameObject = Object.Instantiate((GameObject)statTrackerRef, Vector3.zero, Quaternion.identity, parameters);
        statTrackerText = gameObject.GetComponentInChildren<TextMeshPro>();
        if (viewmodel)
        {
            Layerer.relayer(gameObject.transform, 11);
        }
    }

    protected void Update()
    {
        if (statTrackerCallback != null && statTrackerCallback(out var type, out var kills))
        {
            kills %= 10000000;
            if (oldStatValue != kills)
            {
                oldStatValue = kills;
                statTrackerText.color = Provider.provider.economyService.getStatTrackerColor(type);
                statTrackerText.text = kills.ToString("D7");
            }
        }
    }

    protected void Awake()
    {
        statTrackerHook = base.transform.Find("Stat_Tracker");
    }
}
