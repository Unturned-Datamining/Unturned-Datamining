using UnityEngine;

namespace SDG.Unturned;

public class MenuConfigurationOptions : MonoBehaviour
{
    private static bool hasPlayed;

    private static AudioSource music;

    public static void apply()
    {
        if (!hasPlayed)
        {
            hasPlayed = true;
            if (music != null)
            {
                music.enabled = OptionsSettings.MainMenuMusicVolume > 0f;
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
