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
                regionLabels[i].IsVisible = false;
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
                int num2 = x - DEBUG_SIZE / 2 + i;
                int num3 = y - DEBUG_SIZE / 2 + j;
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
                sleekLabel.Text = localization.format("Point", num2, num3);
                sleekLabel.Text = sleekLabel.Text + "\n" + localization.format("Objects", num4, num6);
                sleekLabel.Text = sleekLabel.Text + "\n" + localization.format("Triangles", num7);
                if (num4 == 0 && num7 == 0)
                {
                    sleekLabel.TextColor = Color.white;
                }
                else
                {
                    sleekLabel.TextColor = ItemTool.getQualityColor(quality);
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
                int x = Editor.editor.area.region_x - DEBUG_SIZE / 2 + i;
                int y = Editor.editor.area.region_y - DEBUG_SIZE / 2 + j;
                ISleekLabel sleekLabel = regionLabels[num];
                if (Regions.tryGetPoint(x, y, out var point))
                {
                    Vector3 vector = MainCamera.instance.WorldToViewportPoint(point + new Vector3(Regions.REGION_SIZE / 2, 0f, Regions.REGION_SIZE / 2));
                    if (vector.z > 0f)
                    {
                        Vector2 vector2 = container.ViewportToNormalizedPosition(vector);
                        sleekLabel.PositionScale_X = vector2.x;
                        sleekLabel.PositionScale_Y = vector2.y;
                        sleekLabel.IsVisible = true;
                    }
                    else
                    {
                        sleekLabel.IsVisible = false;
                    }
                }
                else
                {
                    sleekLabel.IsVisible = false;
                }
            }
        }
    }

    public EditorLevelVisibilityUI()
    {
        localization = Localization.read("/Editor/EditorLevelVisibility.dat");
        container = new SleekFullscreenBox();
        container.PositionScale_X = 1f;
        container.SizeScale_X = 1f;
        container.SizeScale_Y = 1f;
        EditorUI.window.AddChild(container);
        active = false;
        roadsToggle = Glazier.Get().CreateToggle();
        roadsToggle.PositionOffset_X = -210f;
        roadsToggle.PositionOffset_Y = 90f;
        roadsToggle.PositionScale_X = 1f;
        roadsToggle.SizeOffset_X = 40f;
        roadsToggle.SizeOffset_Y = 40f;
        roadsToggle.Value = LevelVisibility.roadsVisible;
        roadsToggle.AddLabel(localization.format("Roads_Label"), ESleekSide.RIGHT);
        roadsToggle.OnValueChanged += onToggledRoadsToggle;
        container.AddChild(roadsToggle);
        navigationToggle = Glazier.Get().CreateToggle();
        navigationToggle.PositionOffset_X = -210f;
        navigationToggle.PositionOffset_Y = 140f;
        navigationToggle.PositionScale_X = 1f;
        navigationToggle.SizeOffset_X = 40f;
        navigationToggle.SizeOffset_Y = 40f;
        navigationToggle.Value = LevelVisibility.navigationVisible;
        navigationToggle.AddLabel(localization.format("Navigation_Label"), ESleekSide.RIGHT);
        navigationToggle.OnValueChanged += onToggledNavigationToggle;
        container.AddChild(navigationToggle);
        nodesToggle = Glazier.Get().CreateToggle();
        nodesToggle.PositionOffset_X = -210f;
        nodesToggle.PositionOffset_Y = 190f;
        nodesToggle.PositionScale_X = 1f;
        nodesToggle.SizeOffset_X = 40f;
        nodesToggle.SizeOffset_Y = 40f;
        nodesToggle.Value = LevelVisibility.nodesVisible;
        nodesToggle.AddLabel(localization.format("Nodes_Label"), ESleekSide.RIGHT);
        nodesToggle.OnValueChanged += onToggledNodesToggle;
        container.AddChild(nodesToggle);
        itemsToggle = Glazier.Get().CreateToggle();
        itemsToggle.PositionOffset_X = -210f;
        itemsToggle.PositionOffset_Y = 240f;
        itemsToggle.PositionScale_X = 1f;
        itemsToggle.SizeOffset_X = 40f;
        itemsToggle.SizeOffset_Y = 40f;
        itemsToggle.Value = LevelVisibility.itemsVisible;
        itemsToggle.AddLabel(localization.format("Items_Label"), ESleekSide.RIGHT);
        itemsToggle.OnValueChanged += onToggledItemsToggle;
        container.AddChild(itemsToggle);
        playersToggle = Glazier.Get().CreateToggle();
        playersToggle.PositionOffset_X = -210f;
        playersToggle.PositionOffset_Y = 290f;
        playersToggle.PositionScale_X = 1f;
        playersToggle.SizeOffset_X = 40f;
        playersToggle.SizeOffset_Y = 40f;
        playersToggle.Value = LevelVisibility.playersVisible;
        playersToggle.AddLabel(localization.format("Players_Label"), ESleekSide.RIGHT);
        playersToggle.OnValueChanged += onToggledPlayersToggle;
        container.AddChild(playersToggle);
        zombiesToggle = Glazier.Get().CreateToggle();
        zombiesToggle.PositionOffset_X = -210f;
        zombiesToggle.PositionOffset_Y = 340f;
        zombiesToggle.PositionScale_X = 1f;
        zombiesToggle.SizeOffset_X = 40f;
        zombiesToggle.SizeOffset_Y = 40f;
        zombiesToggle.Value = LevelVisibility.zombiesVisible;
        zombiesToggle.AddLabel(localization.format("Zombies_Label"), ESleekSide.RIGHT);
        zombiesToggle.OnValueChanged += onToggledZombiesToggle;
        container.AddChild(zombiesToggle);
        vehiclesToggle = Glazier.Get().CreateToggle();
        vehiclesToggle.PositionOffset_X = -210f;
        vehiclesToggle.PositionOffset_Y = 390f;
        vehiclesToggle.PositionScale_X = 1f;
        vehiclesToggle.SizeOffset_X = 40f;
        vehiclesToggle.SizeOffset_Y = 40f;
        vehiclesToggle.Value = LevelVisibility.vehiclesVisible;
        vehiclesToggle.AddLabel(localization.format("Vehicles_Label"), ESleekSide.RIGHT);
        vehiclesToggle.OnValueChanged += onToggledVehiclesToggle;
        container.AddChild(vehiclesToggle);
        borderToggle = Glazier.Get().CreateToggle();
        borderToggle.PositionOffset_X = -210f;
        borderToggle.PositionOffset_Y = 440f;
        borderToggle.PositionScale_X = 1f;
        borderToggle.SizeOffset_X = 40f;
        borderToggle.SizeOffset_Y = 40f;
        borderToggle.Value = LevelVisibility.borderVisible;
        borderToggle.AddLabel(localization.format("Border_Label"), ESleekSide.RIGHT);
        borderToggle.OnValueChanged += onToggledBorderToggle;
        container.AddChild(borderToggle);
        animalsToggle = Glazier.Get().CreateToggle();
        animalsToggle.PositionOffset_X = -210f;
        animalsToggle.PositionOffset_Y = 490f;
        animalsToggle.PositionScale_X = 1f;
        animalsToggle.SizeOffset_X = 40f;
        animalsToggle.SizeOffset_Y = 40f;
        animalsToggle.Value = LevelVisibility.animalsVisible;
        animalsToggle.AddLabel(localization.format("Animals_Label"), ESleekSide.RIGHT);
        animalsToggle.OnValueChanged += onToggledAnimalsToggle;
        container.AddChild(animalsToggle);
        decalsToggle = Glazier.Get().CreateToggle();
        decalsToggle.PositionOffset_X = -210f;
        decalsToggle.PositionOffset_Y = 540f;
        decalsToggle.PositionScale_X = 1f;
        decalsToggle.SizeOffset_X = 40f;
        decalsToggle.SizeOffset_Y = 40f;
        decalsToggle.Value = DecalSystem.IsVisible;
        decalsToggle.AddLabel(localization.format("Decals_Label"), ESleekSide.RIGHT);
        decalsToggle.OnValueChanged += onToggledDecalsToggle;
        container.AddChild(decalsToggle);
        regionLabels = new ISleekLabel[DEBUG_SIZE * DEBUG_SIZE];
        for (int i = 0; i < regionLabels.Length; i++)
        {
            ISleekLabel sleekLabel = Glazier.Get().CreateLabel();
            sleekLabel.PositionOffset_X = -100f;
            sleekLabel.PositionOffset_Y = -25f;
            sleekLabel.SizeOffset_X = 200f;
            sleekLabel.SizeOffset_Y = 50f;
            sleekLabel.TextContrastContext = ETextContrastContext.ColorfulBackdrop;
            regionLabels[i] = sleekLabel;
            container.AddChild(sleekLabel);
        }
        EditorArea area = Editor.editor.area;
        area.onRegionUpdated = (EditorRegionUpdated)Delegate.Combine(area.onRegionUpdated, new EditorRegionUpdated(onRegionUpdated));
    }
}
