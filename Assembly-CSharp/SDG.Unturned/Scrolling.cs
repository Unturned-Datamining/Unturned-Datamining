using UnityEngine;

namespace SDG.Unturned;

public class Scrolling : MonoBehaviour
{
    public Material material;

    public float x;

    public float y;

    private void Update()
    {
        material.mainTextureOffset = new Vector2(x * Time.time, y * Time.time);
    }
}
