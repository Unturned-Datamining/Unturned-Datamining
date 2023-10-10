using System;
using System.Collections.Generic;
using UnityEngine;

namespace SDG.Unturned;

public class SleekJars : SleekWrapper
{
    public ClickedJar onClickedJar;

    private void onClickedButton(SleekItem item)
    {
        int num = FindIndexOfChild(item);
        if (num != -1)
        {
            onClickedJar?.Invoke(this, num);
        }
    }

    public SleekJars(float radius, List<InventorySearch> search, float startAngle = 0f)
    {
        float num = MathF.PI * 2f / (float)search.Count;
        for (int i = 0; i < search.Count; i++)
        {
            ItemJar jar = search[i].jar;
            if (jar.GetAsset() != null)
            {
                SleekItem sleekItem = new SleekItem(jar);
                sleekItem.PositionOffset_X = (float)(int)(Mathf.Cos(startAngle + num * (float)i) * radius) - sleekItem.SizeOffset_X / 2f;
                sleekItem.PositionOffset_Y = (float)(int)(Mathf.Sin(startAngle + num * (float)i) * radius) - sleekItem.SizeOffset_Y / 2f;
                sleekItem.PositionScale_X = 0.5f;
                sleekItem.PositionScale_Y = 0.5f;
                sleekItem.onClickedItem = onClickedButton;
                sleekItem.onDraggedItem = onClickedButton;
                AddChild(sleekItem);
            }
        }
    }
}
