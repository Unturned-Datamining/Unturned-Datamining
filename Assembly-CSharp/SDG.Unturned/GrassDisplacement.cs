using UnityEngine;

namespace SDG.Unturned;

public class GrassDisplacement : MonoBehaviour
{
    private int _Grass_Displacement_Point = -1;

    private void Update()
    {
        Shader.SetGlobalVector(_Grass_Displacement_Point, new Vector4(base.transform.position.x, base.transform.position.y + 0.5f, base.transform.position.z, 0f));
    }

    private void OnEnable()
    {
        if (_Grass_Displacement_Point == -1)
        {
            _Grass_Displacement_Point = Shader.PropertyToID("_Grass_Displacement_Point");
        }
    }
}
