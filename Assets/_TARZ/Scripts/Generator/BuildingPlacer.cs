using System.Collections.Generic;
using UnityEngine;

public class BuildingPlacer : MonoBehaviour
{
    public void Place(MapContext context)
    {
        if (context.buildingPlacementRule == null)
        {
            Debug.LogWarning("BuildingPlacementRule is missing.");
            return;
        }

        HashSet<Vector2Int> roadSet = new HashSet<Vector2Int>(context.roadGridPositions);

        foreach (Vector2Int roadGrid in context.roadGridPositions)
        {
            Vector3 roadWorld = context.GridToWorld(roadGrid);

            Vector3 roadDirection = EstimateRoadDirection(roadGrid, roadSet);
            Vector3 left = Vector3.Cross(Vector3.up, roadDirection).normalized;
            Vector3 right = -left;

            TryPlaceBuildingOnSide(context, roadWorld, roadDirection, left);
            TryPlaceBuildingOnSide(context, roadWorld, roadDirection, right);
        }
    }

    private Vector3 EstimateRoadDirection(Vector2Int grid, HashSet<Vector2Int> roadSet)
    {
        bool hasUp = roadSet.Contains(grid + Vector2Int.up);
        bool hasDown = roadSet.Contains(grid + Vector2Int.down);
        bool hasRight = roadSet.Contains(grid + Vector2Int.right);
        bool hasLeft = roadSet.Contains(grid + Vector2Int.left);

        int verticalCount = 0;
        if (hasUp) verticalCount++;
        if (hasDown) verticalCount++;

        int horizontalCount = 0;
        if (hasRight) horizontalCount++;
        if (hasLeft) horizontalCount++;

        if (horizontalCount > verticalCount)
            return Vector3.right;

        return Vector3.forward;
    }

    private void TryPlaceBuildingOnSide(
        MapContext context,
        Vector3 roadWorld,
        Vector3 roadDirection,
        Vector3 sideDirection
    )
    {
        BuildingPlacementRule rule = context.buildingPlacementRule;

        for (int i = 0; i < rule.maxTryPerRoadSide; i++)
        {
            float chance = rule.basePlacementChance * context.theme.buildingDensity;

            if (IsNearCombatZone(context, roadWorld))
                chance *= rule.combatZoneBuildingChanceMultiplier;

            if (context.random.NextDouble() > chance)
                return;

            GameObject prefab = PickBuildingPrefab(context);
            if (prefab == null)
                return;

            float halfRoadWidth = context.settings.tileSize * 0.5f;
            float buildingHalfSize = EstimatePrefabHalfSize(prefab);
            float extra = RandomRange(context, 0f, rule.randomExtraOffset);

            float offset =
                halfRoadWidth +
                buildingHalfSize +
                rule.minGapFromRoad +
                extra;

            Vector3 position = roadWorld + sideDirection.normalized * offset;

            Quaternion rotation = GetRotationFacingRoad(sideDirection);

            GameObject building = Instantiate(
                prefab,
                position,
                rotation,
                context.mapRoot
            );

            building.name = "Building";

            Bounds bounds = BoundsUtility.GetObjectBounds(building);

            if (!CanPlaceBuilding(context, bounds, position))
            {
                Destroy(building);
                continue;
            }

            context.occupiedBounds.Add(bounds);
            return;
        }
    }

    private GameObject PickBuildingPrefab(MapContext context)
    {
        if (context.theme.buildingPrefabs == null || context.theme.buildingPrefabs.Count == 0)
            return null;

        return PrefabPicker.Pick(context.theme.buildingPrefabs, context.random);
    }

    private float EstimatePrefabHalfSize(GameObject prefab)
    {
        Renderer[] renderers = prefab.GetComponentsInChildren<Renderer>();

        if (renderers == null || renderers.Length == 0)
            return 5f;

        Bounds bounds = renderers[0].bounds;

        for (int i = 1; i < renderers.Length; i++)
            bounds.Encapsulate(renderers[i].bounds);

        float maxSize = Mathf.Max(bounds.size.x, bounds.size.z);

        // Prefab Asset 상태에서는 bounds가 예상과 다를 수 있으므로 최소값 보정
        return Mathf.Max(maxSize * 0.5f, 4f);
    }

    private Quaternion GetRotationFacingRoad(Vector3 sideDirection)
    {
        // 건물의 Forward가 도로를 바라보게 함
        Vector3 lookDirection = -sideDirection.normalized;

        if (lookDirection.sqrMagnitude < 0.01f)
            return Quaternion.identity;

        return Quaternion.LookRotation(lookDirection, Vector3.up);
    }

    private bool CanPlaceBuilding(MapContext context, Bounds bounds, Vector3 position)
    {
        if (BoundsUtility.IsOverlapping(context.occupiedBounds, bounds))
            return false;

        if (IsOverlappingPOI(context, bounds))
            return false;

        if (IsTooCloseToCombatCenter(context, position))
            return false;

        return true;
    }

    private bool IsOverlappingPOI(MapContext context, Bounds bounds)
    {
        foreach (POIArea poi in context.poiAreas)
        {
            Bounds expanded = poi.bounds;
            expanded.Expand(context.buildingPlacementRule.poiExtraAvoidanceRadius);

            if (expanded.Intersects(bounds))
                return true;
        }

        return false;
    }

    private bool IsTooCloseToCombatCenter(MapContext context, Vector3 position)
    {
        float limit = context.buildingPlacementRule.combatZoneAvoidanceRadius;

        foreach (Vector3 combatPos in context.combatPositions)
        {
            Vector3 a = new Vector3(position.x, 0f, position.z);
            Vector3 b = new Vector3(combatPos.x, 0f, combatPos.z);

            if (Vector3.Distance(a, b) < limit)
                return true;
        }

        return false;
    }

    private bool IsNearCombatZone(MapContext context, Vector3 roadWorld)
    {
        float limit = context.buildingPlacementRule.combatZoneAvoidanceRadius;

        foreach (Vector3 combatPos in context.combatPositions)
        {
            Vector3 a = new Vector3(roadWorld.x, 0f, roadWorld.z);
            Vector3 b = new Vector3(combatPos.x, 0f, combatPos.z);

            if (Vector3.Distance(a, b) < limit)
                return true;
        }

        return false;
    }

    private float RandomRange(MapContext context, float min, float max)
    {
        return min + (float)context.random.NextDouble() * (max - min);
    }
}