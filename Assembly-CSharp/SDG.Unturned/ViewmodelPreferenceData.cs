using UnityEngine;

namespace SDG.Unturned;

public class ViewmodelPreferenceData
{
    public float Field_Of_View_Aim;

    public float Field_Of_View_Hip;

    public float Offset_Horizontal;

    public float Offset_Vertical;

    public float Offset_Depth;

    public void Clamp()
    {
        Field_Of_View_Aim = (Field_Of_View_Aim.IsFinite() ? Mathf.Clamp(Field_Of_View_Aim, 1f, 179f) : 60f);
        Field_Of_View_Hip = (Field_Of_View_Hip.IsFinite() ? Mathf.Clamp(Field_Of_View_Hip, 1f, 179f) : 60f);
        Offset_Horizontal = (Offset_Horizontal.IsFinite() ? Mathf.Clamp(Offset_Horizontal, -1f, 1f) : 0f);
        Offset_Vertical = (Offset_Vertical.IsFinite() ? Mathf.Clamp(Offset_Vertical, -1f, 1f) : 0f);
        Offset_Depth = (Offset_Depth.IsFinite() ? Mathf.Clamp(Offset_Depth, -0.5f, 0.5f) : 0f);
    }

    public ViewmodelPreferenceData()
    {
        Field_Of_View_Aim = 60f;
        Field_Of_View_Hip = 60f;
        Offset_Horizontal = 0f;
        Offset_Vertical = 0f;
        Offset_Depth = 0f;
    }
}
