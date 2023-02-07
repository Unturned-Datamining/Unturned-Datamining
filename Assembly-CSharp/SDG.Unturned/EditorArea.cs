using SDG.Framework.Devkit;
using UnityEngine;

namespace SDG.Unturned;

public class EditorArea : MonoBehaviour
{
    public EditorRegionUpdated onRegionUpdated;

    public EditorBoundUpdated onBoundUpdated;

    private byte _region_x;

    private byte _region_y;

    private byte _bound;

    public static EditorArea instance { get; protected set; }

    public byte region_x => _region_x;

    public byte region_y => _region_y;

    public byte bound => _bound;

    public IAmbianceNode effectNode { get; private set; }

    public static event EditorAreaRegisteredHandler registered;

    protected void triggerRegistered()
    {
        EditorArea.registered?.Invoke(this);
    }

    private void Update()
    {
        if (Regions.tryGetCoordinate(base.transform.position, out var x, out var y) && (x != region_x || y != region_y))
        {
            byte old_x = region_x;
            byte old_y = region_y;
            _region_x = x;
            _region_y = y;
            onRegionUpdated?.Invoke(old_x, old_y, x, y);
        }
        LevelNavigation.tryGetBounds(base.transform.position, out var b);
        if (b != bound)
        {
            byte oldBound = bound;
            _bound = b;
            onBoundUpdated?.Invoke(oldBound, b);
        }
        effectNode = VolumeManager<AmbianceVolume, AmbianceVolumeManager>.Get().GetFirstOverlappingVolume(base.transform.position);
        LevelLighting.updateLocal(MainCamera.instance.transform.position, 0f, effectNode);
    }

    private void Start()
    {
        _region_x = byte.MaxValue;
        _region_y = byte.MaxValue;
        _bound = byte.MaxValue;
        instance = this;
        LevelLighting.updateLighting();
        triggerRegistered();
    }
}
