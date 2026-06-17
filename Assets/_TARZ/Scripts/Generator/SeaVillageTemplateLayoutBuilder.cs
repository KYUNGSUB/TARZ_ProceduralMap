using UnityEngine;

public static class SeaVillageTemplateLayoutBuilder
{
    public static bool Build(MapContext context, StageTemplateData template)
    {
        if (context == null)
        {
            Debug.LogError("[SeaVillageTemplateLayoutBuilder] MapContext is null.");
            return false;
        }

        if (template == null)
        {
            Debug.LogWarning("[SeaVillageTemplateLayoutBuilder] StageTemplateData is null.");
            return false;
        }

        template.ApplyTo(context);

        BuildParkingZones(context, template);
        BuildDecorZones(context, template);
        ApplyFallbackMapBounds(context);
        BuildBoundaryVisual(context, template);
        ValidateTemplateResult(context, template);

        Debug.Log(
            $"[SeaVillageTemplateLayoutBuilder] Template layout built. " +
            $"Stage={template.stageNumber}, Template={template.name}, " +
            $"RoadRects={SafeCount(template.roadRects)}, " +
            $"BuildingZones={SafeCount(template.buildingZones)}, " +
            $"BlockedZones={SafeCount(template.blockedZones)}, " +
            $"ParkingZones={SafeCount(template.parkingZones)}"
        );

        return true;
    }

    private static void BuildDecorZones(MapContext context, StageTemplateData template)
    {
        if (context == null ||
            context.theme == null ||
            template == null ||
            template.decorZones == null ||
            template.decorZones.Count == 0)
        {
            return;
        }

        if (context.theme.plazaPrefab == null)
        {
            Debug.LogWarning("[SeaVillageTemplateLayoutBuilder] plazaPrefab is null.");
            return;
        }

        int createdCount = 0;

        for (int i = 0; i < template.decorZones.Count; i++)
        {
            StageTemplateRectZone zone = template.decorZones[i];

            if (zone == null || !zone.enabled)
                continue;

            Bounds bounds = zone.rect.ToBounds();
            Vector3 position = bounds.center;
            position.y = 0.015f;

            GameObject plaza = Object.Instantiate(
                context.theme.plazaPrefab,
                position,
                Quaternion.identity,
                context.mapRoot
            );

            plaza.name = string.IsNullOrEmpty(zone.zoneId)
                ? $"SeaVillage_DecorZone_{createdCount:00}"
                : $"SeaVillage_DecorZone_{zone.zoneId}";

            plaza.transform.localScale = new Vector3(
                bounds.size.x,
                1f,
                bounds.size.z
            );

            createdCount++;
        }

        Debug.Log($"[SeaVillageTemplateLayoutBuilder] Decor zones created: {createdCount}");
    }

    private static void ApplyFallbackMapBounds(MapContext context)
    {
        if (context.hasMapBounds)
            return;

        Bounds bounds = new Bounds(context.startPosition, Vector3.zero);
        bool hasAnyPoint = false;

        EncapsulatePoint(ref bounds, context.startPosition, ref hasAnyPoint);
        EncapsulatePoint(ref bounds, context.exitPosition, ref hasAnyPoint);

        for (int i = 0; i < context.roadWorldPositions.Count; i++)
            EncapsulatePoint(ref bounds, context.roadWorldPositions[i], ref hasAnyPoint);

        for (int i = 0; i < context.combatPositions.Count; i++)
            EncapsulatePoint(ref bounds, context.combatPositions[i], ref hasAnyPoint);

        for (int i = 0; i < context.rewardPositions.Count; i++)
            EncapsulatePoint(ref bounds, context.rewardPositions[i], ref hasAnyPoint);

        for (int i = 0; i < context.secretPositions.Count; i++)
            EncapsulatePoint(ref bounds, context.secretPositions[i], ref hasAnyPoint);

        if (!hasAnyPoint)
            return;

        bounds.Expand(new Vector3(24f, 4f, 24f));
        context.mapBounds = bounds;
        context.hasMapBounds = true;
    }

    private static void EncapsulatePoint(ref Bounds bounds, Vector3 point, ref bool hasAnyPoint)
    {
        if (!hasAnyPoint)
        {
            bounds = new Bounds(point, Vector3.zero);
            hasAnyPoint = true;
            return;
        }

        bounds.Encapsulate(point);
    }

    private static void ValidateTemplateResult(MapContext context, StageTemplateData template)
    {
        if (context.roadWorldPositions.Count == 0)
        {
            Debug.LogWarning(
                $"[SeaVillageTemplateLayoutBuilder] Template has no road positions. Template={template.name}"
            );
        }

        if (context.startPosition == context.exitPosition)
        {
            Debug.LogWarning(
                $"[SeaVillageTemplateLayoutBuilder] Start and Exit positions are identical. Template={template.name}"
            );
        }
    }

    private static void BuildParkingZones(MapContext context, StageTemplateData template)
    {
        if (context == null ||
            context.theme == null ||
            template == null ||
            template.parkingZones == null ||
            template.parkingZones.Count == 0)
        {
            return;
        }

        if (context.theme.parkingLotPrefab == null)
        {
            Debug.LogWarning("[SeaVillageTemplateLayoutBuilder] parkingLotPrefab is null.");
            return;
        }

        int createdCount = 0;

        for (int i = 0; i < template.parkingZones.Count; i++)
        {
            StageTemplateRectZone zone = template.parkingZones[i];

            if (zone == null || !zone.enabled)
                continue;

            Bounds bounds = zone.rect.ToBounds();
            Vector3 position = bounds.center;
            position.y = 0.02f;

            GameObject parkingLot = Object.Instantiate(
                context.theme.parkingLotPrefab,
                position,
                Quaternion.identity,
                context.mapRoot
            );

            parkingLot.name = string.IsNullOrEmpty(zone.zoneId)
                ? $"SeaVillage_ParkingLot_{createdCount:00}"
                : $"SeaVillage_ParkingLot_{zone.zoneId}";

            parkingLot.transform.localScale = new Vector3(
                bounds.size.x,
                1f,
                bounds.size.z
            );

            createdCount++;
        }

        Debug.Log($"[SeaVillageTemplateLayoutBuilder] Parking lots created: {createdCount}");
    }

    private static void BuildBoundaryVisual(MapContext context, StageTemplateData template)
    {
        if (context == null ||
            template == null ||
            !template.showBoundaryVisual ||
            !context.hasMapBounds)
        {
            return;
        }

        Transform parent = context.debugRoot != null
            ? context.debugRoot
            : context.mapRoot;

        if (parent == null)
            return;

        Bounds bounds = context.mapBounds;
        float y = template.boundaryVisualY;

        GameObject visual = new GameObject("StageTemplate_BoundaryVisual");
        visual.transform.SetParent(parent);

        LineRenderer line = visual.AddComponent<LineRenderer>();
        line.useWorldSpace = true;
        line.loop = true;
        line.positionCount = 4;
        line.startWidth = template.boundaryVisualWidth;
        line.endWidth = template.boundaryVisualWidth;
        line.startColor = template.boundaryVisualColor;
        line.endColor = template.boundaryVisualColor;

        Shader shader = Shader.Find("Sprites/Default");

        if (shader != null)
            line.material = new Material(shader);

        line.SetPosition(0, new Vector3(bounds.min.x, y, bounds.min.z));
        line.SetPosition(1, new Vector3(bounds.min.x, y, bounds.max.z));
        line.SetPosition(2, new Vector3(bounds.max.x, y, bounds.max.z));
        line.SetPosition(3, new Vector3(bounds.max.x, y, bounds.min.z));

        Debug.Log(
            $"[SeaVillageTemplateLayoutBuilder] Boundary visual created. " +
            $"Center={bounds.center}, Size={bounds.size}"
        );
    }

    private static int SafeCount<T>(System.Collections.Generic.List<T> list)
    {
        return list != null ? list.Count : 0;
    }
}
