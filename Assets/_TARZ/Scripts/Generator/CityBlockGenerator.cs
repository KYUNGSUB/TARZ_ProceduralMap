using System.Collections.Generic;
using UnityEngine;

public class CityBlockGenerator : MonoBehaviour
{
    public void Generate(MapContext context)
    {
        if (context.theme.districtThemeRule == null)
        {
            Debug.LogWarning("DistrictThemeRule is missing.");
            return;
        }

        HashSet<Vector2Int> roadSet = new HashSet<Vector2Int>(context.roadGridPositions);

        foreach (Vector2Int roadGrid in context.roadGridPositions)
        {
            Vector3 roadWorld = context.GridToWorld(roadGrid);

            Vector3 roadDirection = EstimateRoadDirection(roadGrid, roadSet);
            Vector3 left = Vector3.Cross(Vector3.up, roadDirection).normalized;
            Vector3 right = -left;

            TryCreateBlock(context, roadWorld, left);
            TryCreateBlock(context, roadWorld, right);
        }

        Debug.Log($"City Blocks generated: {context.cityBlocks.Count}");
    }

    private void TryCreateBlock(MapContext context, Vector3 roadWorld, Vector3 sideDirection)
    {
        DistrictThemeRule rule = context.theme.districtThemeRule;

        if (context.random.NextDouble() > rule.blockCreationChance)
            return;

        for (int i = 0; i < rule.maxTryPerRoad; i++)
        {
            Vector2 size = new Vector2(
                RandomRange(context, rule.minBlockSize.x, rule.maxBlockSize.x),
                RandomRange(context, rule.minBlockSize.y, rule.maxBlockSize.y)
            );

            float offset = rule.blockOffsetFromRoad + RandomRange(context, 0f, 8f);

            Vector3 center = roadWorld + sideDirection.normalized * offset;

            CityBlockType type = context.theme.districtThemeRule.PickBlockType(context.random);
            CityBlock block = new CityBlock(type, center, size);

            if (!CanPlaceBlock(context, block))
                continue;

            context.cityBlocks.Add(block);
            context.blockBounds.Add(block.bounds);

            return;
        }
    }

    private bool CanPlaceBlock(MapContext context, CityBlock block)
    {
        DistrictThemeRule rule = context.theme.districtThemeRule;

        // 1. Map 영역 밖이면 금지
        if (context.hasMapBounds && !ContainsBoundsXZ(context.mapBounds, block.bounds))
            return false;

        // 2. Road와 겹치면 금지
        foreach (Bounds road in context.roadBounds)
        {
            Bounds expandedRoad = road;
            expandedRoad.Expand(new Vector3(rule.roadAvoidanceGap * 2f, 0f, rule.roadAvoidanceGap * 2f));

            if (IntersectsXZ(expandedRoad, block.bounds))
                return false;
        }

        // 3. 기존 Block과 겹치면 금지
        foreach (Bounds existing in context.blockBounds)
        {
            if (IntersectsXZ(existing, block.bounds))
                return false;
        }

        return true;
    }

    private bool IntersectsXZ(Bounds a, Bounds b)
    {
        bool overlapX = a.min.x <= b.max.x && a.max.x >= b.min.x;
        bool overlapZ = a.min.z <= b.max.z && a.max.z >= b.min.z;

        return overlapX && overlapZ;
    }

    private bool ContainsBoundsXZ(Bounds outer, Bounds inner)
    {
        return inner.min.x >= outer.min.x &&
               inner.max.x <= outer.max.x &&
               inner.min.z >= outer.min.z &&
               inner.max.z <= outer.max.z;
    }

    private Vector3 EstimateRoadDirection(Vector2Int grid, HashSet<Vector2Int> roadSet)
    {
        bool hasUp = roadSet.Contains(grid + Vector2Int.up);
        bool hasDown = roadSet.Contains(grid + Vector2Int.down);
        bool hasRight = roadSet.Contains(grid + Vector2Int.right);
        bool hasLeft = roadSet.Contains(grid + Vector2Int.left);

        int vertical = 0;
        if (hasUp) vertical++;
        if (hasDown) vertical++;

        int horizontal = 0;
        if (hasRight) horizontal++;
        if (hasLeft) horizontal++;

        if (horizontal > vertical)
            return Vector3.right;

        return Vector3.forward;
    }

    private float RandomRange(MapContext context, float min, float max)
    {
        return min + (float)context.random.NextDouble() * (max - min);
    }
}