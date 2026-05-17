using System.Collections.Generic;
using UnityEngine;

public class CombatZoneGenerator : MonoBehaviour
{
    [Header("Prefabs")]
    public List<GameObject> coverPrefabs = new List<GameObject>();

    public void Generate(MapContext context)
    {
        if (context.combatZoneRule == null)
        {
            Debug.LogWarning("CombatZoneRule is missing.");
            return;
        }

        foreach (Vector3 combatCenter in context.combatPositions)
        {
            GenerateCombatZone(context, combatCenter);
        }
    }

    private void GenerateCombatZone(MapContext context, Vector3 center)
    {
        PlaceCovers(context, center);
        PlaceThrowObjects(context, center);
        RegisterEnemySpawnPositions(context, center);
        AnalyzeSight(context, center);
    }

    private void PlaceCovers(MapContext context, Vector3 center)
    {
        int count = context.random.Next(
            context.combatZoneRule.minCoverCount,
            context.combatZoneRule.maxCoverCount + 1
        );

        int placed = 0;
        int maxTry = count * 10;

        for (int i = 0; i < maxTry && placed < count; i++)
        {
            Vector3 pos = GetRandomRingPoint(
                context,
                center,
                context.combatZoneRule.coverMinDistanceFromCenter,
                context.combatZoneRule.coverMaxDistanceFromCenter
            );

            if (IsInsideCenterClearArea(context, center, pos))
                continue;

            if (IsOccupied(context, pos, 2.0f))
                continue;

            GameObject prefab = PickCoverPrefab(context);

            if (prefab == null)
                return;

            Quaternion rot = Quaternion.Euler(
                0f,
                RandomRange(context, 0f, 360f),
                0f
            );

            GameObject cover = Instantiate(prefab, pos, rot, context.mapRoot);
            cover.name = "Combat_Cover";

            Bounds bounds = BoundsUtility.GetObjectBounds(cover);

            if (BoundsUtility.IsOverlapping(context.occupiedBounds, bounds))
            {
                Destroy(cover);
                continue;
            }

            context.occupiedBounds.Add(bounds);
            placed++;
        }
    }

    private void PlaceThrowObjects(MapContext context, Vector3 center)
    {
        int count = context.random.Next(
            context.combatZoneRule.minThrowObjectCount,
            context.combatZoneRule.maxThrowObjectCount + 1
        );

        int placed = 0;
        int maxTry = count * 20;

        for (int i = 0; i < maxTry && placed < count; i++)
        {
            Vector3 pos = GetRandomRingPoint(
                context,
                center,
                context.combatZoneRule.throwObjectMinDistanceFromCenter,
                context.combatZoneRule.throwObjectMaxDistanceFromCenter
            );

            // 전투 중심부는 플레이어 이동 공간으로 비워둠
            if (IsInsideCenterClearArea(context, center, pos))
                continue;

            GameObject prefab = PrefabPicker.Pick(
                context.theme.throwObjectPrefabs,
                context.random
            );

            if (prefab == null)
            {
                Debug.LogWarning("ThrowObject prefab is missing in ChapterThemeData.");
                return;
            }

            Quaternion rot = Quaternion.Euler(
                0f,
                RandomRange(context, 0f, 360f),
                0f
            );

            GameObject obj = Instantiate(prefab, pos + Vector3.up * 0.5f, rot, context.mapRoot);
            obj.name = "Combat_ThrowObject";

            if (obj.GetComponent<ThrowObject>() == null)
                obj.AddComponent<ThrowObject>();

            // ThrowObject는 Road/POI 위에 있어야 하므로 occupiedBounds 검사로 막지 않음
            // 단, 이후 다른 큰 구조물 배치를 막을 필요가 있으면 별도 throwObjectBounds로 관리하는 것이 좋음

            placed++;
        }

        Debug.Log($"ThrowObjects placed: {placed}");
    }

    private void RegisterEnemySpawnPositions(MapContext context, Vector3 center)
    {
        int count = context.random.Next(
            context.combatZoneRule.minEnemySpawnCount,
            context.combatZoneRule.maxEnemySpawnCount + 1
        );

        int placed = 0;
        int maxTry = count * 20;

        for (int i = 0; i < maxTry && placed < count; i++)
        {
            Vector3 pos = GetRandomRingPoint(
                context,
                center,
                context.combatZoneRule.enemySpawnMinDistanceFromCenter,
                context.combatZoneRule.enemySpawnMaxDistanceFromCenter
            );

            // Enemy Spawn은 Road/POI 위에 있어야 하므로 occupiedBounds 전체 검사 금지
            // 대신 중심부 너무 가까운 위치만 피함
            if (IsInsideCenterClearArea(context, center, pos))
                continue;

            context.enemySpawnPositions.Add(pos);
            placed++;
        }

        Debug.Log($"Enemy spawn positions count: {context.enemySpawnPositions.Count}");
    }

    private void AnalyzeSight(MapContext context, Vector3 center)
    {
        int blocked = 0;
        int rayCount = Mathf.Max(1, context.combatZoneRule.sightRayCount);

        for (int i = 0; i < rayCount; i++)
        {
            float angle = (360f / rayCount) * i;
            Vector3 dir = Quaternion.Euler(0f, angle, 0f) * Vector3.forward;

            Vector3 origin = center + Vector3.up * 1.5f;
            float distance = context.combatZoneRule.combatRadius;

            if (Physics.Raycast(origin, dir, distance))
            {
                blocked++;
            }
        }

        float ratio = (float)blocked / rayCount;

        if (ratio > context.combatZoneRule.maxCoverBlockingRatio)
        {
            Debug.LogWarning(
                $"Combat sight is too blocked. Center={center}, BlockedRatio={ratio}"
            );
        }
    }

    private bool IsInsideCenterClearArea(MapContext context, Vector3 center, Vector3 pos)
    {
        float distance = Vector3.Distance(
            new Vector3(center.x, 0f, center.z),
            new Vector3(pos.x, 0f, pos.z)
        );

        return distance < context.combatZoneRule.centerClearRadius;
    }

    private bool IsOccupied(MapContext context, Vector3 pos, float radius)
    {
        foreach (Bounds b in context.occupiedBounds)
        {
            if (b.SqrDistance(pos) < radius * radius)
                return true;
        }

        return false;
    }

    private Vector3 GetRandomRingPoint(
        MapContext context,
        Vector3 center,
        float minRadius,
        float maxRadius
    )
    {
        float angle = RandomRange(context, 0f, Mathf.PI * 2f);
        float distance = RandomRange(context, minRadius, maxRadius);

        return center + new Vector3(
            Mathf.Cos(angle) * distance,
            0f,
            Mathf.Sin(angle) * distance
        );
    }

    private GameObject PickCoverPrefab(MapContext context)
    {
        if (coverPrefabs != null && coverPrefabs.Count > 0)
            return PrefabPicker.Pick(coverPrefabs, context.random);

        if (context.theme.facilityPrefabs != null && context.theme.facilityPrefabs.Count > 0)
            return PrefabPicker.Pick(context.theme.facilityPrefabs, context.random);

        return null;
    }

    private float RandomRange(MapContext context, float min, float max)
    {
        return min + (float)context.random.NextDouble() * (max - min);
    }
}