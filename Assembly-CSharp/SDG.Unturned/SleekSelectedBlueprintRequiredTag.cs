using UnityEngine;

namespace SDG.Unturned;

public class SleekSelectedBlueprintRequiredTag : SleekWrapper
{
    private CachingAssetRef _tagRef;

    private ISleekImage icon;

    private ISleekLabel nameLabel;

    private ISleekLabel missingLabel;

    public void SetTag(CachingAssetRef tagRef, bool isMissing)
    {
        if (_tagRef != tagRef)
        {
            _tagRef = tagRef;
            TagAsset tagAsset = _tagRef.Get<TagAsset>();
            if (tagAsset != null)
            {
                icon.Texture = tagAsset.Icon;
                icon.TintColor = (tagAsset.ShouldTintIcon ? ESleekTint.FOREGROUND : ESleekTint.NONE);
                nameLabel.Text = tagAsset.PlainTextName;
                nameLabel.TextColor = tagAsset.NameColorOrPreferredFontColor;
            }
        }
        missingLabel.IsVisible = isMissing;
        nameLabel.SizeOffset_Y = (isMissing ? 30 : 50);
    }

    public SleekSelectedBlueprintRequiredTag()
    {
        ISleekBox sleekBox = Glazier.Get().CreateBox();
        sleekBox.SizeScale_X = 1f;
        sleekBox.SizeScale_Y = 1f;
        AddChild(sleekBox);
        icon = Glazier.Get().CreateImage();
        icon.PositionOffset_X = 5f;
        icon.PositionOffset_Y = 5f;
        icon.SizeOffset_X = 40f;
        icon.SizeOffset_Y = 40f;
        AddChild(icon);
        nameLabel = Glazier.Get().CreateLabel();
        nameLabel.PositionOffset_X = 50f;
        nameLabel.SizeOffset_X = -50f;
        nameLabel.SizeScale_X = 1f;
        nameLabel.TextAlignment = TextAnchor.MiddleLeft;
        nameLabel.AllowRichText = false;
        AddChild(nameLabel);
        missingLabel = Glazier.Get().CreateLabel();
        missingLabel.PositionOffset_X = 50f;
        missingLabel.PositionOffset_Y = 20f;
        missingLabel.SizeOffset_X = -50f;
        missingLabel.SizeScale_X = 1f;
        missingLabel.SizeOffset_Y = 30f;
        missingLabel.TextColor = ESleekTint.BAD;
        missingLabel.Text = PlayerDashboardCraftingUI.localization.format("Details_TagMissing");
        missingLabel.TextAlignment = TextAnchor.MiddleLeft;
        AddChild(missingLabel);
    }
}
