using UnityEngine;

namespace SDG.Unturned;

public class FlickeringLight : MonoBehaviour
{
    public Light target;

    private Material material;

    private float blackoutTime;

    private float blackoutDelay;

    private void Update()
    {
        float num = Random.Range(0.9f, 1.4f);
        if (Time.time - blackoutTime < 0.15f)
        {
            num = 0.15f;
        }
        else if (Time.time - blackoutTime > blackoutDelay)
        {
            blackoutTime = Time.time;
            blackoutDelay = Random.Range(7.3f, 13.2f);
        }
        if (target != null)
        {
            target.intensity = num;
        }
        if (material != null)
        {
            material.SetColor("_EmissionColor", new Color(num, num, num));
        }
    }

    private void Awake()
    {
        material = HighlighterTool.getMaterialInstance(base.transform);
        blackoutTime = Time.time;
        blackoutDelay = Random.Range(0f, 13.2f);
    }

    private void OnDestroy()
    {
        if (material != null)
        {
            Object.DestroyImmediate(material);
        }
    }
}
