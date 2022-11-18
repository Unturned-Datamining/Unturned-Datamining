using System;
using System.Collections.Generic;
using UnityEngine;

namespace SDG.Unturned;

public class EditorLevelVisibilityUI
{
    private static readonly byte DEBUG_SIZE = 7;

    private static Local localization;

    private static SleekFullscreenBox container;

    public static bool active;

    private static List<MeshFilter> meshes = new List<MeshFilter>();

    public static ISleekToggle roadsToggle;

    public static ISleekToggle navigationToggle;

    public static ISleekToggle nodesToggle;

    public static ISleekToggle itemsToggle;

    public static ISleekToggle playersToggle;

    public static ISleekToggle zombiesToggle;

    public static ISleekToggle vehiclesToggle;

    public static ISleekToggle borderToggle;

    public static ISleekToggle animalsToggle;

    public static ISleekToggle decalsToggle;

    private static ISleekLabel[] regionLabels;

    public static void open()
    {
        if (!active)
        {
            active = true;
            update(Editor.editor.area.region_x, Editor.editor.area.region_y);
            EditorUI.message(EEditorMessage.VISIBILITY);
            container.AnimateIntoView();
        }
    }

    public static void close()
    {
        if (active)
        {
            active = false;
            for (int i = 0; i < regionLabels.Length; i++)
            {
                regionLabels[i].isVisible = false;
            }
            container.AnimateOutOfView(1f, 0f);
        }
    }

    private static void onToggledRoadsToggle(ISleekToggle toggle, bool state)
    {
        LevelVisibility.roadsVisible = state;
    }

    private static void onToggledNavigationToggle(ISleekToggle toggle, bool state)
    {
        LevelVisibility.navigationVisible = state;
    }

    private static void onToggledNodesToggle(ISleekToggle toggle, bool state)
    {
        LevelVisibility.nodesVisible = state;
    }

    private static void onToggledItemsToggle(ISleekToggle toggle, bool state)
    {
        LevelVisibility.itemsVisible = state;
    }

    private static void onToggledPlayersToggle(ISleekToggle toggle, bool state)
    {
        LevelVisibility.playersVisible = state;
    }

    private static void onToggledZombiesToggle(ISleekToggle toggle, bool state)
    {
        LevelVisibility.zombiesVisible = state;
    }

    private static void onToggledVehiclesToggle(ISleekToggle toggle, bool state)
    {
        LevelVisibility.vehiclesVisible = state;
    }

    private static void onToggledBorderToggle(ISleekToggle toggle, bool state)
    {
        LevelVisibility.borderVisible = state;
    }

    private static void onToggledAnimalsToggle(ISleekToggle toggle, bool state)
    {
        LevelVisibility.animalsVisible = state;
    }

    private static void onToggledDecalsToggle(ISleekToggle toggle, bool state)
    {
        DecalSystem.IsVisible = state;
    }

    private static void onRegionUpdated(byte old_x, byte old_y, byte new_x, byte new_y)
    {
        if (active)
        {
            update(new_x, new_y);
        }
    }

    private static void update(int x, int y)
    {
        for (int i = 0; i < DEBUG_SIZE; i++)
        {
            for (int j = 0; j < DEBUG_SIZE; j++)
            {
                int num = i * DEBUG_SIZE + j;
                int num2 = x - (int)DEBUG_SIZE / 2 + i;
                int num3 = y - (int)DEBUG_SIZE / 2 + j;
                ISleekLabel sleekLabel = regionLabels[num];
                if (!Regions.checkSafe(num2, num3))
                {
                    continue;
                }
                int num4 = LevelObjects.objects[num2, num3].Count + LevelGround.trees[num2, num3].Count;
                int num5 = LevelObjects.total + LevelGround.total;
                double num6 = Math.Round((double)num4 / (double)num5 * 1000.0) / 10.0;
                int num7 = 0;
                for (int k = 0; k < LevelObjects.objects[num2, num3].Count; k++)
                {
                    LevelObject levelObject = LevelObjects.objects[num2, num3][k];
                    if (!levelObject.transform)
                    {
                        continue;
                    }
                    levelObject.transform.GetComponents(meshes);
                    if (meshes.Count == 0)
                    {
                        Transform transform = levelObject.transform.Find("Model_0");
                        if ((bool)transform)
                        {
                            transform.GetComponentsInChildren(includeInactive: true, meshes);
                        }
                    }
                    if (meshes.Count == 0)
                    {
                        continue;
                    }
                    for (int l = 0; l < meshes.Count; l++)
                    {
                        Mesh sharedMesh = meshes[l].sharedMesh;
                        if ((bool)sharedMesh)
                        {
                            num7 += sharedMesh.triangles.Length;
                        }
                    }
                }
                for (int m = 0; m < LevelGround.trees[num2, num3].Count; m++)
                {
                    ResourceSpawnpoint resourceSpawnpoint = LevelGround.trees[num2, num3][m];
                    if (!resourceSpawnpoint.model)
                    {
                        continue;
                    }
                    resourceSpawnpoint.model.GetComponents(meshes);
                    if (meshes.Count == 0)
                    {
                        Transform transform2 = resourceSpawnpoint.model.Find("Model_0");
                        if ((bool)transform2)
                        {
                            transform2.GetComponentsInChildren(includeInactive: true, meshes);
                        }
                    }
                    if (meshes.Count == 0)
                    {
                        continue;
                    }
                    for (int n = 0; n < meshes.Count; n++)
                    {
                        Mesh sharedMesh2 = meshes[n].sharedMesh;
                        if ((bool)sharedMesh2)
                        {
                            num7 += sharedMesh2.triangles.Length;
                        }
                    }
                }
                long num8 = (long)num4 * (long)num7;
                float quality = Mathf.Clamp01((float)(1.0 - (double)num8 / 50000000.0));
                sleekLabel.text = localization.format("Point", num2, num3);
                sleekLabel.text = sleekLabel.text + "\n" + localization.format("Objects", num4, num6);
                sleekLabel.text = sleekLabel.text + "\n" + localization.format("Triangles", num7);
                if (num4 == 0 && num7 == 0)
                {
                    sleekLabel.textColor = Color.white;
                }
                else
                {
                    sleekLabel.textColor = ItemTool.getQualityColor(quality);
                }
            }
        }
    }

    public static void update()
    {
        for (int i = 0; i < DEBUG_SIZE; i++)
        {
            for (int j = 0; j < DEBUG_SIZE; j++)
            {
                int num = i * DEBUG_SIZE + j;
                int x = Editor.editor.area.region_x - (int)DEBUG_SIZE / 2 + i;
                int y = Editor.editor.area.region_y - (int)DEBUG_SIZE / 2 + j;
                ISleekLabel sleekLabel = regionLabels[num];
                if (Regions.tryGetPoint(x, y, out var point))
                {
                    Vector3 vector = MainCamera.instance.WorldToViewportPoint(point + new Vector3((int)Regions.REGION_SIZE / 2, 0f, (int)Regions.REGION_SIZE / 2));
                    if (vector.z > 0f)
                    {
                        Vector2 vector2 = container.ViewportToNormalizedPosition(vector);
                        sleekLabel.positionScale_X = vector2.x;
                        sleekLabel.positionScale_Y = vector2.y;
                        sleekLabel.isVisible = true;
                    }
                    else
                    {
                        sleekLabel.isVisible = false;
                    }
                }
                else
                {
                    sleekLabel.isVisible = false;
                }
            }
        }
    }

    public EditorLevelVisibilityUI()
    {
        localization = Localization.read("/Editor/EditorLevelVisibility.dat");
        container = new SleekFullscreenBox();
        container.positionScale_X = 1f;
        container.sizeScale_X = 1f;
        container.sizeScale_Y = 1f;
        EditorUI.window.AddChild(container);
        active = false;
        roadsToggle = Glazier.Get().CreateToggle();
        roadsToggle.positionOffset_X = -210;
        roadsToggle.positionOffset_Y = 90;
        roadsToggle.positionScale_X = 1f;
        roadsToggle.sizeOffset_X = 40;
        roadsToggle.sizeOffset_Y = 40;
        roadsToggle.state = LevelVisibility.roadsVisible;
        roadsToggle.addLabel(localization.format("Roads_Label"), ESleekSide.RIGHT);
        roadsToggle.onToggled += onToggledRoadsToggle;
        container.AddChild(roadsToggle);
        navigationToggle = Glazier.Get().CreateToggle();
        navigationToggle.positionOffset_X = -210;
        navigationToggle.positionOffset_Y = 140;
        navigationToggle.positionScale_X = 1f;
        navigationToggle.sizeOffset_X = 40;
        navigationToggle.sizeOffset_Y = 40;
        navigationToggle.state = LevelVisibility.navigationVisible;
        navigationToggle.addLabel(localization.format("Navigation_Label"), ESleekSide.RIGHT);
        navigationToggle.onToggled += onToggledNavigationToggle;
        container.AddChild(navigationToggle);
        nodesToggle = Glazier.Get().CreateToggle();
        nodesToggle.positionOffset_X = -210;
        nodesToggle.positionOffset_Y = 190;
        nodesToggle.positionScale_X = 1f;
        nodesToggle.sizeOffset_X = 40;
        nodesToggle.sizeOffset_Y = 40;
        nodesToggle.state = LevelVisibility.nodesVisible;
        nodesToggle.addLabel(localization.format("Nodes_Label"), ESleekSide.RIGHT);
        nodesToggle.onToggled += onToggledNodesToggle;
        container.AddChild(nodesToggle);
        itemsToggle = Glazier.Get().CreateToggle();
        itemsToggle.positionOffset_X = -210;
        itemsToggle.positionOffset_Y = 240;
        itemsToggle.positionScale_X = 1f;
        itemsToggle.sizeOffset_X = 40;
        itemsToggle.sizeOffset_Y = 40;
        itemsToggle.state = LevelVisibility.itemsVisible;
        itemsToggle.addLabel(localization.format("Items_Label"), ESleekSide.RIGHT);
        itemsToggle.onToggled += onToggledItemsToggle;
        container.AddChild(itemsToggle);
        playersToggle = Glazier.Get().CreateToggle();
        playersToggle.positionOffset_X = -210;
        playersToggle.positionOffset_Y = 290;
        playersToggle.positionScale_X = 1f;
        playersToggle.sizeOffset_X = 40;
        playersToggle.sizeOffset_Y = 40;
        playersToggle.state = LevelVisibility.playersVisible;
        playersToggle.addLabel(localization.format("Players_Label"), ESleekSide.RIGHT);
        playersToggle.onToggled += onToggledPlayersToggle;
        container.AddChild(playersToggle);
        zombiesToggle = Glazier.Get().CreateToggle();
        zombiesToggle.positionOffset_X = -210;
        zombiesToggle.positionOffset_Y = 340;
        zombiesToggle.positionScale_X = 1f;
        zombiesToggle.sizeOffset_X = 40;
        zombiesToggle.sizeOffset_Y = 40;
        zombiesToggle.state = LevelVisibility.zombiesVisible;
        zombiesToggle.addLabel(localization.format("Zombies_Label"), ESleekSide.RIGHT);
        zombiesToggle.onToggled += onToggledZombiesToggle;
        container.AddChild(zombiesToggle);
        vehiclesToggle = Glazier.Get().CreateToggle();
        vehiclesToggle.positionOffset_X = -210;
        vehiclesToggle.positionOffset_Y = 390;
        vehiclesToggle.positionScale_X = 1f;
        vehiclesToggle.sizeOffset_X = 40;
        vehiclesToggle.sizeOffset_Y = 40;
        vehiclesToggle.state = LevelVisibility.vehiclesVisible;
        vehiclesToggle.addLabel(localization.format("Vehicles_Label"), ESleekSide.RIGHT);
        vehiclesToggle.onToggled += onToggledVehiclesToggle;
        container.AddChild(vehiclesToggle);
        borderToggle = Glazier.Get().CreateToggle();
        borderToggle.positionOffset_X = -210;
        borderToggle.positionOffset_Y = 440;
        borderToggle.positionScale_X = 1f;
        borderToggle.sizeOffset_X = 40;
        borderToggle.sizeOffset_Y = 40;
        borderToggle.state = LevelVisibility.borderVisible;
        borderToggle.addLabel(localization.format("Border_Label"), ESleekSide.RIGHT);
        borderToggle.onToggled += onToggledBorderToggle;
        container.AddChild(borderToggle);
        animalsToggle = Glazier.Get().CreateToggle();
        animalsToggle.positionOffset_X = -210;
        animalsToggle.positionOffset_Y = 490;
        animalsToggle.positionScale_X = 1f;
        animalsToggle.sizeOffset_X = 40;
        animalsToggle.sizeOffset_Y = 40;
        animalsToggle.state = LevelVisibility.animalsVisible;
        animalsToggle.addLabel(localization.format("Animals_Label"), ESleekSide.RIGHT);
        animalsToggle.onToggled += onToggledAnimalsToggle;
        container.AddChild(animalsToggle);
        decalsToggle = Glazier.Get().CreateToggle();
        decalsToggle.positionOffset_X = -210;
        decalsToggle.positionOffset_Y = 540;
        decalsToggle.positionScale_X = 1f;
        decalsToggle.sizeOffset_X = 40;
        decalsToggle.sizeOffset_Y = 40;
        decalsToggle.state = DecalSystem.IsVisible;
        decalsToggle.addLabel(localization.format("Decals_Label"), ESleekSide.RIGHT);
        decalsToggle.onToggled += onToggledDecalsToggle;
        container.AddChild(decalsToggle);
        regionLabels = new ISleekLabel[DEBUG_SIZE * DEBUG_SIZE];
        for (int i = 0; i < regionLabels.Length; i++)
        {
            ISleekLabel sleekLabel = Glazier.Get().CreateLabel();
            sleekLabel.positionOffset_X = -100;
            sleekLabel.positionOffset_Y = -25;
            sleekLabel.sizeOffset_X = 200;
            sleekLabel.sizeOffset_Y = 50;
            sleekLabel.shadowStyle = ETextContrastContext.ColorfulBackdrop;
            regionLabels[i] = sleekLabel;
            container.AddChild(sleekLabel);
        }
        EditorArea area = Editor.editor.area;
        area.onRegionUpdated = (EditorRegionUpdated)Delegate.Combine(area.onRegionUpdated, new EditorRegionUpdated(onRegionUpdated));
    }
}
