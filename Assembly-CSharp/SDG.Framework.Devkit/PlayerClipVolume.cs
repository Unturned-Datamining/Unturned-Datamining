using SDG.Framework.IO.FormattedFiles;
using SDG.Unturned;
using UnityEngine;

namespace SDG.Framework.Devkit;

public class PlayerClipVolume : LevelVolume<PlayerClipVolume, PlayerClipVolumeManager>
{
    private class Menu : SleekWrapper
    {
        private PlayerClipVolume volume;

        public Menu(PlayerClipVolume volume)
        {
            this.volume = volume;
            base.sizeOffset_X = 400;
            base.sizeOffset_Y = 40;
            ISleekToggle sleekToggle = Glazier.Get().CreateToggle();
            sleekToggle.sizeOffset_X = 40;
            sleekToggle.sizeOffset_Y = 40;
            sleekToggle.state = volume.blockPlayer;
            sleekToggle.addLabel("Block Player", ESleekSide.RIGHT);
            sleekToggle.onToggled += OnBlockPlayerToggled;
            AddChild(sleekToggle);
        }

        private void OnBlockPlayerToggled(ISleekToggle toggle, bool state)
        {
            volume.blockPlayer = state;
        }
    }

    [SerializeField]
    protected bool _blockPlayer = true;

    public bool blockPlayer
    {
        get
        {
            return _blockPlayer;
        }
        set
        {
            _blockPlayer = value;
            if (!Level.isEditor)
            {
                volumeCollider.enabled = value;
            }
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
        if (reader.containsKey("Block_Player"))
        {
            blockPlayer = reader.readValue<bool>("Block_Player");
        }
        else
        {
            blockPlayer = true;
        }
    }

    protected override void writeHierarchyItem(IFormattedFileWriter writer)
    {
        base.writeHierarchyItem(writer);
        writer.writeValue("Block_Player", blockPlayer);
    }

    protected override void Awake()
    {
        forceShouldAddCollider = true;
        base.Awake();
        volumeCollider.isTrigger = false;
        base.gameObject.layer = 21;
    }
}
