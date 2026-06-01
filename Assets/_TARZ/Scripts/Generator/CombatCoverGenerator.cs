using UnityEngine;

public class CombatCoverGenerator : MonoBehaviour
{
    [Header("Cover Prefabs")]
    public GameObject[] coverPrefabs;

    [Header("Cover Count")]
    public int minCoverPerZone = 3;
    public int maxCoverPerZone = 5;

    [Header("Placement")]
    public float innerRadiusRate = 0.45f;
    public float outerRadiusRate = 0.85f;

    [Header("Validation")]
    public float minDistanceFromPOI = 4f;
    public float minDistanceFromRoadCenter = 3f;
    public float occupiedCheckSize = 2.5f;

    [Header("Scale")]
    public float minScale = 0.9f;
    public float maxScale = 1.2f;

    public void Generate(MapContext context)
    {
        if (context == null || context.combatZones == null)
            return;

        if (coverPrefabs == null || coverPrefabs.Length == 0)
        {
            Debug.LogWarning("[CombatCoverGenerator] Cover prefabs are missing.");
            return;
        }

        int totalPlaced = 0;

        foreach (CombatZoneArea zone in context.combatZones)
        {
            totalPlaced += GenerateCoverInZone(context, zone);
        }

        Debug.Log($"[CombatCoverGenerator] Covers placed: {totalPlaced}");
    }

    private int GenerateCoverInZone(MapContext context, CombatZoneArea zone)
    {
        int targetCount = context.random.Next(minCoverPerZone, maxCoverPerZone + 1);

        if (context.selectedStage == 3)
            targetCount += 2;

        if (zone.isMidBossZone)
            targetCount += 2;

        if (zone.isBossZone)
            targetCount += 4;

        int placed = 0;
        int attempts = 0;
        int maxAttempts = targetCount * 10;

        while (placed < targetCount && attempts < maxAttempts)
        {
            attempts++;

            Vector3 position = GetCoverPosition(context, zone);

            if (!CanPlaceCover(context, position))
                continue;

            GameObject prefab = PickCoverPrefab(context);

            if (prefab == null)
                continue;

            Quaternion rotation = Quaternion.Euler(
                0f,
                RandomRange(context, 0f, 360f),
                0f
            );

            GameObject cover = Instantiate(
                prefab,
                position,
                rotation,
                context.mapRoot
            );

            cover.name = $"CombatCover_{Mathf.RoundToInt(position.x)}_{Mathf.RoundToInt(position.z)}";

            float scale = RandomRange(context, minScale, maxScale);
            cover.transform.localScale *= scale;

            Bounds bounds = BoundsUtility.GetObjectBounds(cover);
            context.occupiedBounds.Add(bounds);

            placed++;
        }

        return placed;
    }

    private Vector3 GetCoverPosition(MapContext context, CombatZoneArea zone)
    {
        float angle = RandomRange(context, 0f, Mathf.PI * 2f);

        float distance = RandomRange(
            context,
            zone.radius * innerRadiusRate,
            zone.radius * outerRadiusRate
        );

        Vector3 offset = new Vector3(
            Mathf.Cos(angle) * distance,
            0f,
            Mathf.Sin(angle) * distance
        );

        Vector3 position = zone.center + offset;
        position.y = 0f;

        return position;
    }

    private bool CanPlaceCover(MapContext context, Vector3 position)
    {
        if (context.hasMapBounds && !context.mapBounds.Contains(position))
            return false;

        if (IsTooCloseToPOI(context, position))
            return false;

        if (IsTooCloseToRoadCenter(context, position))
            return false;

        if (IsOverlappingOccupied(context, position))
            return false;

        return true;
    }

    private bool IsTooCloseToPOI(MapContext context, Vector3 position)
    {
        if (context.poiAreas == null)
            return false;

        foreach (POIArea poi in context.poiAreas)
        {
            if (Vector3.Distance(poi.center, position) < minDistanceFromPOI)
                return true;
        }

        return false;
    }

    private bool IsTooCloseToRoadCenter(MapContext context, Vector3 position)
    {
        if (context.roadWorldPositions == null)
            return false;

        foreach (Vector3 roadPos in context.roadWorldPositions)
        {
            if (Vector3.Distance(roadPos, position) < minDistanceFromRoadCenter)
                return true;
        }

        return false;
    }

    private bool IsOverlappingOccupied(MapContext context, Vector3 position)
    {
        Bounds testBounds = new Bounds(
            position,
            new Vector3(occupiedCheckSize, 3f, occupiedCheckSize)
        );

        foreach (Bounds occupied in context.occupiedBounds)
        {
            if (occupied.Intersects(testBounds))
                return true;
        }

        return false;
    }

    private GameObject PickCoverPrefab(MapContext context)
    {
        int index = context.random.Next(0, coverPrefabs.Length);
        return coverPrefabs[index];
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