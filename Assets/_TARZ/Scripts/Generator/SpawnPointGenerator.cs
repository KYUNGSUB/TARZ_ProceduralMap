using UnityEngine;
using UnityEngine.AI;

public class SpawnPointGenerator : MonoBehaviour
{
    public float navMeshSampleDistance = 5f;

    public void GenerateEnemySpawns(MapContext context)
    {
        Debug.Log($"SpawnPointGenerator received {context.enemySpawnPositions.Count} enemy spawn positions.");

        foreach (Vector3 spawnPos in context.enemySpawnPositions)
        {
            if (context.theme.enemyPrefab == null)
            {
                Debug.LogWarning("Enemy prefab is missing in ChapterThemeData.");
                continue;
            }

            Vector3 finalPos = spawnPos;

            if (NavMesh.SamplePosition(spawnPos, out NavMeshHit hit, navMeshSampleDistance, NavMesh.AllAreas))
            {
                finalPos = hit.position;
            }
            else
            {
                Debug.LogWarning($"Enemy spawn position is not near NavMesh: {spawnPos}");
                continue;
            }

            GameObject enemy = Instantiate(
                context.theme.enemyPrefab,
                finalPos + Vector3.up,
                Quaternion.identity,
                context.runtimeRoot
            );

            enemy.name = "Enemy_Zombie";
        }
    }
}