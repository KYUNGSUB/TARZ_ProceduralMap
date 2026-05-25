using System.Collections.Generic;
using UnityEngine;

public class DebrisClusterGenerator : MonoBehaviour
{
    [Header("Cluster Settings")]
    public int minClusterCount = 4;
    public int maxClusterCount = 10;

    [Header("Debris Per Cluster")]
    public int minDebrisPerCluster = 3;
    public int maxDebrisPerCluster = 7;

    [Header("Placement")]
    public float clusterRadius = 5f;
    public float buildingOffsetMin = 3f;
    public float buildingOffsetMax = 9f;

    [Header("Validation")]
    public float minDistanceFromRoad = 5f;
    public float minDistanceFromPOI = 6f;
    public float minDistanceBetweenClusters = 8f;

    [Header("Random Scale")]
    public float minScale = 0.8f;
    public float maxScale = 1.25f;

    private readonly List<Vector3> clusterCenters = new List<Vector3>();

    public void Generate(MapContext context)
    {
        if (context == null || context.theme == null)
        {
            Debug.LogWarning("[DebrisClusterGenerator] Context or theme is null.");
            return;
        }

        if (context.theme.debrisPrefabs == null || context.theme.debrisPrefabs.Count == 0)
        {
            Debug.LogWarning("[DebrisClusterGenerator] No debris prefabs.");
            return;
        }

        if (context.buildingBounds == null || context.buildingBounds.Count == 0)
        {
            Debug.LogWarning("[DebrisClusterGenerator] No building bounds.");
            return;
        }

        clusterCenters.Clear();

        int clusterCount = context.random.Next(minClusterCount, maxClusterCount + 1);
        int placedCount = 0;
        int attempts = 0;
        int maxAttempts = clusterCount * 12;

        while (placedCount < clusterCount && attempts < maxAttempts)
        {
            attempts++;

            Bounds building = context.buildingBounds[
                context.random.Next(0, context.buildingBounds.Count)
            ];

            Vector3 clusterCenter = GetClusterCenterNearBuilding(context, building);

            if (!CanPlaceCluster(context, clusterCenter))
                continue;

            PlaceCluster(context, clusterCenter);

            clusterCenters.Add(clusterCenter);
            placedCount++;
        }

        Debug.Log($"[DebrisClusterGenerator] Debris clusters placed: {placedCount}");
    }

    private Vector3 GetClusterCenterNearBuilding(MapContext context, Bounds building)
    {
        Vector2 randomCircle = RandomInsideCircle(context);

        Vector3 direction = new Vector3(
            randomCircle.x,
            0f,
            randomCircle.y
        ).normalized;

        if (direction.sqrMagnitude < 0.01f)
            direction = Vector3.forward;

        float distance = RandomRange(
            context,
            buildingOffsetMin,
            buildingOffsetMax
        );

        Vector3 pos = building.center + direction * distance;
        pos.y = 0f;

        return pos;
    }

    private void PlaceCluster(MapContext context, Vector3 center)
    {
        int debrisCount = context.random.Next(
            minDebrisPerCluster,
            maxDebrisPerCluster + 1
        );

        for (int i = 0; i < debrisCount; i++)
        {
            GameObject prefab = PickDebrisPrefab(context);

            if (prefab == null)
                continue;

            Vector2 randomCircle = RandomInsideCircle(context);

            Vector3 offset = new Vector3(
                randomCircle.x,
                0f,
                randomCircle.y
            ) * clusterRadius;

            Vector3 pos = center + offset;
            pos.y = 0f;

            if (!CanPlaceSingleDebris(context, pos))
                continue;

            Quaternion rot = Quaternion.Euler(
                0f,
                RandomRange(context, 0f, 360f),
                0f
            );

            GameObject debris = Instantiate(
                prefab,
                pos,
                rot,
                context.mapRoot
            );

            debris.name = "Debris_Cluster";

            float scale = RandomRange(context, minScale, maxScale);
            debris.transform.localScale *= scale;

            Bounds bounds = BoundsUtility.GetObjectBounds(debris);

            context.occupiedBounds.Add(bounds);
        }
    }

    private GameObject PickDebrisPrefab(MapContext context)
    {
        if (context.theme.debrisPrefabs == null ||
            context.theme.debrisPrefabs.Count == 0)
            return null;

        int index = context.random.Next(0, context.theme.debrisPrefabs.Count);
        return context.theme.debrisPrefabs[index];
    }

    private bool CanPlaceCluster(MapContext context, Vector3 center)
    {
        if (context.hasMapBounds && !context.mapBounds.Contains(center))
            return false;

        foreach (Vector3 existing in clusterCenters)
        {
            if (Vector3.Distance(existing, center) < minDistanceBetweenClusters)
                return false;
        }

        if (IsTooCloseToRoad(context, center, minDistanceFromRoad))
            return false;

        if (IsTooCloseToPOI(context, center, minDistanceFromPOI))
            return false;

        return true;
    }

    private bool CanPlaceSingleDebris(MapContext context, Vector3 position)
    {
        if (context.hasMapBounds && !context.mapBounds.Contains(position))
            return false;

        if (IsTooCloseToRoad(context, position, minDistanceFromRoad * 0.6f))
            return false;

        if (IsTooCloseToPOI(context, position, minDistanceFromPOI * 0.6f))
            return false;

        return true;
    }

    private bool IsTooCloseToRoad(
        MapContext context,
        Vector3 position,
        float minDistance)
    {
        if (context.roadWorldPositions == null)
            return false;

        foreach (Vector3 roadPos in context.roadWorldPositions)
        {
            if (Vector3.Distance(roadPos, position) < minDistance)
                return true;
        }

        return false;
    }

    private bool IsTooCloseToPOI(
        MapContext context,
        Vector3 position,
        float minDistance)
    {
        if (context.poiAreas == null)
            return false;

        foreach (POIArea poi in context.poiAreas)
        {
            if (Vector3.Distance(poi.center, position) < minDistance)
                return true;
        }

        return false;
    }

    private Vector2 RandomInsideCircle(MapContext context)
    {
        float angle = RandomRange(context, 0f, Mathf.PI * 2f);
        float radius = Mathf.Sqrt((float)context.random.NextDouble());

        return new Vector2(
            Mathf.Cos(angle) * radius,
            Mathf.Sin(angle) * radius
        );
    }

    private float RandomRange(MapContext context, float min, float max)
    {
        return Mathf.Lerp(
            min,
            max,
            (float)context.random.NextDouble()
        );
    }
}