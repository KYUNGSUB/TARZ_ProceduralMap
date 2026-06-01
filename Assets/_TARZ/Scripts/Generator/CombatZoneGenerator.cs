using UnityEngine;

public class CombatZoneGenerator : MonoBehaviour
{
    [Header("Combat Zone Radius")]
    public float smallCombatRadius = 8f;
    public float normalCombatRadius = 10f;
    public float largeCombatRadius = 12f;
    public float midBossRadius = 14f;
    public float bossRadius = 22f;

    [Header("Placement")]
    public int normalCombatCount = 2;
    public float minDistanceBetweenCombatZones = 25f;

    public void Generate(MapContext context)
    {
        if (context == null)
        {
            Debug.LogWarning("[CombatZoneGenerator] Context is null.");
            return;
        }

        context.combatPositions.Clear();

        if (context.combatZones != null)
            context.combatZones.Clear();

        switch (context.selectedStageType)
        {
            case StageNodeType.Start:
                GenerateStartStageCombat(context);
                break;

            case StageNodeType.NormalBattle:
                GenerateNormalCombatZones(context);
                break;

            case StageNodeType.ObjectReward:
                GenerateRewardCombatZone(context);
                break;

            case StageNodeType.Event:
                GenerateNormalCombatZones(context);
                break;

            case StageNodeType.MidBoss:
                GenerateMidBossCombatZone(context);
                break;

            case StageNodeType.SecretRoomEntrance:
                GenerateSecretEntranceCombatZone(context);
                break;

            case StageNodeType.BossRoom:
                GenerateBossCombatZone(context);
                break;

            default:
                GenerateNormalCombatZones(context);
                break;
        }

        Debug.Log($"[CombatZoneGenerator] Combat zones created: {context.combatPositions.Count}");
    }

    private void GenerateStartStageCombat(MapContext context)
    {
        // Stage 1: Start에서 조금 진행한 지점에 작은 튜토리얼 전투 구역 생성
        Vector3 pos = GetRoadPositionByRate(context, 0.65f);

        AddCombatZone(
            context,
            pos,
            normalCombatRadius * 0.75f,
            false,
            false
        );

        Debug.Log("[CombatZoneGenerator] Small tutorial combat zone created for Stage 1.");
    }

    private void GenerateNormalCombatZones(MapContext context)
    {
        AddCombatZone(
            context,
            GetRoadPositionByRate(context, 0.40f),
            normalCombatRadius
        );

        AddCombatZone(
            context,
            GetRoadPositionByRate(context, 0.72f),
            normalCombatRadius
        );
    }

    private void GenerateRewardCombatZone(MapContext context)
    {
        AddCombatZone(
            context,
            GetRoadPositionByRate(context, 0.25f),
            normalCombatRadius * 0.8f
        );

        AddCombatZone(
            context,
            GetRoadPositionByRate(context, 0.55f),
            normalCombatRadius
        );

        AddCombatZone(
            context,
            GetRoadPositionByRate(context, 0.78f),
            normalCombatRadius * 1.2f
        );

        Vector3 rewardPos = GetRoadPositionByRate(context, 0.65f);

        context.rewardPositions.Add(rewardPos);

        Debug.Log("[CombatZoneGenerator] Stage 3 reward combat zones created.");
    }

    private void GenerateMidBossCombatZone(MapContext context)
    {
        AddCombatZone(
            context,
            GetRoadPositionByRate(context, 0.35f),
            normalCombatRadius
        );

        Vector3 midBossPos = GetRoadPositionByRate(context, 0.70f);

        AddCombatZone(
            context,
            midBossPos,
            midBossRadius,
            false,
            true
        );

        context.midBossPosition = midBossPos;

        Vector3 rewardPos = GetRoadPositionByRate(context, 0.85f);
        context.rewardPositions.Add(rewardPos);
    }

    private void GenerateSecretEntranceCombatZone(MapContext context)
    {
        // 1. 메인 경로 중간 또는 분기 입구 근처 전투 구역
        Vector3 entranceCombatPos = GetRoadPositionByRate(context, 0.55f);

        AddCombatZone(
            context,
            entranceCombatPos,
            normalCombatRadius
        );

        // 2. Secret Room 내부 전투 구역
        if (context.secretRoomPosition != Vector3.zero)
        {
            AddSecretRoomCombatZone(
                context,
                context.secretRoomPosition,
                normalCombatRadius * 0.8f
            );

            if (!context.secretPositions.Contains(context.secretRoomPosition))
            {
                context.secretPositions.Add(context.secretRoomPosition);
            }
        }

        Debug.Log("[CombatZoneGenerator] Secret entrance and secret room combat zones created.");
    }

    private void AddSecretRoomCombatZone(
    MapContext context,
    Vector3 center,
    float radius)
    {
//        if (center == Vector3.zero)
  //          return;

        CombatZoneArea zone = new CombatZoneArea(
            center,
            radius,
            false,
            false
        );

        context.combatPositions.Add(center);

        if (context.combatZones != null)
            context.combatZones.Add(zone);

        CreateEnemySpawnPoints(context, zone);
    }

    private void GenerateBossCombatZone(MapContext context)
    {
        Vector3 pos = context.bossRoomPosition;

        if (pos == Vector3.zero)
            pos = GetRoadPositionByRate(context, 0.85f);

        AddCombatZone(
            context,
            pos,
            bossRadius,
            true,
            false
        );

        context.bossRoomPosition = pos;
    }

    private void AddCombatZone(
    MapContext context,
    Vector3 center,
    float radius,
    bool isBossZone = false,
    bool isMidBossZone = false)
    {
        if (center == Vector3.zero)
            return;

        if (!CanPlaceCombatZone(context, center, radius))
        {
            Debug.LogWarning($"[CombatZoneGenerator] Combat zone skipped: {center}");
            return;
        }

        context.combatPositions.Add(center);

        CombatZoneArea zone = new CombatZoneArea(
            center,
            radius,
            isBossZone,
            isMidBossZone
        );

        if (context.combatZones != null)
        {
            context.combatZones.Add(zone);
        }

        CreateEnemySpawnPoints(context, zone);

        Bounds bounds = new Bounds(
            center,
            new Vector3(radius * 2f, 4f, radius * 2f)
        );

        // Combat Zone은 장애물이 아니므로 occupiedBounds에 넣지 않음
        // context.occupiedBounds.Add(bounds);
    }

    private void CreateEnemySpawnPoints(
    MapContext context,
    CombatZoneArea zone)
    {
        if (context.enemySpawnPositions == null)
            return;

        int spawnCount = GetSpawnPointCount(context, zone);

        Vector3 forward = GetRoadForwardAtPosition(context, zone.center);
        Vector3 right = Vector3.Cross(Vector3.up, forward).normalized;

        for (int i = 0; i < spawnCount; i++)
        {
            float forwardBias = RandomRange(context, 0.8f, zone.radius * 1.2f);
            float sideBias = RandomRange(context, -zone.radius * 0.4f, zone.radius * 0.4f);

            Vector3 spawnPos =
                zone.center +
                forward * forwardBias +
                right * sideBias;

            spawnPos.y = 0f;

            if (!CanUseEnemySpawnPosition(context, spawnPos))
            {
                spawnPos = GetFallbackSpawnPosition(context, zone);
            }

            context.enemySpawnPositions.Add(spawnPos);
        }

        Debug.Log(
            $"[CombatZoneGenerator] Enemy spawn positions added: {spawnCount}"
        );
    }

    private bool CanUseEnemySpawnPosition(
    MapContext context,
    Vector3 position)
    {
        if (context.hasMapBounds && !context.mapBounds.Contains(position))
            return false;

        if (IsTooCloseToPOI(context, position, 5f))
            return false;

        if (IsTooCloseToRoadCenter(context, position, context.settings.tileSize * 0.15f))
            return false;

        return true;
    }

    private Vector3 GetFallbackSpawnPosition(
    MapContext context,
    CombatZoneArea zone)
    {
        float angle = RandomRange(context, 0f, Mathf.PI * 2f);
        float distance = RandomRange(context, zone.radius * 0.4f, zone.radius * 0.8f);

        Vector3 offset = new Vector3(
            Mathf.Cos(angle) * distance,
            0f,
            Mathf.Sin(angle) * distance
        );

        Vector3 pos = zone.center + offset;
        pos.y = 0f;

        return pos;
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

    private bool IsTooCloseToRoadCenter(
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

    private Vector3 GetRoadForwardAtPosition(
    MapContext context,
    Vector3 position)
    {
        if (context.roadWorldPositions == null ||
            context.roadWorldPositions.Count < 2)
        {
            return Vector3.forward;
        }

        int nearestIndex = 0;
        float nearestDistance = float.MaxValue;

        for (int i = 0; i < context.roadWorldPositions.Count; i++)
        {
            float distance = Vector3.Distance(position, context.roadWorldPositions[i]);

            if (distance < nearestDistance)
            {
                nearestDistance = distance;
                nearestIndex = i;
            }
        }

        if (nearestIndex <= 0)
        {
            return (
                context.roadWorldPositions[1] -
                context.roadWorldPositions[0]
            ).normalized;
        }

        if (nearestIndex >= context.roadWorldPositions.Count - 1)
        {
            return (
                context.roadWorldPositions[nearestIndex] -
                context.roadWorldPositions[nearestIndex - 1]
            ).normalized;
        }

        Vector3 prev =
            context.roadWorldPositions[nearestIndex] -
            context.roadWorldPositions[nearestIndex - 1];

        Vector3 next =
            context.roadWorldPositions[nearestIndex + 1] -
            context.roadWorldPositions[nearestIndex];

        return (prev + next).normalized;
    }

    private int GetSpawnPointCount(
        MapContext context,
        CombatZoneArea zone)
    {
        if (zone.isBossZone)
            return 1;

        if (zone.isMidBossZone)
            return 6;

        if (context.selectedStageType == StageNodeType.Start)
            return 3;

        return 4;
    }

    private bool CanPlaceCombatZone(MapContext context, Vector3 center, float radius)
    {
        if (context.hasMapBounds && !context.mapBounds.Contains(center))
            return false;

        foreach (Vector3 existing in context.combatPositions)
        {
            if (Vector3.Distance(existing, center) < minDistanceBetweenCombatZones)
                return false;
        }

        return true;
    }

    private Vector3 GetRoadPositionByRate(MapContext context, float rate)
    {
        if (context.roadWorldPositions == null || context.roadWorldPositions.Count == 0)
            return Vector3.zero;

        int index = Mathf.Clamp(
            Mathf.RoundToInt((context.roadWorldPositions.Count - 1) * rate),
            0,
            context.roadWorldPositions.Count - 1
        );

        return context.roadWorldPositions[index];
    }
}