using System.Collections.Generic;
using SDG.Framework.IO.FormattedFiles;
using UnityEngine;

namespace SDG.Unturned;

public class CullingVolume : LevelVolume<CullingVolume, CullingVolumeManager>
{
    private class Menu : SleekWrapper
    {
        private ISleekFloat32Field distanceField;

        private CullingVolume volume;

        public Menu(CullingVolume volume)
        {
            this.volume = volume;
            base.SizeOffset_X = 400f;
            base.SizeOffset_Y = 80f;
            distanceField = Glazier.Get().CreateFloat32Field();
            distanceField.PositionOffset_Y = 0f;
            distanceField.SizeOffset_X = 200f;
            distanceField.SizeOffset_Y = 30f;
            distanceField.Value = volume.cullDistance;
            distanceField.AddLabel("Cull Distance", ESleekSide.RIGHT);
            distanceField.OnValueChanged += OnDistanceChanged;
            AddChild(distanceField);
            ISleekToggle sleekToggle = Glazier.Get().CreateToggle();
            sleekToggle.PositionOffset_Y = 40f;
            sleekToggle.SizeOffset_X = 40f;
            sleekToggle.SizeOffset_Y = 40f;
            sleekToggle.Value = volume.includeLargeObjects;
            sleekToggle.AddLabel("Include Large Objects", ESleekSide.RIGHT);
            sleekToggle.OnValueChanged += OnIncludeLargeObjectsToggled;
            AddChild(sleekToggle);
        }

        private void OnDistanceChanged(ISleekFloat32Field field, float value)
        {
            volume.cullDistance = Mathf.Clamp(value, 1f, ObjectManager.OBJECT_REGIONS * Regions.REGION_SIZE);
            distanceField.Value = volume.cullDistance;
        }

        private void OnIncludeLargeObjectsToggled(ISleekToggle toggle, bool value)
        {
            volume.includeLargeObjects = value;
        }
    }

    internal List<LevelObject> objects;

    internal bool isCulled;

    private static List<MeshFilter> meshFilters = new List<MeshFilter>();

    private static List<OcclusionArea> occlusionAreas = new List<OcclusionArea>();

    [SerializeField]
    internal float cullDistance = 64f;

    [SerializeField]
    private bool includeLargeObjects;

    private int objectUpdateIndex = -1;

    private bool isManagedByLevelObject;

    private LevelObject targetLevelObject;

    private Vector3 positionRelativeToLevelObject;

    private bool shouldUpdate;

    public override bool ShouldSave => !isManagedByLevelObject;

    public override bool CanBeSelected => !isManagedByLevelObject;

    internal bool HasPendingVisibilityUpdates => objectUpdateIndex >= 0;

    public override ISleekElement CreateMenu()
    {
        ISleekElement sleekElement = new Menu(this);
        AppendBaseMenu(sleekElement);
        return sleekElement;
    }

    protected override void readHierarchyItem(IFormattedFileReader reader)
    {
        base.readHierarchyItem(reader);
        cullDistance = reader.readValue<float>("Cull_Distance");
        includeLargeObjects = reader.readValue<bool>("Include_Large_Objects");
    }

    protected override void writeHierarchyItem(IFormattedFileWriter writer)
    {
        base.writeHierarchyItem(writer);
        writer.writeValue("Cull_Distance", cullDistance);
        writer.writeValue("Include_Large_Objects", includeLargeObjects);
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        SetShouldUpdate(objects != null && objects.Count > 0);
    }

    protected override void OnDisable()
    {
        if (objects != null && objects.Count > 0)
        {
            ClearObjects();
            SetShouldUpdate(newShouldUpdate: false);
        }
        base.OnDisable();
    }

    internal void SetupForLevelObject(LevelObject targetLevelObject)
    {
        isManagedByLevelObject = true;
        this.targetLevelObject = targetLevelObject;
        cullDistance = 64f * targetLevelObject.asset.lodBias;
        Transform transform = targetLevelObject.transform;
        EObjectLOD lod = targetLevelObject.asset.lod;
        Vector3 cullingVolumeLocalPositionOffset = targetLevelObject.asset.cullingVolumeLocalPositionOffset;
        Vector3 cullingVolumeSizeOffset = targetLevelObject.asset.cullingVolumeSizeOffset;
        switch (lod)
        {
        case EObjectLOD.MESH:
        {
            meshFilters.Clear();
            targetLevelObject.transform.GetComponentsInChildren(includeInactive: true, meshFilters);
            if (meshFilters.Count == 0)
            {
                return;
            }
            Bounds bounds2 = default(Bounds);
            bool flag2 = false;
            foreach (MeshFilter meshFilter in meshFilters)
            {
                Mesh sharedMesh = meshFilter.sharedMesh;
                if (!(sharedMesh == null))
                {
                    Transform transform3 = meshFilter.transform;
                    Bounds bounds3 = sharedMesh.bounds;
                    Vector3 center = bounds3.center;
                    Vector3 extents = bounds3.extents;
                    Vector3 vector4 = transform.InverseTransformPoint(transform3.TransformPoint(center + extents));
                    if (flag2)
                    {
                        bounds2.Encapsulate(vector4);
                    }
                    else
                    {
                        bounds2 = new Bounds(vector4, Vector3.zero);
                        flag2 = true;
                    }
                    bounds2.Encapsulate(transform.InverseTransformPoint(transform3.TransformPoint(center - extents)));
                    bounds2.Encapsulate(transform.InverseTransformPoint(transform3.TransformPoint(center.x - extents.x, center.y + extents.y, center.z + extents.z)));
                    bounds2.Encapsulate(transform.InverseTransformPoint(transform3.TransformPoint(center.x + extents.x, center.y - extents.y, center.z + extents.z)));
                    bounds2.Encapsulate(transform.InverseTransformPoint(transform3.TransformPoint(center.x + extents.x, center.y + extents.y, center.z - extents.z)));
                    bounds2.Encapsulate(transform.InverseTransformPoint(transform3.TransformPoint(center.x - extents.x, center.y - extents.y, center.z + extents.z)));
                    bounds2.Encapsulate(transform.InverseTransformPoint(transform3.TransformPoint(center.x - extents.x, center.y + extents.y, center.z - extents.z)));
                    bounds2.Encapsulate(transform.InverseTransformPoint(transform3.TransformPoint(center.x + extents.x, center.y - extents.y, center.z - extents.z)));
                }
            }
            bounds2.Expand(-1f);
            bounds2.center += cullingVolumeLocalPositionOffset;
            bounds2.size += cullingVolumeSizeOffset;
            if (bounds2.size.x < 1f || bounds2.size.y < 1f || bounds2.size.z < 1f)
            {
                return;
            }
            positionRelativeToLevelObject = bounds2.center;
            base.transform.localScale = bounds2.size;
            break;
        }
        case EObjectLOD.AREA:
        {
            occlusionAreas.Clear();
            transform.GetComponentsInChildren(includeInactive: true, occlusionAreas);
            if (occlusionAreas.Count == 0)
            {
                return;
            }
            Bounds bounds = default(Bounds);
            bool flag = false;
            foreach (OcclusionArea occlusionArea in occlusionAreas)
            {
                Transform transform2 = occlusionArea.transform;
                Vector3 vector = targetLevelObject.transform.InverseTransformPoint(transform2.TransformPoint(occlusionArea.center));
                Vector3 vector2 = new Vector3(occlusionArea.size.x * 0.5f, occlusionArea.size.z * 0.5f, occlusionArea.size.y * 0.5f);
                Vector3 vector3 = vector + vector2;
                if (flag)
                {
                    bounds.Encapsulate(vector3);
                }
                else
                {
                    bounds = new Bounds(vector3, Vector3.zero);
                    flag = true;
                }
                bounds.Encapsulate(vector - vector2);
                bounds.Encapsulate(new Vector3(vector.x - vector2.x, vector.y + vector2.y, vector.z + vector2.z));
                bounds.Encapsulate(new Vector3(vector.x + vector2.x, vector.y - vector2.y, vector.z + vector2.z));
                bounds.Encapsulate(new Vector3(vector.x + vector2.x, vector.y + vector2.y, vector.z - vector2.z));
                bounds.Encapsulate(new Vector3(vector.x - vector2.x, vector.y - vector2.y, vector.z + vector2.z));
                bounds.Encapsulate(new Vector3(vector.x - vector2.x, vector.y + vector2.y, vector.z - vector2.z));
                bounds.Encapsulate(new Vector3(vector.x + vector2.x, vector.y - vector2.y, vector.z - vector2.z));
            }
            positionRelativeToLevelObject = bounds.center;
            base.transform.localScale = bounds.size;
            break;
        }
        }
        SynchronizeTransformWithLevelObject();
    }

    internal void OnLevelObjectMoved()
    {
        if (objects != null && objects.Count > 0)
        {
            ClearObjects();
            SetShouldUpdate(newShouldUpdate: false);
        }
        SynchronizeTransformWithLevelObject();
    }

    private void SynchronizeTransformWithLevelObject()
    {
        base.transform.position = targetLevelObject.transform.TransformPoint(positionRelativeToLevelObject);
        base.transform.rotation = targetLevelObject.transform.rotation;
    }

    internal void FindObjectsInsideVolume()
    {
        if (objects == null)
        {
            objects = new List<LevelObject>();
        }
        Bounds worldBounds = CalculateWorldBounds();
        RegionBounds regionBounds = new RegionBounds(worldBounds);
        for (int i = regionBounds.min.x; i <= regionBounds.max.x; i++)
        {
            for (int j = regionBounds.min.y; j <= regionBounds.max.y; j++)
            {
                if (!Regions.checkSafe(i, j))
                {
                    continue;
                }
                foreach (LevelObject item in LevelObjects.objects[i, j])
                {
                    if (item != targetLevelObject && item.asset != null && !(item.transform == null) && (item.asset.type != 0 || includeLargeObjects) && !item.asset.shouldExcludeFromCullingVolumes && !item.isSpeciallyCulled && IsPositionInsideVolume(item.transform.position))
                    {
                        item.isSpeciallyCulled = true;
                        item.SetIsVisibleInCullingVolume(!isCulled);
                        objects.Add(item);
                    }
                }
            }
        }
        SetShouldUpdate(base.enabled && objects.Count > 0);
    }

    internal bool UpdateCulling(Vector3 viewPosition, bool forceCull)
    {
        float sqrMagnitude = (base.transform.position - viewPosition).sqrMagnitude;
        if (!forceCull && sqrMagnitude < cullDistance * cullDistance)
        {
            if (isCulled)
            {
                isCulled = false;
                objectUpdateIndex = 0;
                return true;
            }
        }
        else if (!isCulled)
        {
            isCulled = true;
            objectUpdateIndex = 0;
            return true;
        }
        return false;
    }

    internal void UpdateObjectsVisibility()
    {
        if (objectUpdateIndex >= objects.Count)
        {
            objectUpdateIndex = -1;
            return;
        }
        objects[objectUpdateIndex].SetIsVisibleInCullingVolume(!isCulled);
        objectUpdateIndex++;
        if (objectUpdateIndex >= objects.Count)
        {
            objectUpdateIndex = -1;
        }
    }

    internal void SyncAllObjectsVisibility()
    {
        if (objectUpdateIndex < 0)
        {
            return;
        }
        objectUpdateIndex = -1;
        foreach (LevelObject @object in objects)
        {
            @object.SetIsVisibleInCullingVolume(!isCulled);
        }
    }

    internal void ClearObjects()
    {
        foreach (LevelObject @object in objects)
        {
            @object.isSpeciallyCulled = false;
            @object.SetIsVisibleInCullingVolume(isVisible: true);
        }
        objects.Clear();
    }

    private void SetShouldUpdate(bool newShouldUpdate)
    {
        if (shouldUpdate != newShouldUpdate)
        {
            shouldUpdate = newShouldUpdate;
            if (shouldUpdate)
            {
                GetVolumeManager().AddVolumeWithObjects(this);
            }
            else
            {
                GetVolumeManager().RemoveVolumeWithObjects(this);
            }
        }
    }
}
