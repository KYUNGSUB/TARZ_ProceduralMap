using System.Collections.Generic;
using UnityEngine;

public class SeaVillageBuildingZonePlacer : MonoBehaviour
{
    [Header("Placement")]
    public float placementSpacing = 8f;
    public float buildingY = 0f;
    [Range(0f, 1f)] public float placementChance = 0.95f;
    public float boundsPadding = 1f;

    [Header("Rotation")]
    public bool faceNearestRoad = false;

    public static bool TryPlaceFromTemplate(MapContext context)
    {
        if (!HasTemplateBuildingZones(context))
            return false;

        GameObject runner = new GameObject("SeaVillageBuildingZonePlacer_Runtime");

        if (context.mapRoot != null)
            runner.transform.SetParent(context.mapRoot);

        SeaVillageBuildingZonePlacer placer = runner.AddComponent<SeaVillageBuildingZonePlacer>();
        placer.Place(context);
        return true;
    }

    public static bool HasTemplateBuildingZones(MapContext context)
    {
        if (context == null ||
            context.theme == null ||
            context.theme.chapterNumber != 2)
        {
            return false;
        }

        StageTemplateData template = context.theme.GetStageTemplate(context.selectedStage);

        return template != null &&
               template.buildingZones != null &&
               template.buildingZones.Count > 0;
    }

    public void Place(MapContext context)
    {
        if (context == null || context.theme == null)
        {
            Debug.LogWarning("[SeaVillageBuildingZonePlacer] Context or theme is null.");
            return;
        }

        StageTemplateData template = context.theme.GetStageTemplate(context.selectedStage);

        if (template == null ||
            template.buildingZones == null ||
            template.buildingZones.Count == 0)
        {
            Debug.LogWarning("[SeaVillageBuildingZonePlacer] Building zones are missing.");
            return;
        }

        List<GameObject> prefabs = GetBuildingPrefabs(context);

        if (prefabs == null || prefabs.Count == 0)
        {
            Debug.LogWarning("[SeaVillageBuildingZonePlacer] Building prefabs are missing.");
            return;
        }

        int placedCount = 0;

        for (int i = 0; i < template.buildingZones.Count; i++)
        {
            StageTemplateRectZone zone = template.buildingZones[i];

            if (zone == null || !zone.enabled)
                continue;

            placedCount += PlaceInZone(context, zone, prefabs);
        }

        Debug.Log($"[SeaVillageBuildingZonePlacer] Buildings placed: {placedCount}");
    }

    private int PlaceInZone(
        MapContext context,
        StageTemplateRectZone zone,
        List<GameObject> prefabs
    )
    {
        int count = 0;
        float spacing = Mathf.Max(1f, placementSpacing);

        foreach (Vector3 basePosition in zone.rect.EnumeratePoints(spacing))
        {
            if (context.random.NextDouble() > placementChance)
                continue;

            Vector3 position = ApplyJitter(basePosition, zone.randomJitter, context.random);
            position.y = buildingY;

            GameObject prefab = PrefabPicker.Pick(prefabs, context.random);

            if (prefab == null)
                continue;

            Bounds candidateBounds = EstimateBounds(prefab, position);
            candidateBounds.Expand(boundsPadding);

            if (!CanPlace(context, candidateBounds))
                continue;

            Quaternion rotation = faceNearestRoad
                ? GetRotationFacingNearestRoad(context, position)
                : Quaternion.Euler(0f, RandomRightAngle(context), 0f);

            GameObject building = Instantiate(prefab, position, rotation, context.mapRoot);
            building.name = $"SeaVillage_Building_{count:00}";

            Bounds actualBounds = BoundsUtility.GetObjectBounds(building);
            actualBounds.Expand(boundsPadding);

            context.buildingBounds.Add(actualBounds);
            context.occupiedBounds.Add(actualBounds);

            count++;
        }

        return count;
    }

    private List<GameObject> GetBuildingPrefabs(MapContext context)
    {
        StageMapShapeType shape = context.selectedMapShape;

        if (shape == StageMapShapeType.HarborArea &&
            context.theme.harborBuildingPrefabs != null &&
            context.theme.harborBuildingPrefabs.Count > 0)
        {
            return context.theme.harborBuildingPrefabs;
        }

        if (shape == StageMapShapeType.CoastalRoad &&
            context.theme.beachBuildingPrefabs != null &&
            context.theme.beachBuildingPrefabs.Count > 0)
        {
            return context.theme.beachBuildingPrefabs;
        }

        if (context.theme.commercialBuildingPrefabs != null &&
            context.theme.commercialBuildingPrefabs.Count > 0)
        {
            return context.theme.commercialBuildingPrefabs;
        }

        return context.theme.buildingPrefabs;
    }

    private Vector3 ApplyJitter(Vector3 position, float radius, System.Random random)
    {
        if (radius <= 0f || random == null)
            return position;

        float angle = (float)(random.NextDouble() * Mathf.PI * 2f);
        float distance = (float)random.NextDouble() * radius;

        position.x += Mathf.Cos(angle) * distance;
        position.z += Mathf.Sin(angle) * distance;
        return position;
    }

    private Bounds EstimateBounds(GameObject prefab, Vector3 position)
    {
        Bounds bounds = new Bounds(position, new Vector3(placementSpacing, 8f, placementSpacing));

        if (prefab == null)
            return bounds;

        Renderer renderer = prefab.GetComponentInChildren<Renderer>();

        if (renderer == null)
            return bounds;

        Vector3 size = renderer.bounds.size;
        size.y = Mathf.Max(size.y, 4f);
        bounds.size = size;
        return bounds;
    }

    private bool CanPlace(MapContext context, Bounds candidate)
    {
        for (int i = 0; i < context.roadBounds.Count; i++)
        {
            if (candidate.Intersects(context.roadBounds[i]))
                return false;
        }

        for (int i = 0; i < context.occupiedBounds.Count; i++)
        {
            if (candidate.Intersects(context.occupiedBounds[i]))
                return false;
        }

        for (int i = 0; i < context.buildingBounds.Count; i++)
        {
            if (candidate.Intersects(context.buildingBounds[i]))
                return false;
        }

        return true;
    }

    private Quaternion GetRotationFacingNearestRoad(MapContext context, Vector3 position)
    {
        Vector3 nearestRoad = Vector3.zero;
        float nearestDistance = float.MaxValue;

        for (int i = 0; i < context.roadWorldPositions.Count; i++)
        {
            Vector3 road = context.roadWorldPositions[i];
            float distance = Vector3.SqrMagnitude(road - position);

            if (distance < nearestDistance)
            {
                nearestDistance = distance;
                nearestRoad = road;
            }
        }

        if (nearestDistance == float.MaxValue)
            return Quaternion.Euler(0f, RandomRightAngle(context), 0f);

        Vector3 direction = nearestRoad - position;
        direction.y = 0f;

        if (direction.sqrMagnitude < 0.01f)
            return Quaternion.Euler(0f, RandomRightAngle(context), 0f);

        return Quaternion.LookRotation(direction.normalized);
    }

    private float RandomRightAngle(MapContext context)
    {
        int step = context != null && context.random != null
            ? context.random.Next(0, 4)
            : Random.Range(0, 4);

        return step * 90f;
    }
}
