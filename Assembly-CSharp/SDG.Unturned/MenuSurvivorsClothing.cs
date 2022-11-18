using UnityEngine;

namespace SDG.Unturned;

public class MenuSurvivorsClothing : MonoBehaviour
{
    private void onClickedMouse()
    {
        Physics.Raycast(MainCamera.instance.ScreenPointToRay(Input.mousePosition), out var hitInfo, 64f, RayMasks.CLOTHING_INTERACT);
        if (hitInfo.collider == null)
        {
            return;
        }
        Transform transform = hitInfo.collider.transform;
        if (transform.CompareTag("Player"))
        {
            switch (DamageTool.getLimb(transform))
            {
            case ELimb.LEFT_FOOT:
            case ELimb.LEFT_LEG:
            case ELimb.RIGHT_FOOT:
            case ELimb.RIGHT_LEG:
                if (Characters.active.packagePants != 0L)
                {
                    Characters.package(Characters.active.packagePants);
                }
                break;
            case ELimb.LEFT_HAND:
            case ELimb.LEFT_ARM:
            case ELimb.RIGHT_HAND:
            case ELimb.RIGHT_ARM:
            case ELimb.SPINE:
                if (Characters.active.packageShirt != 0L)
                {
                    Characters.package(Characters.active.packageShirt);
                }
                break;
            }
        }
        else if (transform.CompareTag("Enemy"))
        {
            if (transform.name == "Hat")
            {
                if (Characters.active.packageHat != 0L)
                {
                    Characters.package(Characters.active.packageHat);
                }
            }
            else if (transform.name == "Glasses")
            {
                if (Characters.active.packageGlasses != 0L)
                {
                    Characters.package(Characters.active.packageGlasses);
                }
            }
            else if (transform.name == "Mask")
            {
                if (Characters.active.packageMask != 0L)
                {
                    Characters.package(Characters.active.packageMask);
                }
            }
            else if (transform.name == "Vest")
            {
                if (Characters.active.packageVest != 0L)
                {
                    Characters.package(Characters.active.packageVest);
                }
            }
            else if (transform.name == "Backpack" && Characters.active.packageBackpack != 0L)
            {
                Characters.package(Characters.active.packageBackpack);
            }
        }
        if (MenuSurvivorsClothingItemUI.active)
        {
            MenuSurvivorsClothingItemUI.viewItem();
        }
    }

    private void Update()
    {
        if ((MenuSurvivorsClothingUI.active || MenuSurvivorsClothingItemUI.active) && Input.GetMouseButtonUp(0) && Glazier.Get().ShouldGameProcessInput)
        {
            onClickedMouse();
        }
    }
}
