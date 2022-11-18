using UnityEngine;

namespace SDG.Unturned;

public class MenuConfigurationOptions : MonoBehaviour
{
    private static bool hasPlayed;

    private static AudioSource music;

    public static void apply()
    {
        if (!hasPlayed || !OptionsSettings.music)
        {
            hasPlayed = true;
            if (music != null)
            {
                music.enabled = OptionsSettings.music;
            }
        }
    }

    private void Start()
    {
        apply();
    }

    private void Awake()
    {
        music = GetComponent<AudioSource>();
    }
}
