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
        BuildingPlacementSettings resolvedSettings = ResolvePlacementSettings(template);

        if (template.includeManualBuildingZones)
        {
            for (int i = 0; i < template.buildingZones.Count; i++)
            {
                StageTemplateRectZone zone = template.buildingZones[i];

                if (zone == null || !zone.enabled)
                    continue;

                placedCount += PlaceInZone(context, zone, prefabs, resolvedSettings);
            }
        }

        if (template.generateRoadAdjacentBuildingZones)
        {
            placedCount += PlaceInRoadAdjacentZones(
                context,
                template,
                prefabs,
                resolvedSettings
            );
        }

        Debug.Log($"[SeaVillageBuildingZonePlacer] Buildings placed: {placedCount}");
    }

    private int PlaceInZone(
        MapContext context,
        StageTemplateRectZone zone,
        List<GameObject> prefabs,
        BuildingPlacementSettings resolvedSettings
    )
    {
        int count = 0;
        float spacing = Mathf.Max(1f, resolvedSettings.spacing);

        foreach (Vector3 basePosition in zone.rect.EnumeratePoints(spacing))
        {
            if (context.random.NextDouble() > resolvedSettings.chance)
                continue;

            Vector3 position = ApplyJitter(basePosition, zone.randomJitter, context.random);
            position.y = buildingY;

            GameObject prefab = FindPlaceablePrefab(
                context,
                prefabs,
                position,
                resolvedSettings,
                out Bounds candidateBounds
            );

            if (prefab == null)
                continue;

            Quaternion rotation = faceNearestRoad
                ? GetRotationFacingNearestRoad(context, position)
                : Quaternion.Euler(0f, RandomRightAngle(context), 0f);

            GameObject building = Instantiate(prefab, position, rotation, context.mapRoot);
            building.name = $"SeaVillage_Building_{count:00}";

            Bounds actualBounds = BoundsUtility.GetObjectBounds(building);
            actualBounds.Expand(resolvedSettings.boundsPadding);

            context.buildingBounds.Add(actualBounds);
            context.occupiedBounds.Add(actualBounds);

            count++;
        }

        return count;
    }

    private int PlaceInRoadAdjacentZones(
        MapContext context,
        StageTemplateData template,
        List<GameObject> prefabs,
        BuildingPlacementSettings resolvedSettings
    )
    {
        if (template.roadRects == null || template.roadRects.Count == 0)
            return 0;

        int placedCount = 0;

        for (int i = 0; i < template.roadRects.Count; i++)
        {
            StageTemplateRectZone roadZone = template.roadRects[i];

            if (roadZone == null || !roadZone.enabled || roadZone.rect == null)
                continue;

            StageTemplateRect roadRect = roadZone.rect;
            float width = Mathf.Abs(roadRect.size.x);
            float height = Mathf.Abs(roadRect.size.y);

            if (width >= height)
            {
                placedCount += PlaceRoadSideZone(
                    context,
                    prefabs,
                    resolvedSettings,
                    CreateRectZone(
                        $"{roadZone.zoneId}_north_buildings",
                        new Vector2(
                            roadRect.center.x,
                            roadRect.center.y + height * 0.5f + template.roadAdjacentBuildingGap + template.roadAdjacentBuildingDepth * 0.5f
                        ),
                        new Vector2(width, template.roadAdjacentBuildingDepth)
                    )
                );

                placedCount += PlaceRoadSideZone(
                    context,
                    prefabs,
                    resolvedSettings,
                    CreateRectZone(
                        $"{roadZone.zoneId}_south_buildings",
                        new Vector2(
                            roadRect.center.x,
                            roadRect.center.y - height * 0.5f - template.roadAdjacentBuildingGap - template.roadAdjacentBuildingDepth * 0.5f
                        ),
                        new Vector2(width, template.roadAdjacentBuildingDepth)
                    )
                );
            }
            else
            {
                placedCount += PlaceRoadSideZone(
                    context,
                    prefabs,
                    resolvedSettings,
                    CreateRectZone(
                        $"{roadZone.zoneId}_east_buildings",
                        new Vector2(
                            roadRect.center.x + width * 0.5f + template.roadAdjacentBuildingGap + template.roadAdjacentBuildingDepth * 0.5f,
                            roadRect.center.y
                        ),
                        new Vector2(template.roadAdjacentBuildingDepth, height)
                    )
                );

                placedCount += PlaceRoadSideZone(
                    context,
                    prefabs,
                    resolvedSettings,
                    CreateRectZone(
                        $"{roadZone.zoneId}_west_buildings",
                        new Vector2(
                            roadRect.center.x - width * 0.5f - template.roadAdjacentBuildingGap - template.roadAdjacentBuildingDepth * 0.5f,
                            roadRect.center.y
                        ),
                        new Vector2(template.roadAdjacentBuildingDepth, height)
                    )
                );
            }
        }

        return placedCount;
    }

    private int PlaceRoadSideZone(
        MapContext context,
        List<GameObject> prefabs,
        BuildingPlacementSettings resolvedSettings,
        StageTemplateRectZone zone
    )
    {
        return PlaceInZone(context, zone, prefabs, resolvedSettings);
    }

    private StageTemplateRectZone CreateRectZone(string zoneId, Vector2 center, Vector2 size)
    {
        return new StageTemplateRectZone
        {
            enabled = true,
            zoneId = zoneId,
            rect = new StageTemplateRect(center, size),
            randomJitter = 0f
        };
    }

    private BuildingPlacementSettings ResolvePlacementSettings(StageTemplateData template)
    {
        BuildingPlacementSettings settings = new BuildingPlacementSettings
        {
            spacing = placementSpacing,
            chance = placementChance,
            boundsPadding = boundsPadding,
            checkRoadBounds = true,
            checkOccupiedBounds = true,
            checkExistingBuildingBounds = true,
            prefabTryCount = 1,
            preferSmallerBuildings = false
        };

        if (template != null && template.overrideBuildingPlacementSettings)
        {
            settings.spacing = template.buildingPlacementSpacing;
            settings.chance = template.buildingPlacementChance;
            settings.boundsPadding = template.buildingBoundsPadding;
            settings.checkRoadBounds = template.checkRoadBoundsForBuildings;
            settings.checkOccupiedBounds = template.checkOccupiedBoundsForBuildings;
            settings.checkExistingBuildingBounds = template.checkExistingBuildingBounds;
            settings.prefabTryCount = template.buildingPrefabTryCount;
            settings.preferSmallerBuildings = template.preferSmallerBuildings;
        }

        settings.spacing = Mathf.Max(1f, settings.spacing);
        settings.chance = Mathf.Clamp01(settings.chance);
        settings.boundsPadding = Mathf.Max(0f, settings.boundsPadding);
        settings.prefabTryCount = Mathf.Max(1, settings.prefabTryCount);

        return settings;
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

    private GameObject FindPlaceablePrefab(
        MapContext context,
        List<GameObject> prefabs,
        Vector3 position,
        BuildingPlacementSettings settings,
        out Bounds candidateBounds
    )
    {
        candidateBounds = new Bounds(position, Vector3.zero);

        if (prefabs == null || prefabs.Count == 0)
            return null;

        int tryCount = Mathf.Max(1, settings.prefabTryCount);
        List<GameObject> candidates = settings.preferSmallerBuildings
            ? GetPrefabsSortedByFootprint(prefabs)
            : prefabs;

        for (int attempt = 0; attempt < tryCount; attempt++)
        {
            GameObject prefab = settings.preferSmallerBuildings
                ? PickSmallPrefabCandidate(candidates, attempt, context.random)
                : PrefabPicker.Pick(candidates, context.random);

            if (prefab == null)
                continue;

            Bounds bounds = EstimateBounds(prefab, position);
            bounds.Expand(settings.boundsPadding);

            if (!CanPlace(context, bounds, settings))
                continue;

            candidateBounds = bounds;
            return prefab;
        }

        return null;
    }

    private GameObject PickSmallPrefabCandidate(
        List<GameObject> sortedPrefabs,
        int attempt,
        System.Random random
    )
    {
        if (sortedPrefabs == null || sortedPrefabs.Count == 0)
            return null;

        int expandingWindow = Mathf.Clamp(attempt + 1, 1, sortedPrefabs.Count);
        int index = random != null
            ? random.Next(0, expandingWindow)
            : Random.Range(0, expandingWindow);

        return sortedPrefabs[index];
    }

    private List<GameObject> GetPrefabsSortedByFootprint(List<GameObject> prefabs)
    {
        List<GameObject> sorted = new List<GameObject>();

        for (int i = 0; i < prefabs.Count; i++)
        {
            if (prefabs[i] != null)
                sorted.Add(prefabs[i]);
        }

        sorted.Sort((a, b) =>
            GetPrefabFootprint(a).CompareTo(GetPrefabFootprint(b))
        );

        return sorted;
    }

    private float GetPrefabFootprint(GameObject prefab)
    {
        if (prefab == null)
            return float.MaxValue;

        Renderer renderer = prefab.GetComponentInChildren<Renderer>();

        if (renderer == null)
            return float.MaxValue;

        Bounds bounds = renderer.bounds;
        return Mathf.Abs(bounds.size.x * bounds.size.z);
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

    private bool CanPlace(MapContext context, Bounds candidate, BuildingPlacementSettings settings)
    {
        if (settings.checkRoadBounds)
        {
            for (int i = 0; i < context.roadBounds.Count; i++)
            {
                if (candidate.Intersects(context.roadBounds[i]))
                    return false;
            }
        }

        if (settings.checkOccupiedBounds)
        {
            for (int i = 0; i < context.occupiedBounds.Count; i++)
            {
                if (candidate.Intersects(context.occupiedBounds[i]))
                    return false;
            }
        }

        if (settings.checkExistingBuildingBounds)
        {
            for (int i = 0; i < context.buildingBounds.Count; i++)
            {
                if (candidate.Intersects(context.buildingBounds[i]))
                    return false;
            }
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

    private struct BuildingPlacementSettings
    {
        public float spacing;
        public float chance;
        public float boundsPadding;
        public bool checkRoadBounds;
        public bool checkOccupiedBounds;
        public bool checkExistingBuildingBounds;
        public int prefabTryCount;
        public bool preferSmallerBuildings;
    }
}
