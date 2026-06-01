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

        if (context.selectedStageType == StageNodeType.MidBoss)
        {
            SpawnMidBossStageEnemies(context);
            return;
        }

        SpawnNormalStageEnemies(context);
    }

    private void SpawnNormalStageEnemies(MapContext context)
    {
        ChapterThemeData theme = context.theme;

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

        int enemyCount = context.random.Next(2, 4); // 2~3마리
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

    private void SpawnMidBossStageEnemies(MapContext context)
    {
        CombatZoneArea midBossZone = FindMidBossZone(context);

        if (midBossZone == null)
        {
            Debug.LogWarning("[EnemySpawnRuleApplier] MidBoss zone not found.");
            SpawnNormalStageEnemies(context);
            return;
        }

        SpawnMidBossEnemy(context, midBossZone);

        SpawnSupportEnemiesAroundMidBoss(context, midBossZone);

        Debug.Log("[EnemySpawnRuleApplier] MidBoss stage enemies spawned.");
    }

    private CombatZoneArea FindMidBossZone(MapContext context)
    {
        if (context.combatZones == null)
            return null;

        foreach (CombatZoneArea zone in context.combatZones)
        {
            if (zone.isMidBossZone)
                return zone;
        }

        return null;
    }

    private void SpawnMidBossEnemy(
    MapContext context,
    CombatZoneArea zone)
    {
        EnemySpawnRule rule = FindRuleByName(context, "Zombie_MidBoss");

        if (rule == null)
        {
            Debug.LogWarning("[EnemySpawnRuleApplier] Zombie_MidBoss rule not found.");
            return;
        }

        if (rule.enemyPrefab == null)
        {
            Debug.LogWarning("[EnemySpawnRuleApplier] MidBoss enemy prefab is null.");
            return;
        }

        Vector3 spawnPos = zone.center;
        spawnPos.y = 0f;

        GameObject enemy = Instantiate(
            rule.enemyPrefab,
            spawnPos + Vector3.up,
            Quaternion.identity,
            context.runtimeRoot
        );

        enemy.name = "Enemy_MidBoss";

        ApplyMidBossStats(enemy);
    }

    private void ApplyMidBossStats(GameObject enemy)
    {
        if (enemy == null)
            return;

        enemy.transform.localScale *= 1.8f;

        // 임시 MidBoss 표현: 크기만 확대
        enemy.transform.localScale *= 1.8f;

        // 나중에 EnemyHealth, EnemyAI, Damageable 같은 컴포넌트가 생기면
        // 여기에서 체력/공격력/이동속도를 조정하면 됩니다.

        /*
        Enemy enemyComponent = enemy.GetComponent<Enemy>();

        if (enemyComponent != null)
        {
            enemyComponent.maxHealth *= 5;
            enemyComponent.currentHealth = enemyComponent.maxHealth;
            enemyComponent.moveSpeed *= 0.85f;
            enemyComponent.attackDamage *= 2;
        }
        */
    }

    private void SpawnSupportEnemiesAroundMidBoss(
    MapContext context,
    CombatZoneArea zone)
    {
        int supportCount = context.random.Next(4, 7); // 4~6마리
        int placed = 0;
        int attempts = 0;
        int maxAttempts = supportCount * 8;

        while (placed < supportCount && attempts < maxAttempts)
        {
            attempts++;

            EnemySpawnRule rule = PickSupportEnemyRule(context);

            if (rule == null || rule.enemyPrefab == null)
                continue;

            Vector3 pos = GetRandomPositionAroundZone(context, zone, 0.45f, 0.85f);

            GameObject enemy = Instantiate(
                rule.enemyPrefab,
                pos + Vector3.up,
                Quaternion.identity,
                context.runtimeRoot
            );

            enemy.name = $"Enemy_Support_{rule.enemyName}";
            placed++;
        }

        Debug.Log($"[EnemySpawnRuleApplier] MidBoss support enemies spawned: {placed}");
    }

    private EnemySpawnRule PickSupportEnemyRule(MapContext context)
    {
        if (context.theme.enemySpawnRules == null)
            return null;

        EnemySpawnRule fallback = null;

        foreach (EnemySpawnRule rule in context.theme.enemySpawnRules)
        {
            if (rule == null || rule.enemyPrefab == null)
                continue;

            if (rule.enemyName == "Zombie_MidBoss")
                continue;

            if (context.selectedStage < rule.minStage ||
                context.selectedStage > rule.maxStage)
                continue;

            if (fallback == null)
                fallback = rule;

            if (rule.roleType == EnemyRoleType.Rush ||
                rule.roleType == EnemyRoleType.FastRush ||
                rule.roleType == EnemyRoleType.Ranged)
            {
                return rule;
            }
        }

        return fallback;
    }

    private Vector3 GetRandomPositionAroundZone(
    MapContext context,
    CombatZoneArea zone,
    float innerRate,
    float outerRate)
    {
        float angle = RandomRange(context, 0f, Mathf.PI * 2f);

        float distance = RandomRange(
            context,
            zone.radius * innerRate,
            zone.radius * outerRate
        );

        Vector3 offset = new Vector3(
            Mathf.Cos(angle) * distance,
            0f,
            Mathf.Sin(angle) * distance
        );

        Vector3 pos = zone.center + offset;
        pos.y = 0f;

        return pos;
    }

    private EnemySpawnRule FindRuleByName(
    MapContext context,
    string enemyName)
    {
        if (context.theme.enemySpawnRules == null)
            return null;

        foreach (EnemySpawnRule rule in context.theme.enemySpawnRules)
        {
            if (rule == null)
                continue;

            if (rule.enemyName == enemyName)
                return rule;
        }

        return null;
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
}