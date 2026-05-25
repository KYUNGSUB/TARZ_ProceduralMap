using UnityEngine;

public class CombatZoneGenerator : MonoBehaviour
{
    [Header("Combat Zone Radius")]
    public float normalCombatRadius = 10f;
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
            GetRoadPositionByRate(context, 0.35f),
            normalCombatRadius
        );

        AddCombatZone(
            context,
            GetRoadPositionByRate(context, 0.70f),
            normalCombatRadius
        );
    }

    private void GenerateRewardCombatZone(MapContext context)
    {
        Vector3 pos = GetRoadPositionByRate(context, 0.55f);

        AddCombatZone(
            context,
            pos,
            normalCombatRadius
        );

        context.rewardPositions.Add(
            pos + new Vector3(0f, 0f, context.settings.tileSize * 0.5f)
        );
    }

    private void GenerateMidBossCombatZone(MapContext context)
    {
        Vector3 pos = context.midBossPosition;

        if (pos == Vector3.zero)
            pos = GetRoadPositionByRate(context, 0.75f);

        AddCombatZone(
            context,
            pos,
            midBossRadius,
            false,
            true
        );

        context.midBossPosition = pos;
    }

    private void GenerateSecretEntranceCombatZone(MapContext context)
    {
        Vector3 pos = GetRoadPositionByRate(context, 0.55f);

        AddCombatZone(
            context,
            pos,
            normalCombatRadius
        );

        if (context.secretRoomPosition != Vector3.zero)
        {
            context.secretPositions.Add(context.secretRoomPosition);
        }
        else
        {
            context.secretPositions.Add(
                pos + new Vector3(context.settings.tileSize, 0f, -context.settings.tileSize)
            );
        }
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

        if (context.combatZones != null)
        {
            context.combatZones.Add(
                new CombatZoneArea(center, radius, isBossZone, isMidBossZone)
            );
        }

        Bounds bounds = new Bounds(
            center,
            new Vector3(radius * 2f, 4f, radius * 2f)
        );

        context.occupiedBounds.Add(bounds);
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