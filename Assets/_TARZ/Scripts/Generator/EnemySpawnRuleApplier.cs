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