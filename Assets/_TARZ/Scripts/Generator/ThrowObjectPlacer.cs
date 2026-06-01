using UnityEngine;

public class ThrowObjectPlacer : MonoBehaviour
{
    [Header("Placement")]
    public float zoneInnerRate = 0.75f;

    [Header("Distance Check")]
    public float minDistanceFromPOI = 3f;
    public float occupiedCheckRadius = 2.5f;

    public void Place(MapContext context)
    {
        foreach (Vector3 combatPos in context.combatPositions)
        {
            for (int i = 0; i < context.settings.throwObjectsPerCombatZone; i++)
            {
                if (context.random.NextDouble() > context.theme.throwObjectDensity)
                    continue;

                Vector3 position = GetRandomCirclePoint(context, combatPos, context.settings.throwObjectRadius);
                SpawnThrowObject(context, position);
            }
        }
    }

    private Vector3 GetRandomCirclePoint(MapContext context, Vector3 center, float radius)
    {
        float angle = (float)context.random.NextDouble() * Mathf.PI * 2f;
        float distance = (float)context.random.NextDouble() * radius;

        return center + new Vector3(
            Mathf.Cos(angle) * distance,
            0f,
            Mathf.Sin(angle) * distance
        );
    }

    private void SpawnThrowObject(MapContext context, Vector3 position)
    {
        GameObject prefab = PrefabPicker.Pick(context.theme.throwObjectPrefabs, context.random);
        if (prefab == null) return;

        GameObject obj = Instantiate(prefab, position, Quaternion.identity, context.mapRoot);
        obj.name = "ThrowObject";

        if (obj.GetComponent<ThrowObject>() == null)
            obj.AddComponent<ThrowObject>();
    }

    public void PlaceCombatThrowObjects(MapContext context)
    {
        if (context == null || context.theme == null)
        {
            Debug.LogWarning("[ThrowObjectPlacer] Context or theme is null.");
            return;
        }

        if (context.theme.throwObjectPrefabs == null ||
            context.theme.throwObjectPrefabs.Count == 0)
        {
            Debug.LogWarning("[ThrowObjectPlacer] No throw object prefabs.");
            return;
        }

        if (context.combatZones == null || context.combatZones.Count == 0)
        {
            Debug.Log("[ThrowObjectPlacer] No combat zones.");
            return;
        }

        foreach (CombatZoneArea zone in context.combatZones)
        {
            PlaceThrowObjectsInZone(context, zone);
        }
    }

    private void PlaceThrowObjectsInZone(
        MapContext context,
        CombatZoneArea zone)
    {
        int count = GetThrowObjectCount(zone, context);
        int placed = 0;
        int attempts = 0;
        int maxAttempts = count * 8;

        while (placed < count && attempts < maxAttempts)
        {
            attempts++;

            Vector3 position = GetRandomPositionInZone(context, zone);

            if (!CanPlaceThrowObject(context, position))
                continue;

            GameObject prefab = PickThrowObjectPrefab(context);

            if (prefab == null)
                continue;

            Quaternion rotation = Quaternion.Euler(
                0f,
                RandomRange(context, 0f, 360f),
                0f
            );

            GameObject obj = Instantiate(
                prefab,
                position,
                rotation,
                context.mapRoot
            );

            obj.name = "ThrowObject_Combat";

            Bounds bounds = BoundsUtility.GetObjectBounds(obj);
            context.occupiedBounds.Add(bounds);

            placed++;
        }

        Debug.Log($"[ThrowObjectPlacer] Combat throw objects placed: {placed}");
    }

    // Zone 종류별 Throw Object 개수
    private int GetThrowObjectCount(
        CombatZoneArea zone,
        MapContext context)
    {
        if (context.selectedStage == 1)
            return context.random.Next(2, 4);

        if (context.selectedStage == 2)
            return context.random.Next(4, 7);

        if (context.selectedStage == 3)
            return context.random.Next(5, 8);

        if (zone.isMidBossZone)
            return context.random.Next(6, 9);

        if (zone.isBossZone)
            return context.random.Next(8, 11);

        return context.random.Next(5, 8);
    }

    // Combat Zone 안 랜덤 위치 계산
    private Vector3 GetRandomPositionInZone(
    MapContext context,
    CombatZoneArea zone)
    {
        float angle = RandomRange(context, 0f, Mathf.PI * 2f);

        float rate = zoneInnerRate;

        if (context.selectedStageType == StageNodeType.Start)
            rate = 1.0f;

        float distance =
            Mathf.Sqrt((float)context.random.NextDouble())
            * zone.radius
            * rate;

        Vector3 offset = new Vector3(
            Mathf.Cos(angle) * distance,
            0f,
            Mathf.Sin(angle) * distance
        );

        Vector3 position = zone.center + offset;
        position.y = 0f;

        return position;
    }

    // 배치 가능 여부 검사
    private bool CanPlaceThrowObject(
    MapContext context,
    Vector3 position)
    {
        if (context.hasMapBounds && !context.mapBounds.Contains(position))
            return false;

        // Stage 1 튜토리얼 전투는 도로 위 배치를 허용
        if (context.selectedStageType != StageNodeType.Start)
        {
            if (IsTooCloseToRoadCenter(context, position))
                return false;
        }

        // Stage 1에서는 POI와 조금 가까워도 허용
        if (context.selectedStageType != StageNodeType.Start)
        {
            if (IsTooCloseToPOI(context, position))
                return false;
        }

        // Stage 1에서는 점유 영역 검사도 완화
        if (context.selectedStageType != StageNodeType.Start)
        {
            if (IsTooCloseToExistingOccupied(context, position))
                return false;
        }

        return true;
    }

    // 도로 중심과 너무 가까운지 검사
    private bool IsTooCloseToRoadCenter(
        MapContext context,
        Vector3 position)
    {
        float minDistance = context.settings.tileSize * 0.25f;

        foreach (Vector3 roadPos in context.roadWorldPositions)
        {
            if (Vector3.Distance(roadPos, position) < minDistance)
                return true;
        }

        return false;
    }

    // POI와 너무 가까운지 검사
    private bool IsTooCloseToPOI(
        MapContext context,
        Vector3 position)
    {
        if (context.poiAreas == null)
            return false;

        float minDistance = 3f;

        foreach (POIArea poi in context.poiAreas)
        {
            if (Vector3.Distance(poi.center, position) < minDistance)
                return true;
        }

        return false;
    }

    // 기존 점유 영역과 겹침 검사
    private bool IsTooCloseToExistingOccupied(
        MapContext context,
        Vector3 position)
    {
        float checkRadius = 2.5f;

        Bounds testBounds = new Bounds(
            position,
            new Vector3(checkRadius, 3f, checkRadius)
        );

        foreach (Bounds occupied in context.occupiedBounds)
        {
            if (occupied.Intersects(testBounds))
                return true;
        }

        return false;
    }

    // Prefab 선택
    private GameObject PickThrowObjectPrefab(MapContext context)
    {
        if (context.theme.throwObjectPrefabs == null ||
            context.theme.throwObjectPrefabs.Count == 0)
        {
            return null;
        }

        int index = context.random.Next(
            0,
            context.theme.throwObjectPrefabs.Count
        );

        return context.theme.throwObjectPrefabs[index];
    }

    // RandomRange 유틸 함수
    private float RandomRange(
        MapContext context,
        float min,
        float max)
    {
        return Mathf.Lerp(
            min,
            max,
            (float)context.random.NextDouble()
        );
    }

    public void PlaceTutorialThrowObjects(MapContext context)
    {
        if (context == null || context.theme == null)
            return;

        if (context.selectedStageType != StageNodeType.Start)
            return;

        if (context.theme.throwObjectPrefabs == null ||
            context.theme.throwObjectPrefabs.Count == 0)
        {
            Debug.LogWarning("[ThrowObjectPlacer] No throw object prefabs.");
            return;
        }

        int count = context.random.Next(2, 4); // 2~3개
        int placed = 0;
        int attempts = 0;
        int maxAttempts = count * 10;

        while (placed < count && attempts < maxAttempts)
        {
            attempts++;

            Vector3 position = GetTutorialThrowObjectPosition(context, placed);

            GameObject prefab = PickThrowObjectPrefab(context);

            if (prefab == null)
                continue;

            Quaternion rotation = Quaternion.Euler(
                0f,
                RandomRange(context, 0f, 360f),
                0f
            );

            GameObject obj = Instantiate(
                prefab,
                position,
                rotation,
                context.mapRoot
            );

            obj.name = "ThrowObject_Tutorial";

            Bounds bounds = BoundsUtility.GetObjectBounds(obj);
            context.occupiedBounds.Add(bounds);

            placed++;
        }

        Debug.Log($"[ThrowObjectPlacer] Tutorial throw objects placed: {placed}");
    }

    private Vector3 GetTutorialThrowObjectPosition(
    MapContext context,
    int index)
    {
        // Combat Zone보다 앞쪽: Road 진행률 35~45% 구간
        float rate = 0.35f + index * 0.06f;

        int roadIndex = Mathf.Clamp(
            Mathf.RoundToInt((context.roadWorldPositions.Count - 1) * rate),
            0,
            context.roadWorldPositions.Count - 1
        );

        Vector3 roadPos = context.roadWorldPositions[roadIndex];

        Vector3 forward = GetRoadForward(context, roadIndex);
        Vector3 right = Vector3.Cross(Vector3.up, forward).normalized;

        float sideOffset =
            Mathf.Lerp(
                -context.settings.tileSize * 0.25f,
                context.settings.tileSize * 0.25f,
                (float)context.random.NextDouble()
            );

        Vector3 position = roadPos + right * sideOffset;
        position.y = 0f;

        return position;
    }

    private Vector3 GetRoadForward(MapContext context, int index)
    {
        if (context.roadWorldPositions == null ||
            context.roadWorldPositions.Count <= 1)
        {
            return Vector3.forward;
        }

        if (index <= 0)
        {
            return (
                context.roadWorldPositions[1] -
                context.roadWorldPositions[0]
            ).normalized;
        }

        if (index >= context.roadWorldPositions.Count - 1)
        {
            return (
                context.roadWorldPositions[index] -
                context.roadWorldPositions[index - 1]
            ).normalized;
        }

        Vector3 prev =
            context.roadWorldPositions[index] -
            context.roadWorldPositions[index - 1];

        Vector3 next =
            context.roadWorldPositions[index + 1] -
            context.roadWorldPositions[index];

        return (prev + next).normalized;
    }
}