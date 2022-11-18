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
        if (num != -1 && onClickedJar != null)
        {
            onClickedJar(this, num);
        }
    }

    public SleekJars(float radius, List<InventorySearch> search, float startAngle = 0f)
    {
        float num = (float)Math.PI * 2f / (float)search.Count;
        for (int i = 0; i < search.Count; i++)
        {
            ItemJar jar = search[i].jar;
            if (jar.GetAsset() != null)
            {
                SleekItem sleekItem = new SleekItem(jar);
                sleekItem.positionOffset_X = (int)(Mathf.Cos(startAngle + num * (float)i) * radius) - sleekItem.sizeOffset_X / 2;
                sleekItem.positionOffset_Y = (int)(Mathf.Sin(startAngle + num * (float)i) * radius) - sleekItem.sizeOffset_Y / 2;
                sleekItem.positionScale_X = 0.5f;
                sleekItem.positionScale_Y = 0.5f;
                sleekItem.onClickedItem = onClickedButton;
                sleekItem.onDraggedItem = onClickedButton;
                AddChild(sleekItem);
            }
        }
    }
}
