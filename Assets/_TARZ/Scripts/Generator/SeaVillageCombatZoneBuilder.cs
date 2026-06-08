using UnityEngine;

public class SeaVillageCombatZoneBuilder : MonoBehaviour
{
    [Header("Combat Zone Settings")]
    public float baseCombatRadius = 10f;
    public float enemySpawnRadius = 6f;

    public void Build(MapContext context)
    {
        if (context == null)
            return;

        if (context.combatPositions == null || context.combatPositions.Count == 0)
        {
            Debug.LogWarning("[SeaVillageCombatZoneBuilder] No combat positions.");
            return;
        }

        context.combatZones.Clear();
        context.enemySpawnPositions.Clear();

        int zoneCount = 0;
        int spawnCount = 0;

        int enemyCountPerCombat = GetEnemyCountPerCombat(context.selectedStage);
        float stageCombatRadius = GetCombatRadius(context.selectedStage);
        float stageEnemySpawnRadius = GetEnemySpawnRadius(context.selectedStage);

        foreach (Vector3 combatCenter in context.combatPositions)
        {
            CombatZoneArea zone =
                new CombatZoneArea(
                    combatCenter,
                    stageCombatRadius,
                    false,
                    false
                );

            context.combatZones.Add(zone);
            zoneCount++;

            for (int i = 0; i < enemyCountPerCombat; i++)
            {
                float angle = 360f / enemyCountPerCombat * i;

                Vector3 offset = new Vector3(
                    Mathf.Cos(angle * Mathf.Deg2Rad),
                    0f,
                    Mathf.Sin(angle * Mathf.Deg2Rad)
                ) * stageEnemySpawnRadius;

                Vector3 spawnPos = combatCenter + offset;

                context.enemySpawnPositions.Add(spawnPos);
                spawnCount++;
            }
        }

        Debug.Log($"[SeaVillageCombatZoneBuilder] Stage={context.selectedStage}");
        Debug.Log($"[SeaVillageCombatZoneBuilder] Combat zones added: {zoneCount}");
        Debug.Log($"[SeaVillageCombatZoneBuilder] Enemy count per combat: {enemyCountPerCombat}");
        Debug.Log($"[SeaVillageCombatZoneBuilder] Enemy spawn positions added: {spawnCount}");
    }

    private int GetEnemyCountPerCombat(int stage)
    {
        switch (stage)
        {
            case 1:
                return 4;

            case 2:
                return 6;   // 전투구역 2개면 총 12명

            case 3:
                return 5;

            case 4:
                return 5;

            case 5:
                return 6;

            case 6:
                return 8;

            default:
                return 4;
        }
    }

    private float GetCombatRadius(int stage)
    {
        switch (stage)
        {
            case 1:
                return 10f;

            case 2:
                return 15f;

            case 3:
                return 12f;

            case 4:
                return 13f;

            case 5:
                return 15f;

            case 6:
                return 18f;

            default:
                return baseCombatRadius;
        }
    }

    private float GetEnemySpawnRadius(int stage)
    {
        switch (stage)
        {
            case 1:
                return 6f;

            case 2:
                return 7f;

            case 3:
                return 8f;

            case 4:
                return 8f;

            case 5:
                return 9f;

            case 6:
                return 10f;

            default:
                return enemySpawnRadius;
        }
    }
}