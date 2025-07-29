using System;
using System.Reflection;

namespace SDG.Unturned;

internal class SleekConfigProperty : SleekWrapper
{
    public FieldInfo fieldInfo;

    public object defaultValue;

    private ISleekToggle overrideToggle;

    private ISleekElement valueWidget;

    public event Action<SleekConfigProperty, bool, object> OnValueChanged;

    public void SetOverrideState(bool isOverridden, object overrideValue)
    {
        overrideToggle.Value = isOverridden;
        object obj = (isOverridden ? overrideValue : defaultValue);
        Type fieldType = fieldInfo.FieldType;
        if (fieldType == typeof(uint))
        {
            ISleekUInt32Field obj2 = (ISleekUInt32Field)valueWidget;
            obj2.Value = (uint)obj;
            obj2.IsClickable = isOverridden;
        }
        else if (fieldType == typeof(float))
        {
            ISleekFloat32Field obj3 = (ISleekFloat32Field)valueWidget;
            obj3.Value = (float)obj;
            obj3.IsClickable = isOverridden;
        }
        else if (fieldType == typeof(bool))
        {
            ISleekToggle obj4 = (ISleekToggle)valueWidget;
            obj4.Value = (bool)obj;
            obj4.IsInteractable = isOverridden;
        }
    }

    public SleekConfigProperty(FieldInfo fieldInfo, string tooltip)
    {
        this.fieldInfo = fieldInfo;
        Type fieldType = fieldInfo.FieldType;
        if (fieldType == typeof(uint))
        {
            ISleekUInt32Field sleekUInt32Field = Glazier.Get().CreateUInt32Field();
            sleekUInt32Field.SizeOffset_X = 200f;
            sleekUInt32Field.SizeOffset_Y = 30f;
            sleekUInt32Field.AddLabel(MenuPlayConfigUI.sanitizeName(fieldInfo.Name), ESleekSide.RIGHT);
            sleekUInt32Field.OnValueChanged += OnTypedUInt32Value;
            sleekUInt32Field.TooltipText = tooltip;
            AddChild(sleekUInt32Field);
            valueWidget = sleekUInt32Field;
            base.SizeOffset_Y = 30f;
        }
        else if (fieldType == typeof(float))
        {
            ISleekFloat32Field sleekFloat32Field = Glazier.Get().CreateFloat32Field();
            sleekFloat32Field.SizeOffset_X = 200f;
            sleekFloat32Field.SizeOffset_Y = 30f;
            sleekFloat32Field.AddLabel(MenuPlayConfigUI.sanitizeName(fieldInfo.Name), ESleekSide.RIGHT);
            sleekFloat32Field.OnValueChanged += OnTypedSingleValue;
            sleekFloat32Field.TooltipText = tooltip;
            AddChild(sleekFloat32Field);
            valueWidget = sleekFloat32Field;
            base.SizeOffset_Y = 30f;
        }
        else
        {
            if (!(fieldType == typeof(bool)))
            {
                throw new NotSupportedException(fieldInfo.ToString());
            }
            ISleekToggle sleekToggle = Glazier.Get().CreateToggle();
            sleekToggle.SizeOffset_X = 40f;
            sleekToggle.SizeOffset_Y = 40f;
            sleekToggle.AddLabel(MenuPlayConfigUI.sanitizeName(fieldInfo.Name), ESleekSide.RIGHT);
            sleekToggle.OnValueChanged += OnToggledValue;
            sleekToggle.TooltipText = tooltip;
            AddChild(sleekToggle);
            valueWidget = sleekToggle;
            base.SizeOffset_Y = 40f;
        }
        overrideToggle = Glazier.Get().CreateToggle();
        overrideToggle.PositionOffset_X = -40f;
        overrideToggle.PositionOffset_Y = -20f;
        overrideToggle.PositionScale_Y = 0.5f;
        overrideToggle.SizeOffset_X = 40f;
        overrideToggle.SizeOffset_Y = 40f;
        overrideToggle.OnValueChanged += OnOverrideToggled;
        overrideToggle.TooltipText = MenuPlayConfigUI.localization.format("Override_Tooltip");
        AddChild(overrideToggle);
    }

    private void OnTypedUInt32Value(ISleekUInt32Field uint32Field, uint state)
    {
        this.OnValueChanged?.Invoke(this, arg2: true, state);
    }

    private void OnTypedSingleValue(ISleekFloat32Field singleField, float state)
    {
        this.OnValueChanged?.Invoke(this, arg2: true, state);
    }

    private void OnToggledValue(ISleekToggle toggle, bool state)
    {
        this.OnValueChanged?.Invoke(this, arg2: true, state);
    }

    private void OnOverrideToggled(ISleekToggle toggle, bool state)
    {
        this.OnValueChanged?.Invoke(this, state, defaultValue);
        SetOverrideState(state, defaultValue);
    }
}
