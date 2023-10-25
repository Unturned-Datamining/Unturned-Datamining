using System.Collections.Generic;
using SDG.Framework.Devkit;
using SDG.Framework.Devkit.Tools;
using SDG.Framework.Devkit.Transactions;
using SDG.Framework.Landscapes;
using SDG.Framework.Rendering;
using SDG.Framework.Utilities;
using UnityEngine;

namespace SDG.Unturned;

public class TerrainEditor : IDevkitTool
{
    public enum EDevkitLandscapeToolMode
    {
        HEIGHTMAP,
        SPLATMAP,
        TILE
    }

    public enum EDevkitLandscapeToolHeightmapMode
    {
        ADJUST,
        FLATTEN,
        SMOOTH,
        RAMP
    }

    public enum EDevkitLandscapeToolSplatmapMode
    {
        PAINT,
        AUTO,
        SMOOTH,
        CUT
    }

    public delegate void DevkitLandscapeToolModeChangedHandler(EDevkitLandscapeToolMode oldMode, EDevkitLandscapeToolMode newMode);

    public delegate void DevkitLandscapeToolSelectedTileChangedHandler(LandscapeTile oldSelectedTile, LandscapeTile newSelectedTile);

    private static readonly RaycastHit[] FOUNDATION_HITS = new RaycastHit[4];

    protected static EDevkitLandscapeToolMode _toolMode;

    protected static LandscapeTile _selectedTile;

    public static EDevkitLandscapeToolHeightmapMode heightmapMode;

    public static EDevkitLandscapeToolSplatmapMode splatmapMode;

    protected static LandscapeMaterialAsset splatmapMaterialTargetAsset;

    protected static AssetReference<LandscapeMaterialAsset> _splatmapMaterialTarget;

    protected int heightmapSmoothSampleCount;

    protected float heightmapSmoothSampleAverage;

    protected float heightmapSmoothTarget;

    protected int splatmapSmoothSampleCount;

    protected Dictionary<AssetReference<LandscapeMaterialAsset>, float> splatmapSmoothSampleAverage = new Dictionary<AssetReference<LandscapeMaterialAsset>, float>();

    protected Vector3 heightmapRampBeginPosition;

    protected Vector3 heightmapRampEndPosition;

    protected Vector3 tilePlanePosition;

    protected Vector3 pointerWorldPosition;

    protected Vector3 brushWorldPosition;

    protected Vector3 changePlanePosition;

    protected Vector3 flattenPlanePosition;

    /// <summary>
    /// Whether the pointer is currently in a spot that can be painted.
    /// </summary>
    protected bool isPointerOnLandscape;

    protected bool isPointerOnTilePlane;

    protected bool isBrushVisible;

    protected bool isTileVisible;

    protected LandscapeCoord pointerTileCoord;

    protected List<LandscapePreviewSample> previewSamples = new List<LandscapePreviewSample>();

    protected bool isChangingBrushRadius;

    protected bool isChangingBrushFalloff;

    protected bool isChangingBrushStrength;

    protected bool isChangingWeightTarget;

    protected bool isSamplingFlattenTarget;

    protected bool isSamplingRampPositions;

    protected bool isSamplingLayer;

    private Dictionary<LandscapeCoord, float[,]> heightmapPixelSmoothBuffer = new Dictionary<LandscapeCoord, float[,]>();

    private Dictionary<LandscapeCoord, float[,,]> splatmapPixelSmoothBuffer = new Dictionary<LandscapeCoord, float[,,]>();

    public static EDevkitLandscapeToolMode toolMode
    {
        get
        {
            return _toolMode;
        }
        set
        {
            if (toolMode != value)
            {
                EDevkitLandscapeToolMode oldMode = toolMode;
                _toolMode = value;
                TerrainEditor.toolModeChanged?.Invoke(oldMode, toolMode);
            }
        }
    }

    public static LandscapeTile selectedTile
    {
        get
        {
            return _selectedTile;
        }
        set
        {
            if (selectedTile != value)
            {
                LandscapeTile oldSelectedTile = selectedTile;
                _selectedTile = value;
                TerrainEditor.selectedTileChanged?.Invoke(oldSelectedTile, selectedTile);
            }
        }
    }

    public virtual float heightmapAdjustSensitivity => DevkitLandscapeToolHeightmapOptions.adjustSensitivity;

    public virtual float heightmapFlattenSensitivity => DevkitLandscapeToolHeightmapOptions.flattenSensitivity;

    public virtual float heightmapBrushRadius
    {
        get
        {
            return DevkitLandscapeToolHeightmapOptions.instance.brushRadius;
        }
        set
        {
            DevkitLandscapeToolHeightmapOptions.instance.brushRadius = value;
        }
    }

    public virtual float heightmapBrushFalloff
    {
        get
        {
            return DevkitLandscapeToolHeightmapOptions.instance.brushFalloff;
        }
        set
        {
            DevkitLandscapeToolHeightmapOptions.instance.brushFalloff = value;
        }
    }

    public virtual float heightmapBrushStrength
    {
        get
        {
            return heightmapMode switch
            {
                EDevkitLandscapeToolHeightmapMode.FLATTEN => DevkitLandscapeToolHeightmapOptions.instance.flattenStrength, 
                EDevkitLandscapeToolHeightmapMode.SMOOTH => DevkitLandscapeToolHeightmapOptions.instance.smoothStrength, 
                _ => DevkitLandscapeToolHeightmapOptions.instance.brushStrength, 
            };
        }
        set
        {
            switch (heightmapMode)
            {
            default:
                DevkitLandscapeToolHeightmapOptions.instance.brushStrength = value;
                break;
            case EDevkitLandscapeToolHeightmapMode.FLATTEN:
                DevkitLandscapeToolHeightmapOptions.instance.flattenStrength = value;
                break;
            case EDevkitLandscapeToolHeightmapMode.SMOOTH:
                DevkitLandscapeToolHeightmapOptions.instance.smoothStrength = value;
                break;
            }
        }
    }

    public virtual float heightmapFlattenTarget
    {
        get
        {
            return DevkitLandscapeToolHeightmapOptions.instance.flattenTarget;
        }
        set
        {
            DevkitLandscapeToolHeightmapOptions.instance.flattenTarget = value;
        }
    }

    public virtual uint heightmapMaxPreviewSamples
    {
        get
        {
            return DevkitLandscapeToolHeightmapOptions.instance.maxPreviewSamples;
        }
        set
        {
            DevkitLandscapeToolHeightmapOptions.instance.maxPreviewSamples = value;
        }
    }

    public virtual float splatmapPaintSensitivity => DevkitLandscapeToolSplatmapOptions.paintSensitivity;

    public virtual float splatmapBrushRadius
    {
        get
        {
            if (splatmapMode == EDevkitLandscapeToolSplatmapMode.CUT)
            {
                return Mathf.Min(32f, DevkitLandscapeToolSplatmapOptions.instance.brushRadius);
            }
            return DevkitLandscapeToolSplatmapOptions.instance.brushRadius;
        }
        set
        {
            DevkitLandscapeToolSplatmapOptions.instance.brushRadius = value;
        }
    }

    public virtual float splatmapBrushFalloff
    {
        get
        {
            return DevkitLandscapeToolSplatmapOptions.instance.brushFalloff;
        }
        set
        {
            DevkitLandscapeToolSplatmapOptions.instance.brushFalloff = value;
        }
    }

    public virtual float splatmapBrushStrength
    {
        get
        {
            return splatmapMode switch
            {
                EDevkitLandscapeToolSplatmapMode.AUTO => DevkitLandscapeToolSplatmapOptions.instance.autoStrength, 
                EDevkitLandscapeToolSplatmapMode.SMOOTH => DevkitLandscapeToolSplatmapOptions.instance.smoothStrength, 
                _ => DevkitLandscapeToolSplatmapOptions.instance.brushStrength, 
            };
        }
        set
        {
            switch (splatmapMode)
            {
            default:
                DevkitLandscapeToolSplatmapOptions.instance.brushStrength = value;
                break;
            case EDevkitLandscapeToolSplatmapMode.AUTO:
                DevkitLandscapeToolSplatmapOptions.instance.autoStrength = value;
                break;
            case EDevkitLandscapeToolSplatmapMode.SMOOTH:
                DevkitLandscapeToolSplatmapOptions.instance.smoothStrength = value;
                break;
            }
        }
    }

    public virtual bool splatmapUseWeightTarget
    {
        get
        {
            return DevkitLandscapeToolSplatmapOptions.instance.useWeightTarget;
        }
        set
        {
            DevkitLandscapeToolSplatmapOptions.instance.useWeightTarget = value;
        }
    }

    public virtual float splatmapWeightTarget
    {
        get
        {
            return DevkitLandscapeToolSplatmapOptions.instance.weightTarget;
        }
        set
        {
            DevkitLandscapeToolSplatmapOptions.instance.weightTarget = value;
        }
    }

    public virtual uint splatmapMaxPreviewSamples
    {
        get
        {
            return DevkitLandscapeToolSplatmapOptions.instance.maxPreviewSamples;
        }
        set
        {
            DevkitLandscapeToolSplatmapOptions.instance.maxPreviewSamples = value;
        }
    }

    public static AssetReference<LandscapeMaterialAsset> splatmapMaterialTarget
    {
        get
        {
            return _splatmapMaterialTarget;
        }
        set
        {
            _splatmapMaterialTarget = value;
            splatmapMaterialTargetAsset = Assets.find(splatmapMaterialTarget);
        }
    }

    protected virtual float brushRadius
    {
        get
        {
            if (toolMode == EDevkitLandscapeToolMode.HEIGHTMAP)
            {
                return heightmapBrushRadius;
            }
            return splatmapBrushRadius;
        }
        set
        {
            if (toolMode == EDevkitLandscapeToolMode.HEIGHTMAP)
            {
                heightmapBrushRadius = value;
            }
            else
            {
                splatmapBrushRadius = value;
            }
        }
    }

    protected virtual float brushFalloff
    {
        get
        {
            if (toolMode == EDevkitLandscapeToolMode.HEIGHTMAP)
            {
                return heightmapBrushFalloff;
            }
            return splatmapBrushFalloff;
        }
        set
        {
            if (toolMode == EDevkitLandscapeToolMode.HEIGHTMAP)
            {
                heightmapBrushFalloff = value;
            }
            else
            {
                splatmapBrushFalloff = value;
            }
        }
    }

    protected virtual float brushStrength
    {
        get
        {
            if (toolMode == EDevkitLandscapeToolMode.HEIGHTMAP)
            {
                return heightmapBrushStrength;
            }
            return splatmapBrushStrength;
        }
        set
        {
            if (toolMode == EDevkitLandscapeToolMode.HEIGHTMAP)
            {
                heightmapBrushStrength = value;
            }
            else
            {
                splatmapBrushStrength = value;
            }
        }
    }

    protected virtual uint maxPreviewSamples
    {
        get
        {
            if (toolMode == EDevkitLandscapeToolMode.HEIGHTMAP)
            {
                return heightmapMaxPreviewSamples;
            }
            return splatmapMaxPreviewSamples;
        }
        set
        {
            if (toolMode == EDevkitLandscapeToolMode.HEIGHTMAP)
            {
                heightmapMaxPreviewSamples = value;
            }
            else
            {
                splatmapMaxPreviewSamples = value;
            }
        }
    }

    protected virtual bool isChangingBrush
    {
        get
        {
            if (!isChangingBrushRadius && !isChangingBrushFalloff && !isChangingBrushStrength)
            {
                return isChangingWeightTarget;
            }
            return true;
        }
    }

    public static event DevkitLandscapeToolModeChangedHandler toolModeChanged;

    public static event DevkitLandscapeToolSelectedTileChangedHandler selectedTileChanged;

    protected virtual void beginChangeHotkeyTransaction()
    {
        DevkitTransactionUtility.beginGenericTransaction();
        DevkitTransactionUtility.recordObjectDelta(DevkitLandscapeToolHeightmapOptions.instance);
        DevkitTransactionUtility.recordObjectDelta(DevkitLandscapeToolSplatmapOptions.instance);
    }

    protected virtual void endChangeHotkeyTransaction()
    {
        DevkitTransactionUtility.endGenericTransaction();
    }

    public virtual void update()
    {
        Ray ray = EditorInteract.ray;
        Plane plane = default(Plane);
        plane.SetNormalAndPosition(Vector3.up, Vector3.zero);
        isPointerOnTilePlane = plane.Raycast(ray, out var enter);
        tilePlanePosition = ray.origin + ray.direction * enter;
        pointerTileCoord = new LandscapeCoord(tilePlanePosition);
        isTileVisible = isPointerOnTilePlane;
        previewSamples.Clear();
        isPointerOnLandscape = Physics.Raycast(ray, out var hitInfo, 8192f, -2146435072);
        pointerWorldPosition = hitInfo.point;
        if (!EditorInteract.isFlying && Glazier.Get().ShouldGameProcessInput)
        {
            if (InputEx.GetKeyDown(KeyCode.B))
            {
                isChangingBrushRadius = true;
                beginChangeHotkeyTransaction();
            }
            if (InputEx.GetKeyDown(KeyCode.F))
            {
                isChangingBrushFalloff = true;
                beginChangeHotkeyTransaction();
            }
            if (InputEx.GetKeyDown(KeyCode.V))
            {
                isChangingBrushStrength = true;
                beginChangeHotkeyTransaction();
            }
            if (InputEx.GetKeyDown(KeyCode.G))
            {
                isChangingWeightTarget = true;
                beginChangeHotkeyTransaction();
            }
        }
        if (InputEx.GetKeyUp(KeyCode.B))
        {
            isChangingBrushRadius = false;
            endChangeHotkeyTransaction();
        }
        if (InputEx.GetKeyUp(KeyCode.F))
        {
            isChangingBrushFalloff = false;
            endChangeHotkeyTransaction();
        }
        if (InputEx.GetKeyUp(KeyCode.V))
        {
            isChangingBrushStrength = false;
            endChangeHotkeyTransaction();
        }
        if (InputEx.GetKeyUp(KeyCode.G))
        {
            isChangingWeightTarget = false;
            endChangeHotkeyTransaction();
        }
        if (isChangingBrush)
        {
            Plane plane2 = default(Plane);
            plane2.SetNormalAndPosition(Vector3.up, brushWorldPosition);
            plane2.Raycast(ray, out var enter2);
            changePlanePosition = ray.origin + ray.direction * enter2;
            if (isChangingBrushRadius)
            {
                brushRadius = (changePlanePosition - brushWorldPosition).magnitude;
            }
            if (isChangingBrushFalloff)
            {
                brushFalloff = Mathf.Clamp01((changePlanePosition - brushWorldPosition).magnitude / brushRadius);
            }
            if (isChangingBrushStrength)
            {
                brushStrength = (changePlanePosition - brushWorldPosition).magnitude / brushRadius;
            }
            if (isChangingWeightTarget)
            {
                splatmapWeightTarget = Mathf.Clamp01((changePlanePosition - brushWorldPosition).magnitude / brushRadius);
            }
        }
        else
        {
            brushWorldPosition = pointerWorldPosition;
            if (toolMode == EDevkitLandscapeToolMode.HEIGHTMAP && heightmapMode == EDevkitLandscapeToolHeightmapMode.FLATTEN)
            {
                Plane plane3 = default(Plane);
                plane3.SetNormalAndPosition(Vector3.up, new Vector3(0f, heightmapFlattenTarget, 0f));
                if (plane3.Raycast(ray, out var enter3))
                {
                    flattenPlanePosition = ray.origin + ray.direction * enter3;
                    brushWorldPosition = flattenPlanePosition;
                    if (!isPointerOnLandscape)
                    {
                        isPointerOnLandscape = Landscape.isPointerInTile(brushWorldPosition);
                    }
                }
                else
                {
                    flattenPlanePosition = new Vector3(brushWorldPosition.x, heightmapFlattenTarget, brushWorldPosition.z);
                }
            }
        }
        isBrushVisible = isPointerOnLandscape || isChangingBrush;
        if (!EditorInteract.isFlying && Glazier.Get().ShouldGameProcessInput)
        {
            if (toolMode == EDevkitLandscapeToolMode.TILE)
            {
                if (InputEx.GetKeyDown(KeyCode.Mouse0))
                {
                    if (isPointerOnTilePlane)
                    {
                        LandscapeTile tile = Landscape.getTile(pointerTileCoord);
                        if (tile == null)
                        {
                            if (enter < 4096f)
                            {
                                LandscapeTile landscapeTile = Landscape.addTile(pointerTileCoord);
                                if (landscapeTile != null)
                                {
                                    landscapeTile.readHeightmaps();
                                    landscapeTile.readSplatmaps();
                                    Landscape.linkNeighbors();
                                    Landscape.reconcileNeighbors(landscapeTile);
                                    Landscape.applyLOD();
                                    LevelHierarchy.MarkDirty();
                                }
                                selectedTile = landscapeTile;
                            }
                            else
                            {
                                selectedTile = null;
                            }
                        }
                        else if (selectedTile != null && selectedTile.coord == pointerTileCoord)
                        {
                            selectedTile = null;
                        }
                        else
                        {
                            selectedTile = tile;
                        }
                    }
                    else
                    {
                        selectedTile = null;
                    }
                }
                if (InputEx.GetKeyDown(KeyCode.Delete) && selectedTile != null)
                {
                    Landscape.removeTile(selectedTile.coord);
                    selectedTile = null;
                    LevelHierarchy.MarkDirty();
                }
            }
            else if (toolMode == EDevkitLandscapeToolMode.HEIGHTMAP)
            {
                if (InputEx.GetKeyDown(KeyCode.Q))
                {
                    heightmapMode = EDevkitLandscapeToolHeightmapMode.ADJUST;
                }
                if (InputEx.GetKeyDown(KeyCode.W))
                {
                    heightmapMode = EDevkitLandscapeToolHeightmapMode.FLATTEN;
                }
                if (InputEx.GetKeyDown(KeyCode.E))
                {
                    heightmapMode = EDevkitLandscapeToolHeightmapMode.SMOOTH;
                }
                if (InputEx.GetKeyDown(KeyCode.R))
                {
                    heightmapMode = EDevkitLandscapeToolHeightmapMode.RAMP;
                }
                if (heightmapMode == EDevkitLandscapeToolHeightmapMode.FLATTEN)
                {
                    if (InputEx.GetKeyDown(KeyCode.LeftAlt))
                    {
                        isSamplingFlattenTarget = true;
                    }
                    if (InputEx.GetKeyUp(KeyCode.Mouse0) && isSamplingFlattenTarget)
                    {
                        if (Physics.Raycast(ray, out var hitInfo2, 8192f))
                        {
                            heightmapFlattenTarget = hitInfo2.point.y;
                        }
                        isSamplingFlattenTarget = false;
                    }
                }
                if (!isSamplingFlattenTarget && isPointerOnLandscape)
                {
                    if (heightmapMode == EDevkitLandscapeToolHeightmapMode.RAMP)
                    {
                        if (InputEx.GetKeyDown(KeyCode.Mouse0))
                        {
                            heightmapRampBeginPosition = pointerWorldPosition;
                            isSamplingRampPositions = true;
                            DevkitTransactionManager.beginTransaction("Heightmap");
                            Landscape.clearHeightmapTransactions();
                        }
                        if (InputEx.GetKeyDown(KeyCode.R))
                        {
                            isSamplingRampPositions = false;
                        }
                        heightmapRampEndPosition = pointerWorldPosition;
                        if (isSamplingRampPositions && new Vector2(heightmapRampBeginPosition.x - heightmapRampEndPosition.x, heightmapRampBeginPosition.z - heightmapRampEndPosition.z).magnitude > 1f)
                        {
                            Vector3 vector = new Vector3(Mathf.Min(heightmapRampBeginPosition.x, heightmapRampEndPosition.x), Mathf.Min(heightmapRampBeginPosition.y, heightmapRampEndPosition.y), Mathf.Min(heightmapRampBeginPosition.z, heightmapRampEndPosition.z));
                            Vector3 vector2 = new Vector3(Mathf.Max(heightmapRampBeginPosition.x, heightmapRampEndPosition.x), Mathf.Max(heightmapRampBeginPosition.y, heightmapRampEndPosition.y), Mathf.Max(heightmapRampBeginPosition.z, heightmapRampEndPosition.z));
                            vector.x -= heightmapBrushRadius;
                            vector.z -= heightmapBrushRadius;
                            vector2.x += heightmapBrushRadius;
                            vector2.z += heightmapBrushRadius;
                            Landscape.getHeightmapVertices(new Bounds((vector + vector2) / 2f, vector2 - vector), handleHeightmapGetVerticesRamp);
                        }
                    }
                    else
                    {
                        if (InputEx.GetKeyDown(KeyCode.Mouse0))
                        {
                            DevkitTransactionManager.beginTransaction("Heightmap");
                            Landscape.clearHeightmapTransactions();
                        }
                        Bounds bounds = new Bounds(brushWorldPosition, new Vector3(heightmapBrushRadius * 2f, 0f, heightmapBrushRadius * 2f));
                        Landscape.getHeightmapVertices(bounds, handleHeightmapGetVerticesBrush);
                        if (InputEx.GetKey(KeyCode.Mouse0))
                        {
                            if (heightmapMode == EDevkitLandscapeToolHeightmapMode.ADJUST)
                            {
                                Landscape.writeHeightmap(bounds, handleHeightmapWriteAdjust);
                            }
                            else if (heightmapMode == EDevkitLandscapeToolHeightmapMode.FLATTEN)
                            {
                                bounds.center = flattenPlanePosition;
                                Landscape.writeHeightmap(bounds, handleHeightmapWriteFlatten);
                            }
                            else if (heightmapMode == EDevkitLandscapeToolHeightmapMode.SMOOTH)
                            {
                                if (DevkitLandscapeToolHeightmapOptions.instance.smoothMethod == EDevkitLandscapeToolHeightmapSmoothMethod.BRUSH_AVERAGE)
                                {
                                    heightmapSmoothSampleCount = 0;
                                    heightmapSmoothSampleAverage = 0f;
                                    Landscape.readHeightmap(bounds, HandleHeightmapReadBrushAverage);
                                    if (heightmapSmoothSampleCount > 0)
                                    {
                                        heightmapSmoothTarget = heightmapSmoothSampleAverage / (float)heightmapSmoothSampleCount;
                                    }
                                    else
                                    {
                                        heightmapSmoothTarget = 0f;
                                    }
                                }
                                else if (DevkitLandscapeToolHeightmapOptions.instance.smoothMethod == EDevkitLandscapeToolHeightmapSmoothMethod.PIXEL_AVERAGE)
                                {
                                    Bounds worldBounds = bounds;
                                    worldBounds.Expand(Landscape.HEIGHTMAP_WORLD_UNIT * 2f);
                                    Landscape.readHeightmap(worldBounds, HandleHeightmapReadPixelSmooth);
                                }
                                Landscape.writeHeightmap(bounds, handleHeightmapWriteSmooth);
                                if (DevkitLandscapeToolHeightmapOptions.instance.smoothMethod == EDevkitLandscapeToolHeightmapSmoothMethod.PIXEL_AVERAGE)
                                {
                                    ReleaseHeightmapPixelSmoothBuffer();
                                }
                            }
                        }
                    }
                }
            }
            else if (toolMode == EDevkitLandscapeToolMode.SPLATMAP)
            {
                if (InputEx.GetKeyDown(KeyCode.Q))
                {
                    splatmapMode = EDevkitLandscapeToolSplatmapMode.PAINT;
                }
                if (InputEx.GetKeyDown(KeyCode.W))
                {
                    splatmapMode = EDevkitLandscapeToolSplatmapMode.AUTO;
                }
                if (InputEx.GetKeyDown(KeyCode.E))
                {
                    splatmapMode = EDevkitLandscapeToolSplatmapMode.SMOOTH;
                }
                if (InputEx.GetKeyDown(KeyCode.R))
                {
                    splatmapMode = EDevkitLandscapeToolSplatmapMode.CUT;
                }
                if (InputEx.GetKeyDown(KeyCode.LeftAlt))
                {
                    isSamplingLayer = true;
                }
                if (InputEx.GetKeyUp(KeyCode.Mouse0) && isSamplingLayer)
                {
                    if (isPointerOnLandscape && Landscape.getSplatmapMaterial(hitInfo.point, out var materialAsset))
                    {
                        splatmapMaterialTarget = materialAsset;
                    }
                    isSamplingLayer = false;
                }
                if (!isSamplingLayer && isPointerOnLandscape)
                {
                    if (InputEx.GetKeyDown(KeyCode.Mouse0))
                    {
                        DevkitTransactionManager.beginTransaction("Splatmap");
                        Landscape.clearSplatmapTransactions();
                        Landscape.clearHoleTransactions();
                    }
                    Bounds bounds2 = new Bounds(brushWorldPosition, new Vector3(splatmapBrushRadius * 2f, 0f, splatmapBrushRadius * 2f));
                    if (DevkitLandscapeToolSplatmapOptions.instance.previewMethod == EDevkitLandscapeToolSplatmapPreviewMethod.BRUSH_ALPHA)
                    {
                        Landscape.getSplatmapVertices(bounds2, handleSplatmapGetVerticesBrush);
                    }
                    else if (DevkitLandscapeToolSplatmapOptions.instance.previewMethod == EDevkitLandscapeToolSplatmapPreviewMethod.WEIGHT)
                    {
                        Landscape.readSplatmap(bounds2, handleSplatmapReadWeights);
                    }
                    if (InputEx.GetKey(KeyCode.Mouse0))
                    {
                        if (splatmapMode == EDevkitLandscapeToolSplatmapMode.PAINT)
                        {
                            Landscape.writeSplatmap(bounds2, handleSplatmapWritePaint);
                        }
                        else if (splatmapMode == EDevkitLandscapeToolSplatmapMode.AUTO)
                        {
                            Landscape.writeSplatmap(bounds2, handleSplatmapWriteAuto);
                        }
                        else if (splatmapMode == EDevkitLandscapeToolSplatmapMode.SMOOTH)
                        {
                            if (DevkitLandscapeToolSplatmapOptions.instance.smoothMethod == EDevkitLandscapeToolSplatmapSmoothMethod.BRUSH_AVERAGE)
                            {
                                splatmapSmoothSampleCount = 0;
                                splatmapSmoothSampleAverage.Clear();
                                Landscape.readSplatmap(bounds2, handleSplatmapReadBrushAverage);
                            }
                            else if (DevkitLandscapeToolSplatmapOptions.instance.smoothMethod == EDevkitLandscapeToolSplatmapSmoothMethod.PIXEL_AVERAGE)
                            {
                                Bounds worldBounds2 = bounds2;
                                worldBounds2.Expand(Landscape.SPLATMAP_WORLD_UNIT * 2f);
                                Landscape.readSplatmap(worldBounds2, HandleSplatmapReadPixelSmooth);
                            }
                            Landscape.writeSplatmap(bounds2, handleSplatmapWriteSmooth);
                            if (DevkitLandscapeToolSplatmapOptions.instance.smoothMethod == EDevkitLandscapeToolSplatmapSmoothMethod.PIXEL_AVERAGE)
                            {
                                ReleaseSplatmapPixelSmoothBuffer();
                            }
                        }
                        else if (splatmapMode == EDevkitLandscapeToolSplatmapMode.CUT)
                        {
                            Landscape.writeHoles(bounds2, handleSplatmapWriteCut);
                        }
                    }
                }
            }
        }
        if (InputEx.GetKeyUp(KeyCode.LeftAlt))
        {
            if (isSamplingFlattenTarget)
            {
                isSamplingFlattenTarget = false;
            }
            if (isSamplingLayer)
            {
                isSamplingLayer = false;
            }
        }
        if (!InputEx.GetKeyUp(KeyCode.Mouse0))
        {
            return;
        }
        if (isSamplingRampPositions)
        {
            if (isPointerOnLandscape && new Vector2(heightmapRampBeginPosition.x - heightmapRampEndPosition.x, heightmapRampBeginPosition.z - heightmapRampEndPosition.z).magnitude > 1f)
            {
                Vector3 vector3 = new Vector3(Mathf.Min(heightmapRampBeginPosition.x, heightmapRampEndPosition.x), Mathf.Min(heightmapRampBeginPosition.y, heightmapRampEndPosition.y), Mathf.Min(heightmapRampBeginPosition.z, heightmapRampEndPosition.z));
                Vector3 vector4 = new Vector3(Mathf.Max(heightmapRampBeginPosition.x, heightmapRampEndPosition.x), Mathf.Max(heightmapRampBeginPosition.y, heightmapRampEndPosition.y), Mathf.Max(heightmapRampBeginPosition.z, heightmapRampEndPosition.z));
                vector3.x -= heightmapBrushRadius;
                vector3.z -= heightmapBrushRadius;
                vector4.x += heightmapBrushRadius;
                vector4.z += heightmapBrushRadius;
                Landscape.writeHeightmap(new Bounds((vector3 + vector4) / 2f, vector4 - vector3), handleHeightmapWriteRamp);
            }
            isSamplingRampPositions = false;
        }
        DevkitTransactionManager.endTransaction();
        if (toolMode == EDevkitLandscapeToolMode.HEIGHTMAP)
        {
            Landscape.applyLOD();
        }
    }

    public virtual void equip()
    {
        GLRenderer.render += handleGLRender;
        Landscape.DisableHoleColliders = true;
    }

    public virtual void dequip()
    {
        GLRenderer.render -= handleGLRender;
        Landscape.DisableHoleColliders = false;
    }

    /// <summary>
    /// Get brush strength multiplier where strength decreases past falloff. Use this method so that different falloffs e.g. linear, curved can be added.
    /// </summary>
    /// <param name="normalizedDistance">Percentage of <see cref="P:SDG.Unturned.TerrainEditor.brushRadius" />.</param>
    protected float getBrushAlpha(float normalizedDistance)
    {
        if (normalizedDistance <= brushFalloff || brushFalloff >= 1f)
        {
            return 1f;
        }
        return (1f - normalizedDistance) / (1f - brushFalloff);
    }

    protected void HandleHeightmapReadBrushAverage(LandscapeCoord tileCoord, HeightmapCoord heightmapCoord, Vector3 worldPosition, float currentHeight)
    {
        if (!(new Vector2(worldPosition.x - brushWorldPosition.x, worldPosition.z - brushWorldPosition.z).magnitude / heightmapBrushRadius > 1f))
        {
            heightmapSmoothSampleCount++;
            heightmapSmoothSampleAverage += currentHeight;
        }
    }

    protected void HandleHeightmapReadPixelSmooth(LandscapeCoord tileCoord, HeightmapCoord heightmapCoord, Vector3 worldPosition, float currentHeight)
    {
        if (!heightmapPixelSmoothBuffer.TryGetValue(tileCoord, out var value))
        {
            value = LandscapeHeightmapCopyPool.claim();
            heightmapPixelSmoothBuffer.Add(tileCoord, value);
        }
        value[heightmapCoord.x, heightmapCoord.y] = currentHeight;
    }

    private void ReleaseHeightmapPixelSmoothBuffer()
    {
        foreach (KeyValuePair<LandscapeCoord, float[,]> item in heightmapPixelSmoothBuffer)
        {
            LandscapeHeightmapCopyPool.release(item.Value);
        }
        heightmapPixelSmoothBuffer.Clear();
    }

    protected void HandleSplatmapReadPixelSmooth(LandscapeCoord tileCoord, SplatmapCoord splatmapCoord, Vector3 worldPosition, float[] currentWeights)
    {
        if (!splatmapPixelSmoothBuffer.TryGetValue(tileCoord, out var value))
        {
            value = LandscapeSplatmapCopyPool.claim();
            splatmapPixelSmoothBuffer.Add(tileCoord, value);
        }
        for (int i = 0; i < Landscape.SPLATMAP_LAYERS; i++)
        {
            value[splatmapCoord.x, splatmapCoord.y, i] = currentWeights[i];
        }
    }

    private void ReleaseSplatmapPixelSmoothBuffer()
    {
        foreach (KeyValuePair<LandscapeCoord, float[,,]> item in splatmapPixelSmoothBuffer)
        {
            LandscapeSplatmapCopyPool.release(item.Value);
        }
        splatmapPixelSmoothBuffer.Clear();
    }

    protected void handleHeightmapGetVerticesBrush(LandscapeCoord tileCoord, HeightmapCoord heightmapCoord, Vector3 worldPosition)
    {
        float num = new Vector2(worldPosition.x - brushWorldPosition.x, worldPosition.z - brushWorldPosition.z).magnitude / heightmapBrushRadius;
        if (!(num > 1f))
        {
            float brushAlpha = getBrushAlpha(num);
            previewSamples.Add(new LandscapePreviewSample(worldPosition, brushAlpha));
        }
    }

    protected void handleHeightmapGetVerticesRamp(LandscapeCoord tileCoord, HeightmapCoord heightmapCoord, Vector3 worldPosition)
    {
        Vector2 vector = new Vector2(heightmapRampEndPosition.x - heightmapRampBeginPosition.x, heightmapRampEndPosition.z - heightmapRampBeginPosition.z);
        float magnitude = vector.magnitude;
        Vector2 vector2 = vector / magnitude;
        Vector2 rhs = vector2.Cross();
        Vector2 vector3 = new Vector2(worldPosition.x - heightmapRampBeginPosition.x, worldPosition.z - heightmapRampBeginPosition.z);
        float magnitude2 = vector3.magnitude;
        Vector2 lhs = vector3 / magnitude2;
        float num = Vector2.Dot(lhs, vector2);
        if (!(num < 0f) && !(magnitude2 * num / magnitude > 1f))
        {
            float num2 = Vector2.Dot(lhs, rhs);
            float num3 = Mathf.Abs(magnitude2 * num2 / heightmapBrushRadius);
            if (!(num3 > 1f))
            {
                float brushAlpha = getBrushAlpha(num3);
                previewSamples.Add(new LandscapePreviewSample(worldPosition, brushAlpha));
            }
        }
    }

    protected void handleSplatmapGetVerticesBrush(LandscapeCoord tileCoord, SplatmapCoord splatmapCoord, Vector3 worldPosition)
    {
        float num = new Vector2(worldPosition.x - brushWorldPosition.x, worldPosition.z - brushWorldPosition.z).magnitude / splatmapBrushRadius;
        if (!(num > 1f))
        {
            float brushAlpha = getBrushAlpha(num);
            previewSamples.Add(new LandscapePreviewSample(worldPosition, brushAlpha));
        }
    }

    protected float handleHeightmapWriteAdjust(LandscapeCoord tileCoord, HeightmapCoord heightmapCoord, Vector3 worldPosition, float currentHeight)
    {
        float num = new Vector2(worldPosition.x - brushWorldPosition.x, worldPosition.z - brushWorldPosition.z).magnitude / heightmapBrushRadius;
        if (num > 1f)
        {
            return currentHeight;
        }
        float brushAlpha = getBrushAlpha(num);
        float num2 = Time.deltaTime * heightmapBrushStrength * brushAlpha;
        num2 *= heightmapAdjustSensitivity;
        if (InputEx.GetKey(KeyCode.LeftShift))
        {
            num2 = 0f - num2;
        }
        currentHeight += num2;
        return currentHeight;
    }

    protected float handleHeightmapWriteFlatten(LandscapeCoord tileCoord, HeightmapCoord heightmapCoord, Vector3 worldPosition, float currentHeight)
    {
        float num = new Vector2(worldPosition.x - brushWorldPosition.x, worldPosition.z - brushWorldPosition.z).magnitude / heightmapBrushRadius;
        if (num > 1f)
        {
            return currentHeight;
        }
        float brushAlpha = getBrushAlpha(num);
        float num2 = (heightmapFlattenTarget + Landscape.TILE_HEIGHT / 2f) / Landscape.TILE_HEIGHT;
        switch (DevkitLandscapeToolHeightmapOptions.instance.flattenMethod)
        {
        case EDevkitLandscapeToolHeightmapFlattenMethod.MIN:
            num2 = Mathf.Min(num2, currentHeight);
            break;
        case EDevkitLandscapeToolHeightmapFlattenMethod.MAX:
            num2 = Mathf.Max(num2, currentHeight);
            break;
        }
        float value = num2 - currentHeight;
        float num3 = Time.deltaTime * heightmapBrushStrength * brushAlpha;
        value = Mathf.Clamp(value, 0f - num3, num3);
        value *= heightmapFlattenSensitivity;
        currentHeight += value;
        return currentHeight;
    }

    private void SampleHeightPixelSmooth(Vector3 worldPosition, ref int sampleCount, ref float sampleAverage)
    {
        LandscapeCoord landscapeCoord = new LandscapeCoord(worldPosition);
        if (heightmapPixelSmoothBuffer.TryGetValue(landscapeCoord, out var value))
        {
            HeightmapCoord heightmapCoord = new HeightmapCoord(landscapeCoord, worldPosition);
            sampleCount++;
            sampleAverage += value[heightmapCoord.x, heightmapCoord.y];
        }
    }

    protected float handleHeightmapWriteSmooth(LandscapeCoord tileCoord, HeightmapCoord heightmapCoord, Vector3 worldPosition, float currentHeight)
    {
        float num = new Vector2(worldPosition.x - brushWorldPosition.x, worldPosition.z - brushWorldPosition.z).magnitude / heightmapBrushRadius;
        if (num > 1f)
        {
            return currentHeight;
        }
        float brushAlpha = getBrushAlpha(num);
        if (DevkitLandscapeToolHeightmapOptions.instance.smoothMethod == EDevkitLandscapeToolHeightmapSmoothMethod.PIXEL_AVERAGE)
        {
            heightmapSmoothSampleCount = 0;
            heightmapSmoothSampleAverage = 0f;
            SampleHeightPixelSmooth(worldPosition + new Vector3(Landscape.HEIGHTMAP_WORLD_UNIT, 0f, 0f), ref heightmapSmoothSampleCount, ref heightmapSmoothSampleAverage);
            SampleHeightPixelSmooth(worldPosition + new Vector3(0f - Landscape.HEIGHTMAP_WORLD_UNIT, 0f, 0f), ref heightmapSmoothSampleCount, ref heightmapSmoothSampleAverage);
            SampleHeightPixelSmooth(worldPosition + new Vector3(0f, 0f, Landscape.HEIGHTMAP_WORLD_UNIT), ref heightmapSmoothSampleCount, ref heightmapSmoothSampleAverage);
            SampleHeightPixelSmooth(worldPosition + new Vector3(0f, 0f, 0f - Landscape.HEIGHTMAP_WORLD_UNIT), ref heightmapSmoothSampleCount, ref heightmapSmoothSampleAverage);
            if (heightmapSmoothSampleCount > 0)
            {
                heightmapSmoothTarget = heightmapSmoothSampleAverage / (float)heightmapSmoothSampleCount;
            }
            else
            {
                heightmapSmoothTarget = currentHeight;
            }
        }
        currentHeight = Mathf.Lerp(currentHeight, heightmapSmoothTarget, Time.deltaTime * heightmapBrushStrength * brushAlpha);
        return currentHeight;
    }

    protected float handleHeightmapWriteRamp(LandscapeCoord tileCoord, HeightmapCoord heightmapCoord, Vector3 worldPosition, float currentHeight)
    {
        Vector2 vector = new Vector2(heightmapRampEndPosition.x - heightmapRampBeginPosition.x, heightmapRampEndPosition.z - heightmapRampBeginPosition.z);
        float magnitude = vector.magnitude;
        Vector2 vector2 = vector / magnitude;
        Vector2 rhs = vector2.Cross();
        Vector2 vector3 = new Vector2(worldPosition.x - heightmapRampBeginPosition.x, worldPosition.z - heightmapRampBeginPosition.z);
        float magnitude2 = vector3.magnitude;
        Vector2 lhs = vector3 / magnitude2;
        float num = Vector2.Dot(lhs, vector2);
        if (num < 0f)
        {
            return currentHeight;
        }
        float num2 = magnitude2 * num / magnitude;
        if (num2 > 1f)
        {
            return currentHeight;
        }
        float num3 = Vector2.Dot(lhs, rhs);
        float num4 = Mathf.Abs(magnitude2 * num3 / heightmapBrushRadius);
        if (num4 > 1f)
        {
            return currentHeight;
        }
        float brushAlpha = getBrushAlpha(num4);
        float a = (heightmapRampBeginPosition.y + Landscape.TILE_HEIGHT / 2f) / Landscape.TILE_HEIGHT;
        float b = (heightmapRampEndPosition.y + Landscape.TILE_HEIGHT / 2f) / Landscape.TILE_HEIGHT;
        currentHeight = Mathf.Lerp(currentHeight, Mathf.Lerp(a, b, num2), brushAlpha);
        return Mathf.Clamp01(currentHeight);
    }

    protected void handleSplatmapReadBrushAverage(LandscapeCoord tileCoord, SplatmapCoord splatmapCoord, Vector3 worldPosition, float[] currentWeights)
    {
        LandscapeTile tile = Landscape.getTile(tileCoord);
        if (tile.materials == null || new Vector2(worldPosition.x - brushWorldPosition.x, worldPosition.z - brushWorldPosition.z).magnitude / splatmapBrushRadius > 1f)
        {
            return;
        }
        for (int i = 0; i < Landscape.SPLATMAP_LAYERS; i++)
        {
            AssetReference<LandscapeMaterialAsset> key = tile.materials[i];
            if (key.isValid)
            {
                if (!splatmapSmoothSampleAverage.ContainsKey(key))
                {
                    splatmapSmoothSampleAverage.Add(key, 0f);
                }
                splatmapSmoothSampleAverage[key] += currentWeights[i];
                splatmapSmoothSampleCount++;
            }
        }
    }

    protected void handleSplatmapReadWeights(LandscapeCoord tileCoord, SplatmapCoord splatmapCoord, Vector3 worldPosition, float[] currentWeights)
    {
        LandscapeTile tile = Landscape.getTile(tileCoord);
        if (tile.materials != null && !(new Vector2(worldPosition.x - brushWorldPosition.x, worldPosition.z - brushWorldPosition.z).magnitude / splatmapBrushRadius > 1f))
        {
            int splatmapTargetMaterialLayerIndex = getSplatmapTargetMaterialLayerIndex(tile, splatmapMaterialTarget);
            float newWeight = ((splatmapTargetMaterialLayerIndex != -1) ? currentWeights[splatmapTargetMaterialLayerIndex] : 0f);
            previewSamples.Add(new LandscapePreviewSample(worldPosition, newWeight));
        }
    }

    protected int getSplatmapTargetMaterialLayerIndex(LandscapeTile tile, AssetReference<LandscapeMaterialAsset> targetMaterial)
    {
        if (!targetMaterial.isValid)
        {
            return -1;
        }
        int num = -1;
        for (int i = 0; i < Landscape.SPLATMAP_LAYERS; i++)
        {
            if (tile.materials[i] == targetMaterial)
            {
                num = i;
                break;
            }
        }
        if (num == -1)
        {
            for (int j = 0; j < Landscape.SPLATMAP_LAYERS; j++)
            {
                if (!tile.materials[j].isValid)
                {
                    tile.materials[j] = targetMaterial;
                    tile.updatePrototypes();
                    num = j;
                    break;
                }
            }
        }
        return num;
    }

    protected void blendSplatmapWeights(float[] currentWeights, int targetMaterialLayer, float targetWeight, float speed)
    {
        int splatmapHighestWeightLayerIndex = Landscape.getSplatmapHighestWeightLayerIndex(currentWeights, targetMaterialLayer);
        for (int i = 0; i < Landscape.SPLATMAP_LAYERS; i++)
        {
            float num = ((i != targetMaterialLayer) ? ((i != splatmapHighestWeightLayerIndex) ? 0f : (1f - targetWeight)) : targetWeight);
            float num2 = num - currentWeights[i];
            num2 *= speed;
            currentWeights[i] += num2;
        }
    }

    protected void handleSplatmapWritePaint(LandscapeCoord tileCoord, SplatmapCoord splatmapCoord, Vector3 worldPosition, float[] currentWeights)
    {
        LandscapeTile tile = Landscape.getTile(tileCoord);
        if (tile.materials == null)
        {
            return;
        }
        int splatmapTargetMaterialLayerIndex = getSplatmapTargetMaterialLayerIndex(tile, splatmapMaterialTarget);
        if (splatmapTargetMaterialLayerIndex == -1)
        {
            return;
        }
        float num = new Vector2(worldPosition.x - brushWorldPosition.x, worldPosition.z - brushWorldPosition.z).magnitude / splatmapBrushRadius;
        if (num > 1f)
        {
            return;
        }
        bool flag = InputEx.GetKey(KeyCode.LeftControl) || splatmapUseWeightTarget;
        float targetWeight = 0.5f;
        if (!DevkitLandscapeToolSplatmapOptions.instance.useAutoFoundation && !DevkitLandscapeToolSplatmapOptions.instance.useAutoSlope)
        {
            targetWeight = (flag ? splatmapWeightTarget : ((!InputEx.GetKey(KeyCode.LeftShift)) ? 1f : 0f));
        }
        else
        {
            bool flag2 = false;
            if (DevkitLandscapeToolSplatmapOptions.instance.useAutoFoundation)
            {
                int num2 = Physics.SphereCastNonAlloc(worldPosition + new Vector3(0f, splatmapMaterialTargetAsset.autoRayLength, 0f), DevkitLandscapeToolSplatmapOptions.instance.autoRayRadius, Vector3.down, FOUNDATION_HITS, DevkitLandscapeToolSplatmapOptions.instance.autoRayLength, (int)DevkitLandscapeToolSplatmapOptions.instance.autoRayMask, QueryTriggerInteraction.Ignore);
                if (num2 > 0)
                {
                    bool flag3 = false;
                    for (int i = 0; i < num2; i++)
                    {
                        RaycastHit raycastHit = FOUNDATION_HITS[i];
                        ObjectAsset asset = LevelObjects.getAsset(raycastHit.transform);
                        if (asset == null)
                        {
                            flag3 = true;
                            break;
                        }
                        if (!asset.isSnowshoe)
                        {
                            flag3 = true;
                            break;
                        }
                    }
                    if (flag3)
                    {
                        targetWeight = (flag ? splatmapWeightTarget : 1f);
                        flag2 = true;
                    }
                }
            }
            if (!flag2 && DevkitLandscapeToolSplatmapOptions.instance.useAutoSlope && Landscape.getNormal(worldPosition, out var normal))
            {
                float num3 = Vector3.Angle(Vector3.up, normal);
                if (num3 >= DevkitLandscapeToolSplatmapOptions.instance.autoMinAngleBegin && num3 <= DevkitLandscapeToolSplatmapOptions.instance.autoMaxAngleEnd)
                {
                    targetWeight = ((num3 < DevkitLandscapeToolSplatmapOptions.instance.autoMinAngleEnd) ? Mathf.InverseLerp(DevkitLandscapeToolSplatmapOptions.instance.autoMinAngleBegin, DevkitLandscapeToolSplatmapOptions.instance.autoMinAngleEnd, num3) : ((!(num3 > DevkitLandscapeToolSplatmapOptions.instance.autoMaxAngleBegin)) ? 1f : (1f - Mathf.InverseLerp(DevkitLandscapeToolSplatmapOptions.instance.autoMaxAngleBegin, DevkitLandscapeToolSplatmapOptions.instance.autoMaxAngleEnd, num3))));
                    flag2 = true;
                }
            }
            if (!flag2)
            {
                return;
            }
        }
        float brushAlpha = getBrushAlpha(num);
        float speed = Time.deltaTime * splatmapBrushStrength * brushAlpha * splatmapPaintSensitivity;
        blendSplatmapWeights(currentWeights, splatmapTargetMaterialLayerIndex, targetWeight, speed);
    }

    protected void handleSplatmapWriteAuto(LandscapeCoord tileCoord, SplatmapCoord splatmapCoord, Vector3 worldPosition, float[] currentWeights)
    {
        if (splatmapMaterialTargetAsset == null)
        {
            return;
        }
        LandscapeTile tile = Landscape.getTile(tileCoord);
        if (tile.materials == null)
        {
            return;
        }
        int splatmapTargetMaterialLayerIndex = getSplatmapTargetMaterialLayerIndex(tile, splatmapMaterialTarget);
        if (splatmapTargetMaterialLayerIndex == -1)
        {
            return;
        }
        float num = new Vector2(worldPosition.x - brushWorldPosition.x, worldPosition.z - brushWorldPosition.z).magnitude / splatmapBrushRadius;
        if (num > 1f)
        {
            return;
        }
        float targetWeight = 1f;
        bool flag = false;
        if (splatmapMaterialTargetAsset.useAutoFoundation)
        {
            int num2 = Physics.SphereCastNonAlloc(worldPosition + new Vector3(0f, splatmapMaterialTargetAsset.autoRayLength, 0f), splatmapMaterialTargetAsset.autoRayRadius, Vector3.down, FOUNDATION_HITS, splatmapMaterialTargetAsset.autoRayLength, (int)splatmapMaterialTargetAsset.autoRayMask, QueryTriggerInteraction.Ignore);
            if (num2 > 0)
            {
                bool flag2 = false;
                for (int i = 0; i < num2; i++)
                {
                    RaycastHit raycastHit = FOUNDATION_HITS[i];
                    ObjectAsset asset = LevelObjects.getAsset(raycastHit.transform);
                    if (asset == null)
                    {
                        flag2 = true;
                        break;
                    }
                    if (!asset.isSnowshoe)
                    {
                        flag2 = true;
                        break;
                    }
                }
                if (flag2)
                {
                    targetWeight = 1f;
                    flag = true;
                }
            }
        }
        if (!flag && splatmapMaterialTargetAsset.useAutoSlope && Landscape.getNormal(worldPosition, out var normal))
        {
            float num3 = Vector3.Angle(Vector3.up, normal);
            if (num3 >= splatmapMaterialTargetAsset.autoMinAngleBegin && num3 <= splatmapMaterialTargetAsset.autoMaxAngleEnd)
            {
                if (num3 < splatmapMaterialTargetAsset.autoMinAngleEnd)
                {
                    targetWeight = Mathf.InverseLerp(splatmapMaterialTargetAsset.autoMinAngleBegin, splatmapMaterialTargetAsset.autoMinAngleEnd, num3);
                }
                else if (num3 > splatmapMaterialTargetAsset.autoMaxAngleBegin)
                {
                    targetWeight = 1f - Mathf.InverseLerp(splatmapMaterialTargetAsset.autoMaxAngleBegin, splatmapMaterialTargetAsset.autoMaxAngleEnd, num3);
                }
                flag = true;
            }
        }
        if (flag)
        {
            float brushAlpha = getBrushAlpha(num);
            float speed = Time.deltaTime * splatmapBrushStrength * brushAlpha * splatmapPaintSensitivity;
            blendSplatmapWeights(currentWeights, splatmapTargetMaterialLayerIndex, targetWeight, speed);
        }
    }

    private void SampleSplatmapPixelSmooth(LandscapeCoord tileCoord, SplatmapCoord splatmapCoord)
    {
        if (!splatmapPixelSmoothBuffer.TryGetValue(tileCoord, out var value))
        {
            return;
        }
        LandscapeTile tile = Landscape.getTile(tileCoord);
        if (tile == null || tile.materials == null)
        {
            return;
        }
        for (int i = 0; i < Landscape.SPLATMAP_LAYERS; i++)
        {
            AssetReference<LandscapeMaterialAsset> key = tile.materials[i];
            if (key.isValid)
            {
                if (!splatmapSmoothSampleAverage.ContainsKey(key))
                {
                    splatmapSmoothSampleAverage.Add(key, 0f);
                }
                splatmapSmoothSampleAverage[key] += value[splatmapCoord.x, splatmapCoord.y, i];
                splatmapSmoothSampleCount++;
            }
        }
    }

    protected void handleSplatmapWriteSmooth(LandscapeCoord tileCoord, SplatmapCoord splatmapCoord, Vector3 worldPosition, float[] currentWeights)
    {
        float num = new Vector2(worldPosition.x - brushWorldPosition.x, worldPosition.z - brushWorldPosition.z).magnitude / splatmapBrushRadius;
        if (num > 1f)
        {
            return;
        }
        if (DevkitLandscapeToolSplatmapOptions.instance.smoothMethod == EDevkitLandscapeToolSplatmapSmoothMethod.PIXEL_AVERAGE)
        {
            splatmapSmoothSampleCount = 0;
            splatmapSmoothSampleAverage.Clear();
            LandscapeCoord tileCoord2 = tileCoord;
            SplatmapCoord splatmapCoord2 = new SplatmapCoord(splatmapCoord.x, splatmapCoord.y - 1);
            LandscapeUtility.cleanSplatmapCoord(ref tileCoord2, ref splatmapCoord2);
            SampleSplatmapPixelSmooth(tileCoord2, splatmapCoord2);
            tileCoord2 = tileCoord;
            splatmapCoord2 = new SplatmapCoord(splatmapCoord.x + 1, splatmapCoord.y);
            LandscapeUtility.cleanSplatmapCoord(ref tileCoord2, ref splatmapCoord2);
            SampleSplatmapPixelSmooth(tileCoord2, splatmapCoord2);
            tileCoord2 = tileCoord;
            splatmapCoord2 = new SplatmapCoord(splatmapCoord.x, splatmapCoord.y + 1);
            LandscapeUtility.cleanSplatmapCoord(ref tileCoord2, ref splatmapCoord2);
            SampleSplatmapPixelSmooth(tileCoord2, splatmapCoord2);
            tileCoord2 = tileCoord;
            splatmapCoord2 = new SplatmapCoord(splatmapCoord.x - 1, splatmapCoord.y);
            LandscapeUtility.cleanSplatmapCoord(ref tileCoord2, ref splatmapCoord2);
            SampleSplatmapPixelSmooth(tileCoord2, splatmapCoord2);
        }
        if (splatmapSmoothSampleCount <= 0)
        {
            return;
        }
        LandscapeTile tile = Landscape.getTile(tileCoord);
        if (tile.materials == null)
        {
            return;
        }
        float brushAlpha = getBrushAlpha(num);
        float num2 = Time.deltaTime * splatmapBrushStrength * brushAlpha;
        float num3 = 0f;
        for (int i = 0; i < Landscape.SPLATMAP_LAYERS; i++)
        {
            if (splatmapSmoothSampleAverage.ContainsKey(tile.materials[i]))
            {
                num3 += splatmapSmoothSampleAverage[tile.materials[i]] / (float)splatmapSmoothSampleCount;
            }
        }
        num3 = 1f / num3;
        for (int j = 0; j < Landscape.SPLATMAP_LAYERS; j++)
        {
            float num4 = ((!splatmapSmoothSampleAverage.ContainsKey(tile.materials[j])) ? 0f : (splatmapSmoothSampleAverage[tile.materials[j]] / (float)splatmapSmoothSampleCount * num3));
            float num5 = num4 - currentWeights[j];
            num5 *= num2;
            currentWeights[j] += num5;
        }
    }

    protected bool handleSplatmapWriteCut(Vector3 worldPosition, bool currentlyVisible)
    {
        if (new Vector2(worldPosition.x - brushWorldPosition.x, worldPosition.z - brushWorldPosition.z).magnitude / splatmapBrushRadius > 1f)
        {
            return currentlyVisible;
        }
        return InputEx.GetKey(KeyCode.LeftShift);
    }

    protected void handleGLCircleOffset(ref Vector3 position)
    {
        Landscape.getWorldHeight(position, out position.y);
    }

    protected void handleGLRender()
    {
        GLUtility.matrix = MathUtility.IDENTITY_MATRIX;
        if (toolMode == EDevkitLandscapeToolMode.TILE)
        {
            GLUtility.LINE_FLAT_COLOR.SetPass(0);
            GL.Begin(1);
            if (selectedTile != null && selectedTile.coord != pointerTileCoord)
            {
                GL.Color(Color.yellow);
                GLUtility.line(new Vector3((float)selectedTile.coord.x * Landscape.TILE_SIZE, 0f, (float)selectedTile.coord.y * Landscape.TILE_SIZE), new Vector3((float)(selectedTile.coord.x + 1) * Landscape.TILE_SIZE, 0f, (float)selectedTile.coord.y * Landscape.TILE_SIZE));
                GLUtility.line(new Vector3((float)selectedTile.coord.x * Landscape.TILE_SIZE, 0f, (float)selectedTile.coord.y * Landscape.TILE_SIZE), new Vector3((float)selectedTile.coord.x * Landscape.TILE_SIZE, 0f, (float)(selectedTile.coord.y + 1) * Landscape.TILE_SIZE));
                GLUtility.line(new Vector3((float)(selectedTile.coord.x + 1) * Landscape.TILE_SIZE, 0f, (float)(selectedTile.coord.y + 1) * Landscape.TILE_SIZE), new Vector3((float)(selectedTile.coord.x + 1) * Landscape.TILE_SIZE, 0f, (float)selectedTile.coord.y * Landscape.TILE_SIZE));
                GLUtility.line(new Vector3((float)(selectedTile.coord.x + 1) * Landscape.TILE_SIZE, 0f, (float)(selectedTile.coord.y + 1) * Landscape.TILE_SIZE), new Vector3((float)selectedTile.coord.x * Landscape.TILE_SIZE, 0f, (float)(selectedTile.coord.y + 1) * Landscape.TILE_SIZE));
            }
            if (isTileVisible && Glazier.Get().ShouldGameProcessInput)
            {
                GL.Color((Landscape.getTile(pointerTileCoord) == null) ? Color.green : ((selectedTile != null && selectedTile.coord == pointerTileCoord) ? Color.red : Color.white));
                GLUtility.line(new Vector3((float)pointerTileCoord.x * Landscape.TILE_SIZE, 0f, (float)pointerTileCoord.y * Landscape.TILE_SIZE), new Vector3((float)(pointerTileCoord.x + 1) * Landscape.TILE_SIZE, 0f, (float)pointerTileCoord.y * Landscape.TILE_SIZE));
                GLUtility.line(new Vector3((float)pointerTileCoord.x * Landscape.TILE_SIZE, 0f, (float)pointerTileCoord.y * Landscape.TILE_SIZE), new Vector3((float)pointerTileCoord.x * Landscape.TILE_SIZE, 0f, (float)(pointerTileCoord.y + 1) * Landscape.TILE_SIZE));
                GLUtility.line(new Vector3((float)(pointerTileCoord.x + 1) * Landscape.TILE_SIZE, 0f, (float)(pointerTileCoord.y + 1) * Landscape.TILE_SIZE), new Vector3((float)(pointerTileCoord.x + 1) * Landscape.TILE_SIZE, 0f, (float)pointerTileCoord.y * Landscape.TILE_SIZE));
                GLUtility.line(new Vector3((float)(pointerTileCoord.x + 1) * Landscape.TILE_SIZE, 0f, (float)(pointerTileCoord.y + 1) * Landscape.TILE_SIZE), new Vector3((float)pointerTileCoord.x * Landscape.TILE_SIZE, 0f, (float)(pointerTileCoord.y + 1) * Landscape.TILE_SIZE));
            }
            GL.End();
            return;
        }
        if (!isBrushVisible || !Glazier.Get().ShouldGameProcessInput)
        {
            return;
        }
        if (previewSamples.Count <= maxPreviewSamples)
        {
            GLUtility.LINE_FLAT_COLOR.SetPass(0);
            GL.Begin(4);
            float num = Mathf.Lerp(0.1f, 1f, brushRadius / 256f);
            Vector3 size = new Vector3(num, num, num);
            foreach (LandscapePreviewSample previewSample in previewSamples)
            {
                GL.Color(Color.Lerp(Color.red, Color.green, previewSample.weight));
                GLUtility.boxSolid(previewSample.position, size);
            }
            GL.End();
        }
        GLUtility.LINE_FLAT_COLOR.SetPass(0);
        GL.Begin(1);
        if (toolMode == EDevkitLandscapeToolMode.HEIGHTMAP && heightmapMode == EDevkitLandscapeToolHeightmapMode.RAMP)
        {
            if (isSamplingRampPositions)
            {
                Vector3 normalized = (heightmapRampEndPosition - heightmapRampBeginPosition).normalized;
                Vector3 vector = Vector3.Cross(Vector3.up, normalized);
                GL.Color(new Color(0.5f, 0.5f, 0f, 0.5f));
                GLUtility.line(heightmapRampBeginPosition - vector * brushRadius, heightmapRampEndPosition - vector * brushRadius);
                GLUtility.line(heightmapRampBeginPosition + vector * brushRadius, heightmapRampEndPosition + vector * brushRadius);
                GL.Color(Color.yellow);
                GLUtility.line(heightmapRampBeginPosition - vector * brushRadius * heightmapBrushFalloff, heightmapRampEndPosition - vector * brushRadius * heightmapBrushFalloff);
                GLUtility.line(heightmapRampBeginPosition + vector * brushRadius * heightmapBrushFalloff, heightmapRampEndPosition + vector * brushRadius * heightmapBrushFalloff);
            }
            else if (isChangingBrushRadius || isChangingBrushFalloff)
            {
                Vector3 normalized2 = (pointerWorldPosition - brushWorldPosition).normalized;
                Vector3 vector2 = Vector3.Cross(Vector3.up, normalized2);
                GL.Color(new Color(0.5f, 0.5f, 0f, 0.5f));
                GLUtility.line(brushWorldPosition - normalized2 * brushRadius - vector2, brushWorldPosition - normalized2 * brushRadius + vector2);
                GLUtility.line(brushWorldPosition + normalized2 * brushRadius - vector2, brushWorldPosition + normalized2 * brushRadius + vector2);
                GL.Color(Color.yellow);
                GLUtility.line(brushWorldPosition - normalized2 * brushRadius * heightmapBrushFalloff - vector2, brushWorldPosition - normalized2 * brushRadius * heightmapBrushFalloff + vector2);
                GLUtility.line(brushWorldPosition + normalized2 * brushRadius * heightmapBrushFalloff - vector2, brushWorldPosition + normalized2 * brushRadius * heightmapBrushFalloff + vector2);
            }
            goto IL_09c4;
        }
        Color color = (isChangingBrushStrength ? Color.Lerp(Color.red, Color.green, brushStrength) : ((!isChangingWeightTarget) ? Color.yellow : Color.Lerp(Color.red, Color.green, splatmapWeightTarget)));
        int num2;
        Color c;
        if (toolMode == EDevkitLandscapeToolMode.SPLATMAP)
        {
            num2 = ((splatmapMode != EDevkitLandscapeToolSplatmapMode.CUT) ? 1 : 0);
            if (num2 == 0)
            {
                c = color;
                goto IL_0881;
            }
        }
        else
        {
            num2 = 1;
        }
        c = color / 2f;
        goto IL_0881;
        IL_09c4:
        GL.End();
        return;
        IL_0881:
        GL.Color(c);
        GLUtility.circle(brushWorldPosition, brushRadius, new Vector3(1f, 0f, 0f), new Vector3(0f, 0f, 1f), handleGLCircleOffset);
        if (toolMode == EDevkitLandscapeToolMode.HEIGHTMAP && heightmapMode == EDevkitLandscapeToolHeightmapMode.FLATTEN)
        {
            GLUtility.circle(flattenPlanePosition, brushRadius, new Vector3(1f, 0f, 0f), new Vector3(0f, 0f, 1f));
        }
        if (num2 != 0)
        {
            GL.Color(color);
            GLUtility.circle(brushWorldPosition, brushRadius * brushFalloff, new Vector3(1f, 0f, 0f), new Vector3(0f, 0f, 1f), handleGLCircleOffset);
            if (toolMode == EDevkitLandscapeToolMode.HEIGHTMAP && heightmapMode == EDevkitLandscapeToolHeightmapMode.FLATTEN)
            {
                GLUtility.circle(flattenPlanePosition, brushRadius * brushFalloff, new Vector3(1f, 0f, 0f), new Vector3(0f, 0f, 1f));
            }
        }
        goto IL_09c4;
    }
}
