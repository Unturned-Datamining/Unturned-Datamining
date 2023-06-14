using System.Collections;
using UnityEngine;

namespace SDG.Unturned;

public class Flashbang : MonoBehaviour, IExplodableThrowable
{
    public Color color = Color.white;

    public float fuseLength = 2.5f;

    public bool playAudioSource = true;

    public void Explode()
    {
        if (playAudioSource)
        {
            AudioSource component = GetComponent<AudioSource>();
            if (component != null)
            {
                component.Play();
            }
        }
        Light component2 = GetComponent<Light>();
        if (component2 != null && !component2.enabled)
        {
            component2.enabled = true;
            StartCoroutine(DisableLightNextFrame(component2));
        }
        if (MainCamera.instance != null)
        {
            Vector3 vector = base.transform.position - MainCamera.instance.transform.position;
            if (vector.sqrMagnitude < 1024f)
            {
                float num = Vector3.Dot(vector.normalized, MainCamera.instance.transform.forward);
                if (num > -0.25f)
                {
                    float magnitude = vector.magnitude;
                    if (magnitude < 0.5f || !Physics.Raycast(new Ray(MainCamera.instance.transform.position, vector / magnitude), out var _, magnitude - 0.5f, RayMasks.DAMAGE_SERVER, QueryTriggerInteraction.Ignore))
                    {
                        float num2 = ((!(num > 0.5f)) ? ((num + 0.25f) / 0.75f) : 1f);
                        PlayerUI.stun(amount: num2 * ((!(magnitude > 8f)) ? 1f : (1f - (magnitude - 8f) / 24f)), color: color);
                    }
                }
            }
        }
        AlertTool.alert(base.transform.position, 32f);
        Object.Destroy(base.gameObject, 2.5f);
    }

    private void Start()
    {
        Invoke("Explode", fuseLength);
    }

    private IEnumerator DisableLightNextFrame(Light lightComponent)
    {
        yield return null;
        if (lightComponent != null)
        {
            lightComponent.enabled = false;
        }
    }
}
