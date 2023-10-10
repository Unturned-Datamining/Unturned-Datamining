using System;
using System.Collections.Generic;
using UnityEngine;

namespace SDG.Unturned;

public class SleekScopeOverlay : SleekWrapper
{
    private class DistanceMarker
    {
        public bool isEnabled;

        public bool isVisible;

        public float distance;

        public ISleekImage horizontalLine;

        public ISleekLabel distanceLabel;

        public bool hasLabel;

        public void SetIsVisible(bool isVisible)
        {
            if (this.isVisible != isVisible)
            {
                this.isVisible = isVisible;
                SyncIsVisible();
            }
        }

        public void SyncIsVisible()
        {
            horizontalLine.IsVisible = isVisible;
            distanceLabel.IsVisible = isVisible && hasLabel;
        }
    }

    private ISleekConstraintFrame scopeFrame;

    public ISleekImage scopeImage;

    private ISleekImage scopeOverlay;

    private ISleekImage scopeLeftOverlay;

    private ISleekImage scopeRightOverlay;

    private ISleekImage scopeUpOverlay;

    private ISleekImage scopeDownOverlay;

    private ItemSightAsset currentSightAsset;

    private List<DistanceMarker> distanceMarkers = new List<DistanceMarker>();

    internal static float CalcAngle(float speed, float distance, float gravity)
    {
        return Mathf.Asin(distance * gravity / (speed * speed)) * 0.5f;
    }

    public override void OnUpdate()
    {
        base.OnUpdate();
        UseableGun useableGun = Player.player.equipment.useable as UseableGun;
        if (useableGun == null || useableGun.equippedGunAsset.projectile != null || useableGun.firstAttachments == null || useableGun.firstAttachments.sightAsset == null || useableGun.firstAttachments.sightAsset.distanceMarkers == null || useableGun.firstAttachments.sightAsset.distanceMarkers.Count < 1)
        {
            DisableDistanceMarkers();
            return;
        }
        if (Player.player == null || Player.player.look == null || Player.player.look.mainCameraZoomFactor <= 0f)
        {
            DisableDistanceMarkers();
            return;
        }
        float num = OptionsSettings.GetZoomBaseFieldOfView() / Player.player.look.mainCameraZoomFactor;
        float num2 = MathF.PI / 180f * num;
        if (num2 < 0.001f)
        {
            DisableDistanceMarkers();
            return;
        }
        if (currentSightAsset != useableGun.firstAttachments.sightAsset)
        {
            EnableDistanceMarkersForSight(useableGun.firstAttachments.sightAsset);
        }
        float muzzleVelocity = useableGun.equippedGunAsset.muzzleVelocity;
        float gravity = useableGun.CalculateBulletGravity();
        foreach (DistanceMarker distanceMarker in distanceMarkers)
        {
            if (!distanceMarker.isEnabled)
            {
                break;
            }
            float num3 = Mathf.Abs(CalcAngle(muzzleVelocity, distanceMarker.distance, gravity)) / num2;
            distanceMarker.horizontalLine.PositionScale_Y = 0.5f + num3;
            distanceMarker.distanceLabel.PositionScale_Y = distanceMarker.horizontalLine.PositionScale_Y;
            distanceMarker.SetIsVisible(num3 > 0.01f && num3 < 0.5f);
        }
    }

    public override void OnDestroy()
    {
        base.OnDestroy();
        OptionsSettings.OnUnitSystemChanged -= SyncMarkerLabels;
    }

    public SleekScopeOverlay()
    {
        scopeFrame = Glazier.Get().CreateConstraintFrame();
        scopeFrame.SizeScale_X = 1f;
        scopeFrame.SizeScale_Y = 1f;
        scopeFrame.Constraint = ESleekConstraint.FitInParent;
        AddChild(scopeFrame);
        scopeOverlay = Glazier.Get().CreateImage((Texture2D)Resources.Load("Overlay/Scope"));
        scopeOverlay.PositionScale_X = 0.1f;
        scopeOverlay.PositionScale_Y = 0.1f;
        scopeOverlay.SizeScale_X = 0.8f;
        scopeOverlay.SizeScale_Y = 0.8f;
        scopeFrame.AddChild(scopeOverlay);
        scopeLeftOverlay = Glazier.Get().CreateImage((Texture2D)GlazierResources.PixelTexture);
        scopeLeftOverlay.PositionOffset_X = 1f;
        scopeLeftOverlay.PositionScale_X = -10f;
        scopeLeftOverlay.SizeScale_X = 10f;
        scopeLeftOverlay.SizeScale_Y = 1f;
        scopeLeftOverlay.TintColor = Color.black;
        scopeOverlay.AddChild(scopeLeftOverlay);
        scopeRightOverlay = Glazier.Get().CreateImage((Texture2D)GlazierResources.PixelTexture);
        scopeRightOverlay.PositionOffset_X = -1f;
        scopeRightOverlay.PositionScale_X = 1f;
        scopeRightOverlay.SizeScale_X = 10f;
        scopeRightOverlay.SizeScale_Y = 1f;
        scopeRightOverlay.TintColor = Color.black;
        scopeOverlay.AddChild(scopeRightOverlay);
        scopeUpOverlay = Glazier.Get().CreateImage((Texture2D)GlazierResources.PixelTexture);
        scopeUpOverlay.PositionOffset_Y = 1f;
        scopeUpOverlay.PositionScale_X = -10f;
        scopeUpOverlay.PositionScale_Y = -10f;
        scopeUpOverlay.SizeScale_X = 21f;
        scopeUpOverlay.SizeScale_Y = 10f;
        scopeUpOverlay.TintColor = Color.black;
        scopeOverlay.AddChild(scopeUpOverlay);
        scopeDownOverlay = Glazier.Get().CreateImage((Texture2D)GlazierResources.PixelTexture);
        scopeDownOverlay.PositionOffset_Y = -1f;
        scopeDownOverlay.PositionScale_X = -10f;
        scopeDownOverlay.PositionScale_Y = 1f;
        scopeDownOverlay.SizeScale_X = 21f;
        scopeDownOverlay.SizeScale_Y = 10f;
        scopeDownOverlay.TintColor = Color.black;
        scopeOverlay.AddChild(scopeDownOverlay);
        scopeImage = Glazier.Get().CreateImage();
        scopeImage.SizeScale_X = 1f;
        scopeImage.SizeScale_Y = 1f;
        scopeOverlay.AddChild(scopeImage);
        OptionsSettings.OnUnitSystemChanged += SyncMarkerLabels;
    }

    private void SyncMarkerLabels()
    {
        foreach (DistanceMarker distanceMarker in distanceMarkers)
        {
            if (!distanceMarker.isEnabled)
            {
                break;
            }
            if (OptionsSettings.metric)
            {
                distanceMarker.distanceLabel.Text = $"{distanceMarker.distance} m";
            }
            else
            {
                distanceMarker.distanceLabel.Text = $"{Mathf.RoundToInt(MeasurementTool.MtoYd(distanceMarker.distance))} yd";
            }
        }
    }

    private void DisableDistanceMarkers()
    {
        currentSightAsset = null;
        for (int i = 0; i < distanceMarkers.Count; i++)
        {
            DistanceMarker distanceMarker = distanceMarkers[i];
            if (distanceMarker.isEnabled)
            {
                distanceMarker.isEnabled = false;
                distanceMarker.SetIsVisible(isVisible: false);
            }
        }
    }

    private void EnableDistanceMarkersForSight(ItemSightAsset newSightAsset)
    {
        currentSightAsset = newSightAsset;
        for (int i = 0; i < currentSightAsset.distanceMarkers.Count; i++)
        {
            ItemSightAsset.DistanceMarker distanceMarker = currentSightAsset.distanceMarkers[i];
            DistanceMarker distanceMarker2;
            if (i < distanceMarkers.Count)
            {
                distanceMarker2 = distanceMarkers[i];
            }
            else
            {
                distanceMarker2 = new DistanceMarker();
                distanceMarker2.isVisible = true;
                distanceMarker2.horizontalLine = Glazier.Get().CreateImage((Texture2D)GlazierResources.PixelTexture);
                distanceMarker2.horizontalLine.SizeOffset_Y = 1f;
                scopeFrame.AddChild(distanceMarker2.horizontalLine);
                distanceMarker2.distanceLabel = Glazier.Get().CreateLabel();
                distanceMarker2.distanceLabel.PositionOffset_Y = -25f;
                distanceMarker2.distanceLabel.SizeOffset_X = 200f;
                distanceMarker2.distanceLabel.SizeOffset_Y = 50f;
                distanceMarker2.distanceLabel.FontStyle = FontStyle.Bold;
                scopeFrame.AddChild(distanceMarker2.distanceLabel);
                distanceMarkers.Add(distanceMarker2);
            }
            distanceMarker2.horizontalLine.SizeScale_X = distanceMarker.lineWidth;
            if (distanceMarker.side == ItemSightAsset.DistanceMarker.ESide.Right)
            {
                distanceMarker2.horizontalLine.PositionScale_X = 0.5f + distanceMarker.lineOffset;
                distanceMarker2.distanceLabel.PositionScale_X = 0.5f + distanceMarker.lineOffset + distanceMarker.lineWidth;
                distanceMarker2.distanceLabel.PositionOffset_X = 0f;
                distanceMarker2.distanceLabel.TextAlignment = TextAnchor.MiddleLeft;
            }
            else
            {
                distanceMarker2.horizontalLine.PositionScale_X = 0.5f - distanceMarker.lineOffset - distanceMarker.lineWidth;
                distanceMarker2.distanceLabel.PositionScale_X = 0.5f - distanceMarker.lineOffset - distanceMarker.lineWidth;
                distanceMarker2.distanceLabel.PositionOffset_X = 0f - distanceMarker2.distanceLabel.SizeOffset_X;
                distanceMarker2.distanceLabel.TextAlignment = TextAnchor.MiddleRight;
            }
            distanceMarker2.distance = distanceMarker.distance;
            distanceMarker2.isEnabled = true;
            distanceMarker2.hasLabel = distanceMarker.hasLabel;
            distanceMarker2.horizontalLine.TintColor = distanceMarker.color;
            distanceMarker2.distanceLabel.TextColor = distanceMarker.color;
            distanceMarker2.SyncIsVisible();
        }
        for (int j = currentSightAsset.distanceMarkers.Count; j < distanceMarkers.Count; j++)
        {
            DistanceMarker distanceMarker3 = distanceMarkers[j];
            if (distanceMarker3.isEnabled)
            {
                distanceMarker3.isEnabled = false;
                distanceMarker3.SetIsVisible(isVisible: false);
            }
        }
        SyncMarkerLabels();
    }
}
