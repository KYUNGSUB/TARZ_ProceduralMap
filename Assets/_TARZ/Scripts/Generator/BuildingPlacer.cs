using System.Collections.Generic;
using UnityEngine;

public class BuildingPlacer : MonoBehaviour
{
    [Header("Stable Road Lot Placement")]
    public float roadSafetyGap = 3f;
    public float extraOffsetMin = 0f;
    public float extraOffsetMax = 4f;
    public float placementChance = 0.85f;
    public int maxTryPerSide = 3;

    public void Place(MapContext context)
    {
        HashSet<Vector2Int> roadSet = new HashSet<Vector2Int>(context.roadGridPositions);

        foreach (Vector2Int roadGrid in context.roadGridPositions)
        {
            Vector3 roadWorld = context.GridToWorld(roadGrid);
            Vector3 roadDir = EstimateRoadDirection(roadGrid, roadSet);

            Vector3 left = Vector3.Cross(Vector3.up, roadDir).normalized;
            Vector3 right = -left;

            TryPlaceOnSide(context, roadWorld, left);
            TryPlaceOnSide(context, roadWorld, right);
        }

        Debug.Log($"Buildings placed: {context.buildingBounds.Count}");
    }

    private void TryPlaceOnSide(MapContext context, Vector3 roadWorld, Vector3 sideDir)
    {
        if (context.random.NextDouble() > placementChance * context.theme.buildingDensity)
            return;

        for (int i = 0; i < maxTryPerSide; i++)
        {
            GameObject prefab = PrefabPicker.Pick(context.theme.buildingPrefabs, context.random);

            if (prefab == null)
                return;

            Vector2 footprint = GetBuildingFootprint(prefab);

            float roadHalf = context.settings.tileSize * 0.5f;
            float buildingHalf = Mathf.Max(footprint.x, footprint.y) * 0.5f;

            float offset =
                roadHalf +
                buildingHalf +
                roadSafetyGap +
                RandomRange(context, extraOffsetMin, extraOffsetMax);

            Vector3 position = roadWorld + sideDir.normalized * offset;
            position.y = 0f;

            Quaternion rotation = Quaternion.LookRotation(-sideDir.normalized, Vector3.up);

            GameObject building = Instantiate(prefab, position, rotation, context.mapRoot);
            building.name = "Building_StableLot";

            Bounds bounds = BoundsUtility.GetObjectBounds(building);

            if (!CanPlaceBuilding(context, bounds))
            {
                Destroy(building);
                continue;
            }

            context.buildingBounds.Add(bounds);
            return;
        }
    }

    private bool CanPlaceBuilding(MapContext context, Bounds buildingBounds)
    {
        if (context.hasMapBounds && !ContainsBoundsXZ(context.mapBounds, buildingBounds))
            return false;

        if (OverlapsAnyRoadTile(context, buildingBounds))
            return false;

        foreach (POIArea poi in context.poiAreas)
        {
            if (poi.type != POIType.Start && poi.type != POIType.Boss)
                continue;

            Bounds protectedBounds = poi.bounds;
            protectedBounds.Expand(new Vector3(2f, 0f, 2f));

            if (IntersectsXZ(protectedBounds, buildingBounds))
                return false;
        }

        foreach (Bounds b in context.buildingBounds)
        {
            if (IntersectsXZ(b, buildingBounds))
                return false;
        }

        return true;
    }

    private bool OverlapsAnyRoadTile(MapContext context, Bounds buildingBounds)
    {
        float gap = 0.5f;

        foreach (Vector3 roadCenter in context.roadWorldPositions)
        {
            Bounds roadBounds = new Bounds(
                roadCenter,
                new Vector3(
                    context.settings.tileSize + gap * 2f,
                    10f,
                    context.settings.tileSize + gap * 2f
                )
            );

            if (IntersectsXZ(roadBounds, buildingBounds))
                return true;
        }

        return false;
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

    private Vector2 GetBuildingFootprint(GameObject prefab)
    {
        Renderer[] renderers = prefab.GetComponentsInChildren<Renderer>();

        if (renderers == null || renderers.Length == 0)
            return new Vector2(8f, 8f);

        Bounds bounds = renderers[0].bounds;

        for (int i = 1; i < renderers.Length; i++)
            bounds.Encapsulate(renderers[i].bounds);

        return new Vector2(
            Mathf.Max(bounds.size.x, 1f),
            Mathf.Max(bounds.size.z, 1f)
        );
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

    private float RandomRange(MapContext context, float min, float max)
    {
        return min + (float)context.random.NextDouble() * (max - min);
    }
}