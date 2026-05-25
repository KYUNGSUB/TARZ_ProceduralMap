using UnityEngine;
using UnityEngine.AI;

public class MapValidator : MonoBehaviour
{
    public float sampleDistance = 3f;
    public float maxPathDistance = 1000f;

    public bool Validate(MapContext context)
    {
        if (context.navMeshSurface == null)
        {
            Debug.LogError("NavMeshSurface is missing.");
            return false;
        }

        context.navMeshSurface.BuildNavMesh();

        if (!IsOnNavMesh(context.startPosition))
        {
            Debug.LogWarning("Start position is not on NavMesh.");
            return false;
        }

        if (!IsOnNavMesh(context.bossRoomPosition))
        {
            Debug.LogWarning("Boss position is not on NavMesh.");
            return false;
        }

        if (!HasValidPath(context.startPosition, context.bossRoomPosition))
        {
            Debug.LogWarning("No valid path from Start to Boss.");
            return false;
        }

        /*
        foreach (Vector3 spawnPos in context.enemySpawnPositions)
        {
            if (!IsOnNavMesh(spawnPos))
            {
                Debug.LogWarning($"Enemy spawn is not on NavMesh: {spawnPos}");
                return false;
            }
        }
        */

        return true;
    }

    private bool IsOnNavMesh(Vector3 position)
    {
        return NavMesh.SamplePosition(
            position,
            out NavMeshHit hit,
            sampleDistance,
            NavMesh.AllAreas
        );
    }

    private bool HasValidPath(Vector3 start, Vector3 end)
    {
        NavMeshPath path = new NavMeshPath();

        bool found = NavMesh.CalculatePath(
            start,
            end,
            NavMesh.AllAreas,
            path
        );

        if (!found)
            return false;

        if (path.status != NavMeshPathStatus.PathComplete)
            return false;

        float distance = GetPathDistance(path);

        return distance <= maxPathDistance;
    }

    private float GetPathDistance(NavMeshPath path)
    {
        float distance = 0f;

        for (int i = 1; i < path.corners.Length; i++)
        {
            distance += Vector3.Distance(path.corners[i - 1], path.corners[i]);
        }

        return distance;
    }
}