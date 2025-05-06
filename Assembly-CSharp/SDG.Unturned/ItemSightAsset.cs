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

        /// <summary>
        /// [0, 1] local distance from center to start of line.
        /// </summary>
        public float lineOffset;

        /// <summary>
        /// [0, 1] local width of horizontal line.
        /// </summary>
        public float lineWidth;

        /// <summary>
        /// Whether line/number are on left or right side of the center line.
        /// </summary>
        public ESide side;

        /// <summary>
        /// If true, text label for distance is visible.
        /// </summary>
        public bool hasLabel;

        public Color32 color;

        public bool TryParse(IDatNode node)
        {
            if (node is IDatDictionary dictionary)
            {
                if (!dictionary.TryParseFloat("Distance", out distance))
                {
                    return false;
                }
                lineOffset = dictionary.ParseFloat("LineOffset");
                lineWidth = dictionary.ParseFloat("LineWidth", 0.05f);
                side = dictionary.ParseEnum("Side", ESide.Right);
                hasLabel = dictionary.ParseBool("HasLabel", defaultValue: true);
                color = dictionary.ParseColor32RGB("Color");
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

    /// <summary>
    /// Whether main camera field of view should zoom without scope camera / scope overlay.
    /// </summary>
    public bool shouldZoomUsingEyes;

    /// <summary>
    /// If true, scale scope overly by 1 texel to keep "middle" pixel centered.
    /// </summary>
    public bool shouldOffsetScopeOverlayByOneTexel;

    public List<DistanceMarker> distanceMarkers;

    public GameObject sight => _sight;

    public ELightingVision vision => _vision;

    /// <summary>
    /// Factor e.g. 2 is a 2x multiplier.
    /// Prior to 2022-04-11 this was the target field of view. (90/fov)
    /// </summary>
    public float zoom { get; private set; }

    /// <summary>
    /// Zoom factor used in third-person view.
    /// </summary>
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

    public override void PopulateAsset(in PopulateAssetParameters p)
    {
        base.PopulateAsset(in p);
        _sight = loadRequiredAsset<GameObject>(p.bundle, "Sight");
        if (p.data.ContainsKey("Vision"))
        {
            _vision = (ELightingVision)Enum.Parse(typeof(ELightingVision), p.data.GetString("Vision"), ignoreCase: true);
            if (vision == ELightingVision.CIVILIAN)
            {
                nightvisionColor = p.data.LegacyParseColor32RGB("Nightvision_Color", LevelLighting.NIGHTVISION_CIVILIAN);
                nightvisionFogIntensity = p.data.ParseFloat("Nightvision_Fog_Intensity", 0.5f);
            }
            else if (vision == ELightingVision.MILITARY)
            {
                nightvisionColor = p.data.LegacyParseColor32RGB("Nightvision_Color", LevelLighting.NIGHTVISION_MILITARY);
                nightvisionFogIntensity = p.data.ParseFloat("Nightvision_Fog_Intensity", 0.25f);
            }
        }
        else
        {
            _vision = ELightingVision.NONE;
        }
        zoom = Mathf.Max(1f, p.data.ParseFloat("Zoom"));
        thirdPersonZoomFactor = Mathf.Max(1f, p.data.ParseFloat("ThirdPerson_Zoom", 1.25f));
        shouldZoomUsingEyes = p.data.ParseBool("Zoom_Using_Eyes");
        shouldOffsetScopeOverlayByOneTexel = p.data.ParseBool("Offset_Scope_Overlay_By_One_Texel");
        _isHolographic = p.data.ContainsKey("Holographic");
        distanceMarkers = p.data.ParseListOfStructs<DistanceMarker>("DistanceMarkers");
    }

    internal override void BuildCargoData(CargoBuilder builder)
    {
        base.BuildCargoData(builder);
        CargoDeclaration orAddDeclaration = builder.GetOrAddDeclaration("Sight");
        orAddDeclaration.Append("GUID", GUID);
        orAddDeclaration.Append("Vision", vision);
        orAddDeclaration.Append("Nightvision_Color", nightvisionColor);
        orAddDeclaration.Append("Nightvision_Fog_Intensity", nightvisionFogIntensity);
        orAddDeclaration.Append("Zoom", zoom);
        orAddDeclaration.Append("ThirdPerson_Zoom", thirdPersonZoomFactor);
        orAddDeclaration.Append("Zoom_Using_Eyes", shouldZoomUsingEyes);
        orAddDeclaration.Append("Holographic", isHolographic);
    }
}
