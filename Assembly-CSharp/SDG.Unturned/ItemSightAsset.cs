using System;
using System.Collections.Generic;
using UnityEngine;

namespace SDG.Unturned;

public class ItemSightAsset : ItemCaliberAsset
{
    public struct DistanceMarker : IDatParseable
    {
        public enum ESide
        {
            Left,
            Right
        }

        public float distance;

        public float lineOffset;

        public float lineWidth;

        public ESide side;

        public bool hasLabel;

        public Color32 color;

        public bool TryParse(IDatNode node)
        {
            if (node is DatDictionary datDictionary)
            {
                if (!datDictionary.TryParseFloat("Distance", out distance))
                {
                    return false;
                }
                lineOffset = datDictionary.ParseFloat("LineOffset");
                lineWidth = datDictionary.ParseFloat("LineWidth", 0.05f);
                side = datDictionary.ParseEnum("Side", ESide.Right);
                hasLabel = datDictionary.ParseBool("HasLabel", defaultValue: true);
                color = datDictionary.ParseColor32RGB("Color");
                return true;
            }
            return false;
        }
    }

    protected GameObject _sight;

    private ELightingVision _vision;

    public Color nightvisionColor;

    public float nightvisionFogIntensity;

    private bool _isHolographic;

    public bool shouldZoomUsingEyes;

    public bool shouldOffsetScopeOverlayByOneTexel;

    public List<DistanceMarker> distanceMarkers;

    public GameObject sight => _sight;

    public ELightingVision vision => _vision;

    public float zoom { get; private set; }

    public float thirdPersonZoomFactor { get; private set; }

    public bool isHolographic => _isHolographic;

    public override void BuildDescription(ItemDescriptionBuilder builder, Item itemInstance)
    {
        base.BuildDescription(builder, itemInstance);
        if (!builder.shouldRestrictToLegacyContent)
        {
            if (zoom != 1f)
            {
                builder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_ZoomFactor", zoom), 10000);
            }
            if (thirdPersonZoomFactor != 1.25f)
            {
                builder.Append(PlayerDashboardInventoryUI.localization.format("ItemDescription_ThirdPersonZoomFactor", thirdPersonZoomFactor), 10001);
            }
        }
    }

    public override void PopulateAsset(Bundle bundle, DatDictionary data, Local localization)
    {
        base.PopulateAsset(bundle, data, localization);
        _sight = loadRequiredAsset<GameObject>(bundle, "Sight");
        if (data.ContainsKey("Vision"))
        {
            _vision = (ELightingVision)Enum.Parse(typeof(ELightingVision), data.GetString("Vision"), ignoreCase: true);
            if (vision == ELightingVision.CIVILIAN)
            {
                nightvisionColor = data.LegacyParseColor32RGB("Nightvision_Color", LevelLighting.NIGHTVISION_CIVILIAN);
                nightvisionFogIntensity = data.ParseFloat("Nightvision_Fog_Intensity", 0.5f);
            }
            else if (vision == ELightingVision.MILITARY)
            {
                nightvisionColor = data.LegacyParseColor32RGB("Nightvision_Color", LevelLighting.NIGHTVISION_MILITARY);
                nightvisionFogIntensity = data.ParseFloat("Nightvision_Fog_Intensity", 0.25f);
            }
        }
        else
        {
            _vision = ELightingVision.NONE;
        }
        zoom = Mathf.Max(1f, data.ParseFloat("Zoom"));
        thirdPersonZoomFactor = Mathf.Max(1f, data.ParseFloat("ThirdPerson_Zoom", 1.25f));
        shouldZoomUsingEyes = data.ParseBool("Zoom_Using_Eyes");
        shouldOffsetScopeOverlayByOneTexel = data.ParseBool("Offset_Scope_Overlay_By_One_Texel");
        _isHolographic = data.ContainsKey("Holographic");
        distanceMarkers = data.ParseListOfStructs<DistanceMarker>("DistanceMarkers");
    }
}
