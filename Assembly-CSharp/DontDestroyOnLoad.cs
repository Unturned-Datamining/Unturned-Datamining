using UnityEngine;

public class DontDestroyOnLoad : MonoBehaviour
{
    private void OnEnable()
    {
        Object.DontDestroyOnLoad(base.gameObject);
    }
}
