using System;
using UnityEngine;

namespace SDG.Unturned;

public class NightLight : MonoBehaviour
{
    public Light target;

    private Material material;

    private Color emissionColor;

    private bool isListeningLoad;

    private bool isListeningTime;

    private void onLevelLoaded(int index)
    {
        if (isListeningLoad)
        {
            isListeningLoad = false;
            Level.onLevelLoaded = (LevelLoaded)Delegate.Remove(Level.onLevelLoaded, new LevelLoaded(onLevelLoaded));
        }
        if (!isListeningTime)
        {
            isListeningTime = true;
            LightingManager.onDayNightUpdated = (DayNightUpdated)Delegate.Combine(LightingManager.onDayNightUpdated, new DayNightUpdated(onDayNightUpdated));
        }
        onDayNightUpdated(LightingManager.isDaytime);
    }

    private void onDayNightUpdated(bool isDaytime)
    {
        if (target != null)
        {
            target.gameObject.SetActive(!isDaytime);
        }
        if (material != null)
        {
            material.SetColor("_EmissionColor", isDaytime ? Color.black : emissionColor);
        }
    }

    private void Awake()
    {
        material = HighlighterTool.getMaterialInstance(base.transform);
        if (material != null)
        {
            emissionColor = material.GetColor("_EmissionColor");
            if (emissionColor.IsNearlyBlack())
            {
                emissionColor = new Color(1.5f, 1.5f, 1.5f);
            }
        }
        if (Level.isEditor)
        {
            onDayNightUpdated(isDaytime: false);
        }
        else if (!isListeningLoad)
        {
            isListeningLoad = true;
            Level.onLevelLoaded = (LevelLoaded)Delegate.Combine(Level.onLevelLoaded, new LevelLoaded(onLevelLoaded));
        }
    }

    private void OnDestroy()
    {
        if (material != null)
        {
            UnityEngine.Object.DestroyImmediate(material);
        }
        if (isListeningLoad)
        {
            isListeningLoad = false;
            Level.onLevelLoaded = (LevelLoaded)Delegate.Remove(Level.onLevelLoaded, new LevelLoaded(onLevelLoaded));
        }
        if (isListeningTime)
        {
            isListeningTime = false;
            LightingManager.onDayNightUpdated = (DayNightUpdated)Delegate.Remove(LightingManager.onDayNightUpdated, new DayNightUpdated(onDayNightUpdated));
        }
    }
}
