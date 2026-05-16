using System.Collections.Generic;
using UnityEngine;

public class POIPlacer : MonoBehaviour
{
    [Header("POI Radius")]
    public float startRadius = 5f;
    public float combatRadius = 7f;
    public float secretRadius = 4f;
    public float rewardRadius = 4f;
    public float bossRadius = 8f;

    [Header("Minimum Distance")]
    public float startToCombatMinDistance = 18f;
    public float combatToCombatMinDistance = 30f;
    public float combatToSecretMinDistance = 18f;
    public float combatToRewardMinDistance = 18f;
    public float secretToRewardMinDistance = 16f;
    public float bossToCombatMinDistance = 24f;
    public float startToBossMinDistance = 80f;

    [Header("Placement")]
    public float branchPOIOffset = 8f;
    public int maxSearchAttempts = 8;

    public void Place(MapContext context)
    {
        context.poiAreas.Clear();

        PlaceStart(context);
        PlaceCombatAreas(context);
        PlaceSecretAreas(context);
        PlaceRewardAreas(context);
        PlaceBoss(context);
    }

    private void PlaceStart(MapContext context)
    {
        TryPlacePOI(
            context,
            POIType.Start,
            context.theme.startPrefabs,
            context.startPosition,
            startRadius,
            "POI_Start",
            true
        );
    }

    private void PlaceCombatAreas(MapContext context)
    {
        foreach (Vector3 pos in context.combatPositions)
        {
            TryPlacePOI(
                context,
                POIType.Combat,
                context.theme.combatPrefabs,
                pos,
                combatRadius,
                "POI_Combat",
                false
            );
        }
    }

    private void PlaceSecretAreas(MapContext context)
    {
        foreach (Vector3 pos in context.secretPositions)
        {
            Vector3 candidate = FindBetterPOIPosition(context, pos, secretRadius);

            TryPlacePOI(
                context,
                POIType.Secret,
                context.theme.secretPrefabs,
                candidate,
                secretRadius,
                "POI_Secret",
                false
            );
        }
    }

    private void PlaceRewardAreas(MapContext context)
    {
        foreach (Vector3 pos in context.rewardPositions)
        {
            Vector3 candidate = FindBetterPOIPosition(context, pos, rewardRadius);

            TryPlacePOI(
                context,
                POIType.Reward,
                context.theme.rewardPrefabs,
                candidate,
                rewardRadius,
                "POI_Reward",
                false
            );
        }
    }

    private void PlaceBoss(MapContext context)
    {
        Vector3 bossPos = FindValidBossPosition(context);

        TryPlacePOI(
            context,
            POIType.Boss,
            context.theme.bossPrefabs,
            bossPos,
            bossRadius,
            "POI_Boss",
            true
        );

        context.bossPosition = bossPos;
    }

    private Vector3 FindValidBossPosition(MapContext context)
    {
        // 1순위: 기존 bossPosition 사용
        POIArea bossArea = new POIArea(POIType.Boss, context.bossPosition, bossRadius);

        if (CanPlacePOI(context, bossArea))
            return context.bossPosition;

        // 2순위: 마지막 도로부터 역순으로 검사
        for (int i = context.roadWorldPositions.Count - 1; i >= 0; i--)
        {
            Vector3 candidate = context.roadWorldPositions[i];
            POIArea testArea = new POIArea(POIType.Boss, candidate, bossRadius);

            if (CanPlacePOI(context, testArea))
                return candidate;
        }

        // 3순위: 그래도 없으면 마지막 도로에 강제 배치
        if (context.roadWorldPositions.Count > 0)
            return context.roadWorldPositions[context.roadWorldPositions.Count - 1];

        return context.bossPosition;
    }

    private bool TryPlacePOI(
        MapContext context,
        POIType type,
        List<GameObject> prefabList,
        Vector3 position,
        float radius,
        string objectName,
        bool forcePlace
    )
    {
        GameObject prefab = PrefabPicker.Pick(prefabList, context.random);

        if (prefab == null)
        {
            Debug.LogWarning($"{objectName} prefab is missing.");
            return false;
        }

        POIArea area = new POIArea(type, position, radius);

        if (!forcePlace)
        {
            if (!CanPlacePOI(context, area))
            {
                Debug.LogWarning($"{objectName} skipped because it overlaps or is too close.");
                return false;
            }
        }

        GameObject obj = Instantiate(prefab, position, Quaternion.identity, context.mapRoot);
        obj.name = objectName;

        Bounds prefabBounds = BoundsUtility.GetObjectBounds(obj);

        // 실제 Prefab Bounds도 점유 영역에 등록
        context.occupiedBounds.Add(prefabBounds);

        // 논리 POI 영역 등록
        context.poiAreas.Add(area);

        return true;
    }

    private bool CanPlacePOI(MapContext context, POIArea newArea)
    {
        foreach (POIArea existing in context.poiAreas)
        {
            float distance = Vector3.Distance(existing.center, newArea.center);
            float requiredDistance = GetRequiredDistance(existing.type, newArea.type);

            if (distance < requiredDistance)
                return false;

            if (existing.bounds.Intersects(newArea.bounds))
                return false;
        }

        foreach (Bounds occupied in context.occupiedBounds)
        {
            // 도로와 완전히 겹치는 것은 허용할 수 있지만,
            // 건물, 시설물, 기존 POI와 겹치는 것은 피하기 위함
            if (occupied.Intersects(newArea.bounds))
            {
                // 너무 엄격하면 POI가 도로 위에 놓이지 못하므로
                // 현재는 occupiedBounds 검사를 약하게 적용하지 않고 POI끼리만 강하게 검사
                // 필요하면 여기서 return false; 활성화 가능
            }
        }

        return true;
    }

    private float GetRequiredDistance(POIType a, POIType b)
    {
        if (IsPair(a, b, POIType.Start, POIType.Combat))
            return startToCombatMinDistance;

        if (IsPair(a, b, POIType.Combat, POIType.Combat))
            return combatToCombatMinDistance;

        if (IsPair(a, b, POIType.Combat, POIType.Secret))
            return combatToSecretMinDistance;

        if (IsPair(a, b, POIType.Combat, POIType.Reward))
            return combatToRewardMinDistance;

        if (IsPair(a, b, POIType.Secret, POIType.Reward))
            return secretToRewardMinDistance;

        if (IsPair(a, b, POIType.Boss, POIType.Combat))
            return bossToCombatMinDistance;

        if (IsPair(a, b, POIType.Start, POIType.Boss))
            return startToBossMinDistance;

        return 16f;
    }

    private bool IsPair(POIType a, POIType b, POIType x, POIType y)
    {
        return (a == x && b == y) || (a == y && b == x);
    }

    private Vector3 FindBetterPOIPosition(MapContext context, Vector3 basePosition, float radius)
    {
        // 기존 코드의 Vector3.forward * 5f 방식은 방향성이 고정되어 문제가 됨.
        // 여기서는 여러 방향 후보를 검사해서 가장 먼저 가능한 위치를 선택.

        Vector3[] directions =
        {
            Vector3.forward,
            Vector3.back,
            Vector3.right,
            Vector3.left,
            (Vector3.forward + Vector3.right).normalized,
            (Vector3.forward + Vector3.left).normalized,
            (Vector3.back + Vector3.right).normalized,
            (Vector3.back + Vector3.left).normalized
        };

        ShuffleDirections(directions, context.random);

        foreach (Vector3 dir in directions)
        {
            Vector3 candidate = basePosition + dir * branchPOIOffset;
            POIArea testArea = new POIArea(POIType.Secret, candidate, radius);

            if (CanPlacePOI(context, testArea))
                return candidate;
        }

        return basePosition;
    }

    private void ShuffleDirections(Vector3[] array, System.Random random)
    {
        for (int i = 0; i < array.Length; i++)
        {
            int rand = random.Next(i, array.Length);

            Vector3 temp = array[i];
            array[i] = array[rand];
            array[rand] = temp;
        }
    }
}