using UnityEngine;

public class EnemySpawnRuleApplier : MonoBehaviour
{
    public void Apply(MapContext context)
    {
        if (context == null || context.theme == null)
        {
            Debug.LogWarning("[EnemySpawnRuleApplier] Context or theme is null.");
            return;
        }

        ChapterThemeData theme = context.theme;

        if (theme.enemySpawnRules == null || theme.enemySpawnRules.Count == 0)
        {
            Debug.LogWarning("[EnemySpawnRuleApplier] EnemySpawnRules are empty. Legacy enemyPrefab may be used.");
            return;
        }

        if (context.selectedStageType == StageNodeType.Start)
        {
            SpawnTutorialEnemies(context);
            return;
        }

        foreach (EnemySpawnRule rule in theme.enemySpawnRules)
        {
            if (rule == null)
                continue;

            if (rule.enemyPrefab == null)
            {
                Debug.LogWarning($"[EnemySpawnRuleApplier] Enemy prefab missing: {rule.enemyName}");
                continue;
            }

            Debug.Log(
                $"[EnemySpawnRuleApplier] " +
                $"Enemy={rule.enemyName}, " +
                $"Role={rule.roleType}, " +
                $"Stage={rule.minStage}-{rule.maxStage}, " +
                $"Count={rule.minCount}-{rule.maxCount}, " +
                $"Weight={rule.spawnWeight}"
            );
        }
    }

    private void SpawnTutorialEnemies(MapContext context)
    {
        if (context.theme.enemySpawnRules == null ||
            context.theme.enemySpawnRules.Count == 0)
        {
            Debug.LogWarning("[EnemySpawnRuleApplier] No enemy spawn rules.");
            return;
        }

        CombatZoneArea zone = context.combatZones[0];

        int enemyCount = context.random.Next(2, 4); // 2~3¸¶¸®
        int placed = 0;
        int attempts = 0;

        while (placed < enemyCount && attempts < enemyCount * 8)
        {
            attempts++;

            EnemySpawnRule rule = PickTutorialEnemyRule(context);

            if (rule == null || rule.enemyPrefab == null)
                continue;

            Vector3 pos = GetRandomPositionInZone(context, zone, 0.7f);

            GameObject enemy = Instantiate(
                rule.enemyPrefab,
                pos + Vector3.up,
                Quaternion.identity,
                context.runtimeRoot
            );

            enemy.name = $"TutorialEnemy_{rule.enemyName}";
            placed++;
        }

        Debug.Log($"[EnemySpawnRuleApplier] Tutorial enemies spawned: {placed}");
    }

    private EnemySpawnRule PickTutorialEnemyRule(MapContext context)
    {
        foreach (EnemySpawnRule rule in context.theme.enemySpawnRules)
        {
            if (rule == null || rule.enemyPrefab == null)
                continue;

            if (rule.roleType == EnemyRoleType.Rush ||
                rule.roleType == EnemyRoleType.FastRush)
            {
                return rule;
            }
        }

        return context.theme.enemySpawnRules[0];
    }

    private Vector3 GetRandomPositionInZone(
        MapContext context,
        CombatZoneArea zone,
        float innerRate)
    {
        float angle = Mathf.Lerp(
            0f,
            Mathf.PI * 2f,
            (float)context.random.NextDouble()
        );

        float distance =
            Mathf.Sqrt((float)context.random.NextDouble())
            * zone.radius
            * innerRate;

        Vector3 offset = new Vector3(
            Mathf.Cos(angle) * distance,
            0f,
            Mathf.Sin(angle) * distance
        );

        return zone.center + offset;
    }

    public EnemySpawnRule GetWeightedRule(ChapterThemeData theme, int stageNumber)
    {
        if (theme == null || theme.enemySpawnRules == null || theme.enemySpawnRules.Count == 0)
            return null;

        float totalWeight = 0f;

        foreach (EnemySpawnRule rule in theme.enemySpawnRules)
        {
            if (rule == null || rule.enemyPrefab == null)
                continue;

            if (stageNumber < rule.minStage || stageNumber > rule.maxStage)
                continue;

            totalWeight += rule.spawnWeight;
        }

        if (totalWeight <= 0f)
            return null;

        float randomValue = Random.Range(0f, totalWeight);
        float current = 0f;

        foreach (EnemySpawnRule rule in theme.enemySpawnRules)
        {
            if (rule == null || rule.enemyPrefab == null)
                continue;

            if (stageNumber < rule.minStage || stageNumber > rule.maxStage)
                continue;

            current += rule.spawnWeight;

            if (randomValue <= current)
                return rule;
        }

        return null;
    }
}