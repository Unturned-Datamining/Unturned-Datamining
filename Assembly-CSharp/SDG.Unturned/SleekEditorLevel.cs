using SDG.Provider;
using Steamworks;
using UnityEngine;

namespace SDG.Unturned;

/// <summary>
/// Button in the list of levels for the map editor.
/// </summary>
public class SleekEditorLevel : SleekLevel
{
    public SleekEditorLevel(LevelInfo level)
        : base(level)
    {
        if (!level.isEditable)
        {
            button.OnClicked -= base.onClickedButton;
            Bundle bundle = Bundles.getBundle("/Bundles/Textures/Menu/Icons/Workshop/MenuWorkshopEditor/MenuWorkshopEditor.unity3d");
            ISleekImage sleekImage = Glazier.Get().CreateImage();
            sleekImage.PositionOffset_X = 20f;
            sleekImage.PositionOffset_Y = -20f;
            sleekImage.PositionScale_Y = 0.5f;
            sleekImage.SizeOffset_X = 40f;
            sleekImage.SizeOffset_Y = 40f;
            sleekImage.Texture = bundle.load<Texture2D>("Lock");
            sleekImage.TintColor = ESleekTint.FOREGROUND;
            button.AddChild(sleekImage);
            bundle.unload();
        }
        if (level.isFromWorkshop)
        {
            if (TempSteamworksWorkshop.getCachedDetails(new PublishedFileId_t(level.publishedFileId), out var cachedDetails))
            {
                button.TooltipText = cachedDetails.GetTitle();
            }
            else
            {
                button.TooltipText = level.publishedFileId.ToString();
            }
        }
        else
        {
            button.TooltipText = level.path;
        }
    }
}
