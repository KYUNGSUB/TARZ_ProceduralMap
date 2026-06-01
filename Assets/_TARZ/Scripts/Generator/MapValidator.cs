using UnityEngine;
using UnityEngine.AI;

public class MapValidator : MonoBehaviour
{
    public float sampleDistance = 3f;
    public float maxPathDistance = 1000f;

    public bool Validate(MapContext context)
    {
        if (context == null)
        {
            Debug.LogError("[MapValidator] Context is null.");
            return false;
        }

        if (context.navMeshSurface == null)
        {
            Debug.LogError("[MapValidator] NavMeshSurface is missing.");
            return false;
        }

        context.navMeshSurface.BuildNavMesh();

        //if (!IsOnNavMesh(context.startPosition))
        //{
        //    Debug.LogWarning("[MapValidator] Start position is not on NavMesh.");
        //    return false;
        //}

        Vector3 targetPosition = GetValidationTargetPosition(context);
        string targetName = GetValidationTargetName(context);

        if (!TryGetNavMeshPosition(context.startPosition, out Vector3 navStart))
        {
            Debug.LogWarning($"[MapValidator] Start position is not on NavMesh. Pos={context.startPosition}");
            return false;
        }

        if (!TryGetNavMeshPosition(targetPosition, out Vector3 navTarget))
        {
            Debug.LogWarning($"[MapValidator] {targetName} position is not on NavMesh. Pos={targetPosition}");
            return false;
        }

        if (!HasValidPath(navStart, navTarget))
        {
            Debug.LogWarning(
                $"[MapValidator] No valid path from Start to {targetName}. " +
                $"Start={navStart}, Target={navTarget}"
            );
            return false;
        }

        ValidateCombatZones(context);

        return true;
    }

    private Vector3 GetValidationTargetPosition(MapContext context)
    {
        switch (context.selectedStageType)
        {
            case StageNodeType.BossRoom:
                return context.bossRoomPosition;

            case StageNodeType.SecretRoom:
            case StageNodeType.SecretRoomEntrance:
                if (context.secretRoomPosition != Vector3.zero)
                    return context.secretRoomPosition;

                return context.exitPosition;

            default:
                return context.exitPosition;
        }
    }

    private string GetValidationTargetName(MapContext context)
    {
        switch (context.selectedStageType)
        {
            case StageNodeType.BossRoom:
                return "Boss";

            case StageNodeType.SecretRoom:
            case StageNodeType.SecretRoomEntrance:
                return "SecretRoom or Exit";

            default:
                return "Exit";
        }
    }

    private void ValidateCombatZones(MapContext context)
    {
        if (context.combatZones == null || context.combatZones.Count == 0)
            return;

        foreach (CombatZoneArea zone in context.combatZones)
        {
            if (!IsOnNavMesh(zone.center))
            {
                Debug.LogWarning($"[MapValidator] Combat zone is not on NavMesh: {zone.center}");
            }
        }
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

    private bool TryGetNavMeshPosition(Vector3 position, out Vector3 navPosition)
    {
        if (NavMesh.SamplePosition(
            position,
            out NavMeshHit hit,
            sampleDistance,
            NavMesh.AllAreas))
        {
            navPosition = hit.position;
            return true;
        }

        navPosition = position;
        return false;
    }
}