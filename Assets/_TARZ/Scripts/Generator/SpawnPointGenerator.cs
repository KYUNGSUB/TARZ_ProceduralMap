using UnityEngine;

public class SpawnPointGenerator : MonoBehaviour
{
    public void GenerateEnemySpawns(MapContext context)
    {
        foreach (Vector3 combatPos in context.combatPositions)
        {
            for (int i = 0; i < context.settings.enemiesPerCombatZone; i++)
            {
                Vector3 position = GetRandomPoint(context, combatPos, 8f);
                context.enemySpawnPositions.Add(position);

                if (context.theme.enemyPrefab != null)
                {
                    GameObject enemy = Instantiate(
                        context.theme.enemyPrefab,
                        position,
                        Quaternion.identity,
                        context.runtimeRoot
                    );

                    enemy.name = "Enemy_Zombie";
                }
            }
        }
    }

    private Vector3 GetRandomPoint(MapContext context, Vector3 center, float radius)
    {
        float angle = (float)context.random.NextDouble() * Mathf.PI * 2f;
        float distance = (float)context.random.NextDouble() * radius;

        return center + new Vector3(
            Mathf.Cos(angle) * distance,
            0f,
            Mathf.Sin(angle) * distance
        );
    }
}