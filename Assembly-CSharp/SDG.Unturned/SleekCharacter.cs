using SDG.SteamworksProvider.Services.Store;
using UnityEngine;

namespace SDG.Unturned;

public class SleekCharacter : SleekWrapper
{
    public ClickedCharacter onClickedCharacter;

    private byte index;

    private ISleekButton button;

    private ISleekLabel nameLabel;

    private ISleekLabel nickLabel;

    private ISleekLabel skillsetLabel;

    public void updateCharacter(Character character)
    {
        nameLabel.Text = MenuSurvivorsCharacterUI.localization.format("Name_Label", character.name);
        nickLabel.Text = MenuSurvivorsCharacterUI.localization.format("Nick_Label", character.nick);
        skillsetLabel.Text = MenuSurvivorsCharacterUI.localization.format("Skillset_" + (byte)character.skillset);
    }

    private void onClickedButton(ISleekElement button)
    {
        if (!Provider.isPro && index >= Customization.FREE_CHARACTERS)
        {
            Provider.provider.storeService.open(new SteamworksStorePackageID(Provider.PRO_ID.m_AppId));
        }
        else
        {
            onClickedCharacter?.Invoke(this, index);
        }
    }

    public SleekCharacter(byte newIndex)
    {
        index = newIndex;
        button = Glazier.Get().CreateButton();
        button.SizeScale_X = 1f;
        button.SizeScale_Y = 1f;
        button.OnClicked += onClickedButton;
        AddChild(button);
        nameLabel = Glazier.Get().CreateLabel();
        nameLabel.PositionOffset_X = 5f;
        nameLabel.PositionOffset_Y = 5f;
        nameLabel.SizeOffset_X = -10f;
        nameLabel.SizeOffset_Y = -10f;
        nameLabel.SizeScale_X = 1f;
        nameLabel.SizeScale_Y = 1f;
        nameLabel.TextAlignment = TextAnchor.UpperCenter;
        button.AddChild(nameLabel);
        nickLabel = Glazier.Get().CreateLabel();
        nickLabel.PositionOffset_X = 5f;
        nickLabel.PositionOffset_Y = 5f;
        nickLabel.SizeOffset_X = -10f;
        nickLabel.SizeOffset_Y = -10f;
        nickLabel.SizeScale_X = 1f;
        nickLabel.SizeScale_Y = 1f;
        nickLabel.TextAlignment = TextAnchor.MiddleCenter;
        button.AddChild(nickLabel);
        skillsetLabel = Glazier.Get().CreateLabel();
        skillsetLabel.PositionOffset_X = 5f;
        skillsetLabel.PositionOffset_Y = 5f;
        skillsetLabel.SizeOffset_X = -10f;
        skillsetLabel.SizeOffset_Y = -10f;
        skillsetLabel.SizeScale_X = 1f;
        skillsetLabel.SizeScale_Y = 1f;
        skillsetLabel.TextAlignment = TextAnchor.LowerCenter;
        button.AddChild(skillsetLabel);
        if (!Provider.isPro && index >= Customization.FREE_CHARACTERS)
        {
            Bundle bundle = Bundles.getBundle("/Bundles/Textures/Menu/Icons/Pro/Pro.unity3d");
            ISleekImage sleekImage = Glazier.Get().CreateImage();
            sleekImage.PositionOffset_X = -20f;
            sleekImage.PositionOffset_Y = -20f;
            sleekImage.PositionScale_X = 0.5f;
            sleekImage.PositionScale_Y = 0.5f;
            sleekImage.SizeOffset_X = 40f;
            sleekImage.SizeOffset_Y = 40f;
            sleekImage.Texture = bundle.load<Texture2D>("Lock_Medium");
            button.AddChild(sleekImage);
            bundle.unload();
        }
        updateCharacter(Characters.list[index]);
    }
}
