#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public static class Chapter2StageTemplateFactory
{
    private const string TemplateFolder = "Assets/_TARZ/ScriptableObjects/StageTemplates/Chapter2_SeaVillage";

    [MenuItem("TARZ/Stage/Create Chapter 2 Sample Stage Templates")]
    public static void CreateChapter2SampleStageTemplates()
    {
        EnsureFolder("Assets/_TARZ/ScriptableObjects", "StageTemplates");
        EnsureFolder("Assets/_TARZ/ScriptableObjects/StageTemplates", "Chapter2_SeaVillage");

        StageTemplateData stage1 = CreateOrReplaceTemplate(
            "StageTemplate_CH2_Stage01.asset",
            BuildStage1Template()
        );

        StageTemplateData stage2 = CreateOrReplaceTemplate(
            "StageTemplate_CH2_Stage02.asset",
            BuildStage2Template()
        );

        ChapterThemeData selectedTheme = Selection.activeObject as ChapterThemeData;

        if (selectedTheme != null && selectedTheme.chapterNumber == 2)
        {
            AssignTemplate(selectedTheme, stage1);
            AssignTemplate(selectedTheme, stage2);
            EditorUtility.SetDirty(selectedTheme);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("[Chapter2StageTemplateFactory] Chapter 2 sample stage templates created.");
    }

    private static StageTemplateData BuildStage1Template()
    {
        StageTemplateData template = ScriptableObject.CreateInstance<StageTemplateData>();
        template.stageNumber = 1;
        template.templateId = "chapter2_stage_01";
        template.templateName = "Sea Village Stage 01";
        template.stageNodeType = StageNodeType.Start;
        template.mapShapeType = StageMapShapeType.CoastalRoad;

        template.startPoint = Point(35f, -38f);
        template.exitPoint = Point(-32f, 38f);

        template.useStageBounds = true;
        template.stageBounds = Rect(Vector2.zero, new Vector2(120f, 120f));
        template.useSeaPlane = false;

        template.roadRects = new List<StageTemplateRectZone>
        {
            RectZone("road_south", new Vector2(0f, -35f), new Vector2(80f, 10f)),
            RectZone("road_west", new Vector2(-35f, 0f), new Vector2(10f, 80f)),
            RectZone("road_center", new Vector2(0f, 10f), new Vector2(80f, 10f)),
            RectZone("road_east", new Vector2(30f, 0f), new Vector2(10f, 70f)),
            RectZone("road_inner", new Vector2(-15f, -15f), new Vector2(50f, 10f))
        };

        template.buildingZones = new List<StageTemplateRectZone>
        {
            RectZone("build_west_south", new Vector2(-42f, -35f), new Vector2(18f, 28f)),
            RectZone("build_far_west", new Vector2(-58f, -5f), new Vector2(16f, 70f)),
            RectZone("build_south_west", new Vector2(-22f, -50f), new Vector2(34f, 16f)),
            RectZone("build_south_east", new Vector2(22f, -50f), new Vector2(34f, 16f)),
            RectZone("build_far_south", new Vector2(0f, -62f), new Vector2(90f, 14f)),
            RectZone("build_east_south", new Vector2(35f, -45f), new Vector2(20f, 22f)),
            RectZone("build_west_north", new Vector2(-48f, 25f), new Vector2(20f, 34f)),
            RectZone("build_north_west", new Vector2(-18f, 42f), new Vector2(34f, 16f)),
            RectZone("build_north_east", new Vector2(20f, 42f), new Vector2(34f, 16f)),
            RectZone("build_far_north", new Vector2(0f, 58f), new Vector2(90f, 14f)),
            RectZone("build_east_north", new Vector2(42f, 20f), new Vector2(22f, 38f)),
            RectZone("build_far_east", new Vector2(58f, -2f), new Vector2(16f, 70f)),
            RectZone("build_center_left", new Vector2(-18f, 22f), new Vector2(18f, 18f)),
            RectZone("build_center_right", new Vector2(18f, -2f), new Vector2(18f, 18f))
        };

        template.blockedZones = new List<StageTemplateRectZone>
        {
            RectZone("block_start", new Vector2(35f, -38f), new Vector2(16f, 16f)),
            RectZone("block_exit", new Vector2(-32f, 38f), new Vector2(16f, 16f)),
            RectZone("block_center_crossroad", new Vector2(0f, 0f), new Vector2(18f, 18f))
        };

        return template;
    }

    private static StageTemplateData BuildStage2Template()
    {
        StageTemplateData template = ScriptableObject.CreateInstance<StageTemplateData>();
        template.stageNumber = 2;
        template.templateId = "chapter2_stage_02";
        template.templateName = "Sea Village Stage 02";
        template.stageNodeType = StageNodeType.NormalBattle;
        template.mapShapeType = StageMapShapeType.VillageCenter;

        template.startPoint = Point(8f, -45f);
        template.exitPoint = Point(-28f, 45f);
        template.combatPoints.Add(Point(-8f, -5f, 2f));
        template.combatPoints.Add(Point(10f, 25f, 2f));
        template.rewardPoints.Add(Point(-22f, 18f, 2f));

        template.useStageBounds = true;
        template.stageBounds = Rect(Vector2.zero, new Vector2(120f, 120f));
        template.useSeaPlane = false;

        template.roadRects = new List<StageTemplateRectZone>
        {
            RectZone("road_start", new Vector2(-35f, -20f), new Vector2(35f, 10f)),
            RectZone("road_south", new Vector2(0f, -20f), new Vector2(70f, 10f)),
            RectZone("road_east", new Vector2(38f, -5f), new Vector2(10f, 45f)),
            RectZone("road_center", new Vector2(0f, 8f), new Vector2(70f, 10f)),
            RectZone("road_parking_west", new Vector2(-20f, 24f), new Vector2(10f, 34f)),
            RectZone("road_parking_east", new Vector2(18f, 24f), new Vector2(10f, 34f))
        };

        template.buildingZones = new List<StageTemplateRectZone>
        {
            RectZone("build_west", new Vector2(-48f, 20f), new Vector2(18f, 45f)),
            RectZone("build_south_west", new Vector2(-20f, -42f), new Vector2(40f, 18f)),
            RectZone("build_south_east", new Vector2(20f, -42f), new Vector2(40f, 18f)),
            RectZone("build_east", new Vector2(48f, 28f), new Vector2(18f, 38f)),
            RectZone("build_north", new Vector2(0f, 45f), new Vector2(80f, 16f))
        };

        template.parkingZones = new List<StageTemplateRectZone>
        {
            RectZone("parking_main", new Vector2(-4f, 24f), new Vector2(32f, 20f))
        };

        template.blockedZones = new List<StageTemplateRectZone>
        {
            RectZone("block_start", new Vector2(-48f, -22f), new Vector2(14f, 14f)),
            RectZone("block_exit", new Vector2(42f, -6f), new Vector2(14f, 14f)),
            RectZone("block_parking", new Vector2(-4f, 24f), new Vector2(36f, 24f))
        };

        return template;
    }

    private static StageTemplateData CreateOrReplaceTemplate(string fileName, StageTemplateData template)
    {
        string path = $"{TemplateFolder}/{fileName}";
        StageTemplateData existing = AssetDatabase.LoadAssetAtPath<StageTemplateData>(path);

        if (existing != null)
            AssetDatabase.DeleteAsset(path);

        AssetDatabase.CreateAsset(template, path);
        return template;
    }

    private static void AssignTemplate(ChapterThemeData theme, StageTemplateData template)
    {
        if (theme.stageTemplates == null)
            theme.stageTemplates = new List<StageTemplateData>();

        for (int i = 0; i < theme.stageTemplates.Count; i++)
        {
            if (theme.stageTemplates[i] != null &&
                theme.stageTemplates[i].stageNumber == template.stageNumber)
            {
                theme.stageTemplates[i] = template;
                return;
            }
        }

        theme.stageTemplates.Add(template);
    }

    private static StageTemplatePoint Point(float x, float z, float randomRadius = 0f)
    {
        return new StageTemplatePoint(new Vector3(x, 0f, z))
        {
            randomRadius = randomRadius
        };
    }

    private static StageTemplateRoadSegment Segment(
        StageTemplatePoint from,
        StageTemplatePoint to,
        int pointCount
    )
    {
        return new StageTemplateRoadSegment
        {
            from = from,
            to = to,
            pointCount = pointCount
        };
    }

    private static StageTemplateRect Rect(Vector2 center, Vector2 size)
    {
        return new StageTemplateRect(center, size);
    }

    private static StageTemplateRectZone RectZone(
        string zoneId,
        Vector2 center,
        Vector2 size,
        float randomJitter = 0f
    )
    {
        return new StageTemplateRectZone
        {
            enabled = true,
            zoneId = zoneId,
            rect = Rect(center, size),
            randomJitter = randomJitter
        };
    }

    private static void EnsureFolder(string parent, string child)
    {
        string path = $"{parent}/{child}";

        if (!AssetDatabase.IsValidFolder(path))
            AssetDatabase.CreateFolder(parent, child);
    }
}
#endif
