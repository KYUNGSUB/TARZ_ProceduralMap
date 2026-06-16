using UnityEngine;

public class SeaVillageCombatZoneBuilder : MonoBehaviour
{
    [Header("Default Combat Zone Settings")]
    public float combatRadius = 10f;
    public float enemySpawnRadius = 6f;
    public int enemySpawnCount = 4;

    public void Build(MapContext context)
    {
        if (context == null)
            return;

        if (context.combatPositions == null || context.combatPositions.Count == 0)
        {
            Debug.LogWarning("[SeaVillageCombatZoneBuilder] No combat positions.");
            return;
        }

        StageTemplateData template = context.theme != null
            ? context.theme.GetStageTemplate(context.selectedStage)
            : null;

        float resolvedCombatRadius = combatRadius;
        float resolvedEnemySpawnRadius = enemySpawnRadius;
        int resolvedEnemySpawnCount = enemySpawnCount;

        if (template != null && template.overrideCombatZoneSettings)
        {
            resolvedCombatRadius = template.combatRadius;
            resolvedEnemySpawnRadius = template.enemySpawnRadius;
            resolvedEnemySpawnCount = template.enemySpawnCount;
        }

        resolvedEnemySpawnCount = Mathf.Max(0, resolvedEnemySpawnCount);

        foreach (Vector3 combatCenter in context.combatPositions)
        {
            CombatZoneArea zone = new CombatZoneArea(
                combatCenter,
                resolvedCombatRadius,
                false,
                false
            );

            context.combatZones.Add(zone);

            if (resolvedEnemySpawnCount == 0)
                continue;

            for (int i = 0; i < resolvedEnemySpawnCount; i++)
            {
                float angle = 360f / resolvedEnemySpawnCount * i;

                Vector3 offset = new Vector3(
                    Mathf.Cos(angle * Mathf.Deg2Rad),
                    0f,
                    Mathf.Sin(angle * Mathf.Deg2Rad)
                ) * resolvedEnemySpawnRadius;

                context.enemySpawnPositions.Add(combatCenter + offset);
            }
        }

        Debug.Log(
            $"[SeaVillageCombatZoneBuilder] Combat zones added: {context.combatPositions.Count}, " +
            $"Radius={resolvedCombatRadius}, SpawnRadius={resolvedEnemySpawnRadius}, SpawnCount={resolvedEnemySpawnCount}"
        );
    }
}