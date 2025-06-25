using System;

namespace SDG.Unturned;

public class SleekBcAssetField : SleekWrapper
{
    private Type _expectedType = typeof(Asset);

    private EAssetType _legacyType;

    private CachingBcAssetRef _value;

    private ISleekField idField;

    private ISleekBox infoBox;

    public Type ExpectedType
    {
        get
        {
            return _expectedType;
        }
        set
        {
            _expectedType = value;
            UpdateInfoBox();
        }
    }

    public EAssetType LegacyType
    {
        get
        {
            return _legacyType;
        }
        set
        {
            _legacyType = value;
            UpdateInfoBox();
        }
    }

    public CachingBcAssetRef Value
    {
        get
        {
            return _value;
        }
        set
        {
            _value = value;
            SynchronizeField();
            UpdateInfoBox();
        }
    }

    public string TooltipText
    {
        get
        {
            return infoBox.TooltipText;
        }
        set
        {
            idField.TooltipText = value;
            infoBox.TooltipText = value;
        }
    }

    public event Action<SleekBcAssetField> OnValueChanged;

    public SleekBcAssetField(CachingBcAssetRef value, Type expectedType, EAssetType legacyType)
    {
        base.SizeOffset_Y = 60f;
        _value = value;
        _expectedType = expectedType;
        _legacyType = legacyType;
        idField = Glazier.Get().CreateStringField();
        idField.SizeScale_X = 1f;
        idField.SizeScale_Y = 0.5f;
        idField.OnTextChanged += OnTextChanged;
        idField.OnTextSubmitted += OnTextSubmitted;
        AddChild(idField);
        infoBox = Glazier.Get().CreateBox();
        infoBox.PositionScale_Y = 0.5f;
        infoBox.SizeScale_X = 1f;
        infoBox.SizeScale_Y = 0.5f;
        AddChild(infoBox);
        SynchronizeField();
        UpdateInfoBox();
    }

    public SleekBcAssetField(Type expectedType, EAssetType legacyType)
        : this(null, expectedType, legacyType)
    {
    }

    public SleekBcAssetField(EAssetType legacyType)
        : this(null, typeof(Asset), legacyType)
    {
    }

    private void OnTextChanged(ISleekField field, string value)
    {
        UpdateValue();
    }

    private void OnTextSubmitted(ISleekField field)
    {
        UpdateValue();
    }

    private void UpdateValue()
    {
        CachingBcAssetRef.TryParse(idField.Text, out var result);
        if (_value != result)
        {
            _value = result;
            UpdateInfoBox();
            this.OnValueChanged?.Invoke(this);
        }
    }

    private void SynchronizeField()
    {
        if (_value.Guid != Guid.Empty)
        {
            idField.Text = _value.Guid.ToString("N");
        }
        else
        {
            idField.Text = _value.LegacyId.ToString();
        }
    }

    private void UpdateInfoBox()
    {
        Asset asset = _value.Get();
        if (asset == null)
        {
            infoBox.TextColor = ESleekTint.FONT;
            infoBox.Text = "null";
            return;
        }
        Type type = asset.GetType();
        if (_expectedType.IsAssignableFrom(type))
        {
            infoBox.TextColor = ESleekTint.FONT;
        }
        else
        {
            infoBox.TextColor = ESleekTint.BAD;
        }
        infoBox.Text = asset.FriendlyName;
    }
}
