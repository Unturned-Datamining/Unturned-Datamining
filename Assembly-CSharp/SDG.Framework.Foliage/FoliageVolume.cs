using System;
using SDG.Framework.IO.FormattedFiles;
using SDG.Unturned;
using UnityEngine;

namespace SDG.Framework.Foliage;

public class FoliageVolume : LevelVolume<FoliageVolume, FoliageVolumeManager>
{
    public enum EFoliageVolumeMode
    {
        ADDITIVE,
        SUBTRACTIVE
    }

    private class Menu : SleekWrapper
    {
        private FoliageVolume volume;

        public Menu(FoliageVolume volume)
        {
            this.volume = volume;
            base.SizeOffset_X = 400f;
            base.SizeOffset_Y = 160f;
            SleekButtonState sleekButtonState = new SleekButtonState(new GUIContent("Additive"), new GUIContent("Subtractive"));
            sleekButtonState.SizeOffset_X = 200f;
            sleekButtonState.SizeOffset_Y = 30f;
            sleekButtonState.AddLabel("Mode", ESleekSide.RIGHT);
            sleekButtonState.state = ((volume.mode != 0) ? 1 : 0);
            sleekButtonState.onSwappedState = (SwappedState)Delegate.Combine(sleekButtonState.onSwappedState, new SwappedState(OnSwappedMode));
            AddChild(sleekButtonState);
            ISleekToggle sleekToggle = Glazier.Get().CreateToggle();
            sleekToggle.PositionOffset_Y = 40f;
            sleekToggle.SizeOffset_X = 40f;
            sleekToggle.SizeOffset_Y = 40f;
            sleekToggle.Value = volume.instancedMeshes;
            sleekToggle.AddLabel("Instanced Meshes", ESleekSide.RIGHT);
            sleekToggle.OnValueChanged += OnInstancedMeshesToggled;
            AddChild(sleekToggle);
            ISleekToggle sleekToggle2 = Glazier.Get().CreateToggle();
            sleekToggle2.PositionOffset_Y = 80f;
            sleekToggle2.SizeOffset_X = 40f;
            sleekToggle2.SizeOffset_Y = 40f;
            sleekToggle2.Value = volume.resources;
            sleekToggle2.AddLabel("Resources", ESleekSide.RIGHT);
            sleekToggle2.OnValueChanged += OnResourcesToggled;
            AddChild(sleekToggle2);
            ISleekToggle sleekToggle3 = Glazier.Get().CreateToggle();
            sleekToggle3.PositionOffset_Y = 120f;
            sleekToggle3.SizeOffset_X = 40f;
            sleekToggle3.SizeOffset_Y = 40f;
            sleekToggle3.Value = volume.objects;
            sleekToggle3.AddLabel("Objects", ESleekSide.RIGHT);
            sleekToggle3.OnValueChanged += OnObjectsToggled;
            AddChild(sleekToggle3);
        }

        private void OnSwappedMode(SleekButtonState button, int state)
        {
            volume.mode = ((state != 0) ? EFoliageVolumeMode.SUBTRACTIVE : EFoliageVolumeMode.ADDITIVE);
        }

        private void OnInstancedMeshesToggled(ISleekToggle toggle, bool state)
        {
            volume.instancedMeshes = state;
        }

        private void OnResourcesToggled(ISleekToggle toggle, bool state)
        {
            volume.resources = state;
        }

        private void OnObjectsToggled(ISleekToggle toggle, bool state)
        {
            volume.objects = state;
        }
    }

    [SerializeField]
    protected EFoliageVolumeMode _mode = EFoliageVolumeMode.SUBTRACTIVE;

    public bool instancedMeshes = true;

    public bool resources = true;

    public bool objects = true;

    public EFoliageVolumeMode mode
    {
        get
        {
            return _mode;
        }
        set
        {
            if (!base.enabled)
            {
                _mode = value;
                return;
            }
            GetVolumeManager().RemoveVolume(this);
            _mode = value;
            GetVolumeManager().AddVolume(this);
        }
    }

    public override ISleekElement CreateMenu()
    {
        ISleekElement sleekElement = new Menu(this);
        AppendBaseMenu(sleekElement);
        return sleekElement;
    }

    protected override void readHierarchyItem(IFormattedFileReader reader)
    {
        base.readHierarchyItem(reader);
        mode = reader.readValue<EFoliageVolumeMode>("Mode");
        if (reader.containsKey("Instanced_Meshes"))
        {
            instancedMeshes = reader.readValue<bool>("Instanced_Meshes");
        }
        if (reader.containsKey("Resources"))
        {
            resources = reader.readValue<bool>("Resources");
        }
        if (reader.containsKey("Objects"))
        {
            objects = reader.readValue<bool>("Objects");
        }
    }

    protected override void writeHierarchyItem(IFormattedFileWriter writer)
    {
        base.writeHierarchyItem(writer);
        writer.writeValue("Mode", mode);
        writer.writeValue("Instanced_Meshes", instancedMeshes);
        writer.writeValue("Resources", resources);
        writer.writeValue("Objects", objects);
    }
}
