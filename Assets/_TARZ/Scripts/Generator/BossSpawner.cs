using UnityEngine;

public class BossSpawner : MonoBehaviour
{
    public void Spawn(MapContext context)
    {
        if (context == null || context.theme == null)
        {
            Debug.LogWarning("[BossSpawner] Context or theme is null.");
            return;
        }

        ChapterThemeData theme = context.theme;

        if (theme.chapterBossPrefab == null)
        {
            Debug.LogWarning("[BossSpawner] Chapter boss prefab is not assigned.");
            return;
        }

        Vector3 spawnPosition = GetBossSpawnPosition(context);

        GameObject boss = Instantiate(
            theme.chapterBossPrefab,
            spawnPosition,
            Quaternion.identity,
            context.runtimeRoot
        );

        boss.name = $"Boss_{theme.bossName}";

        Debug.Log($"[BossSpawner] Spawned boss: {theme.bossName} at {spawnPosition}");
    }

    private Vector3 GetBossSpawnPosition(MapContext context)
    {
        Vector3 pos = context.bossRoomPosition;
        pos.y = 0f;

        return pos;
    }
}