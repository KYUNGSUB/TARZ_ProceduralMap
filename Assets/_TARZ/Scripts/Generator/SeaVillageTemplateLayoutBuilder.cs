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

        ApplyFallbackMapBounds(context);
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

    private static int SafeCount<T>(System.Collections.Generic.List<T> list)
    {
        return list != null ? list.Count : 0;
    }
}
