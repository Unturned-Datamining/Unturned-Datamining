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
            base.sizeOffset_X = 400;
            base.sizeOffset_Y = 160;
            SleekButtonState sleekButtonState = new SleekButtonState(new GUIContent("Additive"), new GUIContent("Subtractive"));
            sleekButtonState.sizeOffset_X = 200;
            sleekButtonState.sizeOffset_Y = 30;
            sleekButtonState.addLabel("Mode", ESleekSide.RIGHT);
            sleekButtonState.state = ((volume.mode != 0) ? 1 : 0);
            sleekButtonState.onSwappedState = (SwappedState)Delegate.Combine(sleekButtonState.onSwappedState, new SwappedState(OnSwappedMode));
            AddChild(sleekButtonState);
            ISleekToggle sleekToggle = Glazier.Get().CreateToggle();
            sleekToggle.positionOffset_Y = 40;
            sleekToggle.sizeOffset_X = 40;
            sleekToggle.sizeOffset_Y = 40;
            sleekToggle.state = volume.instancedMeshes;
            sleekToggle.addLabel("Instanced Meshes", ESleekSide.RIGHT);
            sleekToggle.onToggled += OnInstancedMeshesToggled;
            AddChild(sleekToggle);
            ISleekToggle sleekToggle2 = Glazier.Get().CreateToggle();
            sleekToggle2.positionOffset_Y = 80;
            sleekToggle2.sizeOffset_X = 40;
            sleekToggle2.sizeOffset_Y = 40;
            sleekToggle2.state = volume.resources;
            sleekToggle2.addLabel("Resources", ESleekSide.RIGHT);
            sleekToggle2.onToggled += OnResourcesToggled;
            AddChild(sleekToggle2);
            ISleekToggle sleekToggle3 = Glazier.Get().CreateToggle();
            sleekToggle3.positionOffset_Y = 120;
            sleekToggle3.sizeOffset_X = 40;
            sleekToggle3.sizeOffset_Y = 40;
            sleekToggle3.state = volume.objects;
            sleekToggle3.addLabel("Objects", ESleekSide.RIGHT);
            sleekToggle3.onToggled += OnObjectsToggled;
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
