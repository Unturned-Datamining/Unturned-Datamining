using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Unturned.SystemEx;

namespace SDG.Unturned;

internal static class ServerPrefabUtil
{
    private static List<Component> workingComponents;

    private static HashSet<Type> typesToRemove;

    public static void RemoveClientComponents(GameObject gameObject)
    {
        gameObject.GetComponentsInChildren(includeInactive: true, workingComponents);
        workingComponents.RemoveSwap(delegate(Component component)
        {
            if (component == null)
            {
                return true;
            }
            if (typesToRemove.Contains(component.GetType()))
            {
                return false;
            }
            if (component is Animation animation)
            {
                animation.cullingType = AnimationCullingType.AlwaysAnimate;
            }
            return true;
        });
        workingComponents.Sort((Component lhs, Component rhs) => (lhs is TextMeshPro || lhs is TextMesh || lhs is LODGroupAdditionalData) ? (-1) : 0);
        foreach (Component workingComponent in workingComponents)
        {
            UnityEngine.Object.DestroyImmediate(workingComponent, allowDestroyingAssets: true);
        }
        workingComponents.Clear();
    }

    static ServerPrefabUtil()
    {
        workingComponents = new List<Component>();
        typesToRemove = new HashSet<Type>
        {
            typeof(LODGroup),
            typeof(LODGroupAdditionalData),
            typeof(MeshFilter),
            typeof(Cloth),
            typeof(TextMesh),
            typeof(TextMeshPro),
            typeof(TextMeshProUGUI),
            typeof(WindZone),
            typeof(LensFlare),
            typeof(Projector),
            typeof(Camera),
            typeof(Skybox),
            typeof(FlareLayer),
            typeof(Light),
            typeof(LightProbeGroup),
            typeof(LightProbeProxyVolume),
            typeof(ReflectionProbe),
            typeof(Tree),
            typeof(CanvasRenderer),
            typeof(Button),
            typeof(CanvasScaler),
            typeof(Dropdown),
            typeof(Graphic),
            typeof(GridLayoutGroup),
            typeof(HorizontalLayoutGroup),
            typeof(Image),
            typeof(InputField),
            typeof(LayoutElement),
            typeof(LayoutGroup),
            typeof(Mask),
            typeof(MaskableGraphic),
            typeof(RawImage),
            typeof(RectMask2D),
            typeof(Scrollbar),
            typeof(ScrollRect),
            typeof(Slider),
            typeof(Text),
            typeof(Toggle),
            typeof(ToggleGroup),
            typeof(VerticalLayoutGroup),
            typeof(AudioChorusFilter),
            typeof(AudioDistortionFilter),
            typeof(AudioEchoFilter),
            typeof(AudioHighPassFilter),
            typeof(AudioListener),
            typeof(AudioLowPassFilter),
            typeof(AudioReverbFilter),
            typeof(AudioReverbZone),
            typeof(AudioSource),
            typeof(Renderer),
            typeof(BillboardRenderer),
            typeof(LineRenderer),
            typeof(MeshRenderer),
            typeof(ParticleSystemRenderer),
            typeof(SkinnedMeshRenderer),
            typeof(SpriteMask),
            typeof(SpriteRenderer),
            typeof(TrailRenderer)
        };
    }
}
