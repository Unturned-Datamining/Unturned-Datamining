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
        nameLabel.text = MenuSurvivorsCharacterUI.localization.format("Name_Label", character.name);
        nickLabel.text = MenuSurvivorsCharacterUI.localization.format("Nick_Label", character.nick);
        skillsetLabel.text = MenuSurvivorsCharacterUI.localization.format("Skillset_" + (byte)character.skillset);
    }

    private void onClickedButton(ISleekElement button)
    {
        if (!Provider.isPro && index >= Customization.FREE_CHARACTERS)
        {
            Provider.provider.storeService.open(new SteamworksStorePackageID(Provider.PRO_ID.m_AppId));
        }
        else if (onClickedCharacter != null)
        {
            onClickedCharacter(this, index);
        }
    }

    public SleekCharacter(byte newIndex)
    {
        index = newIndex;
        button = Glazier.Get().CreateButton();
        button.sizeScale_X = 1f;
        button.sizeScale_Y = 1f;
        button.onClickedButton += onClickedButton;
        AddChild(button);
        nameLabel = Glazier.Get().CreateLabel();
        nameLabel.positionOffset_X = 5;
        nameLabel.positionOffset_Y = 5;
        nameLabel.sizeOffset_X = -10;
        nameLabel.sizeOffset_Y = -10;
        nameLabel.sizeScale_X = 1f;
        nameLabel.sizeScale_Y = 1f;
        nameLabel.fontAlignment = TextAnchor.UpperCenter;
        button.AddChild(nameLabel);
        nickLabel = Glazier.Get().CreateLabel();
        nickLabel.positionOffset_X = 5;
        nickLabel.positionOffset_Y = 5;
        nickLabel.sizeOffset_X = -10;
        nickLabel.sizeOffset_Y = -10;
        nickLabel.sizeScale_X = 1f;
        nickLabel.sizeScale_Y = 1f;
        nickLabel.fontAlignment = TextAnchor.MiddleCenter;
        button.AddChild(nickLabel);
        skillsetLabel = Glazier.Get().CreateLabel();
        skillsetLabel.positionOffset_X = 5;
        skillsetLabel.positionOffset_Y = 5;
        skillsetLabel.sizeOffset_X = -10;
        skillsetLabel.sizeOffset_Y = -10;
        skillsetLabel.sizeScale_X = 1f;
        skillsetLabel.sizeScale_Y = 1f;
        skillsetLabel.fontAlignment = TextAnchor.LowerCenter;
        button.AddChild(skillsetLabel);
        if (!Provider.isPro && index >= Customization.FREE_CHARACTERS)
        {
            Bundle bundle = Bundles.getBundle("/Bundles/Textures/Menu/Icons/Pro/Pro.unity3d");
            ISleekImage sleekImage = Glazier.Get().CreateImage();
            sleekImage.positionOffset_X = -20;
            sleekImage.positionOffset_Y = -20;
            sleekImage.positionScale_X = 0.5f;
            sleekImage.positionScale_Y = 0.5f;
            sleekImage.sizeOffset_X = 40;
            sleekImage.sizeOffset_Y = 40;
            sleekImage.texture = bundle.load<Texture2D>("Lock_Medium");
            button.AddChild(sleekImage);
            bundle.unload();
        }
        updateCharacter(Characters.list[index]);
    }
}
