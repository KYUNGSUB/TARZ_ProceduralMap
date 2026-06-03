using System.Collections.Generic;
using UnityEngine;

public class SeaVillageSidewalkBuilder : MonoBehaviour
{
    [Header("Sidewalk Prefabs")]
    public GameObject sidewalkStraightPrefab;

    [Header("Sidewalk Settings")]
    public float sidewalkOffset = 2.2f;
    public float sidewalkY = 0.16f;

    [Header("Duplicate Check")]
    public float duplicateThreshold = 0.1f;

    public void Build(MapContext context)
    {
        if (context == null)
            return;

        if (sidewalkStraightPrefab == null)
        {
            Debug.LogError("[SeaVillageSidewalkBuilder] sidewalkStraightPrefab is null.");
            return;
        }

        if (context.roadWorldPositions == null ||
            context.roadWorldPositions.Count == 0)
        {
            Debug.LogWarning("[SeaVillageSidewalkBuilder] No road positions.");
            return;
        }

        HashSet<string> createdKeys = new HashSet<string>();

        int count = 0;
        int skipped = 0;

        for (int i = 0; i < context.roadWorldPositions.Count; i++)
        {
            Vector3 roadPos = context.roadWorldPositions[i];

            Vector3 forward = GetRoadForward(context, i);

            if (forward.sqrMagnitude < 0.01f)
                forward = Vector3.forward;

            Vector3 right = Vector3.Cross(Vector3.up, forward).normalized;

            Quaternion rotation = Quaternion.LookRotation(forward);

            Vector3 leftPos = roadPos - right * sidewalkOffset;
            Vector3 rightPos = roadPos + right * sidewalkOffset;

            leftPos.y = sidewalkY;
            rightPos.y = sidewalkY;

            TryCreateSidewalk(
                context,
                leftPos,
                rotation,
                "Sidewalk_Left",
                createdKeys,
                ref count,
                ref skipped
            );

            TryCreateSidewalk(
                context,
                rightPos,
                rotation,
                "Sidewalk_Right",
                createdKeys,
                ref count,
                ref skipped
            );
        }

        Debug.Log(
            $"[SeaVillageSidewalkBuilder] Sidewalk Created = {count}, Skipped Duplicates = {skipped}"
        );
    }

    private void TryCreateSidewalk(
        MapContext context,
        Vector3 pos,
        Quaternion rotation,
        string namePrefix,
        HashSet<string> createdKeys,
        ref int count,
        ref int skipped)
    {
        string key = GetSidewalkKey(pos);

        if (createdKeys.Contains(key))
        {
            skipped++;
            return;
        }

        createdKeys.Add(key);

        GameObject sidewalk = Instantiate(
            sidewalkStraightPrefab,
            pos,
            rotation,
            context.mapRoot
        );

        sidewalk.name =
            $"{namePrefix}_{Mathf.RoundToInt(pos.x)}_{Mathf.RoundToInt(pos.z)}";

        count++;
    }

    private Vector3 GetRoadForward(MapContext context, int index)
    {
        if (context.roadWorldPositions == null ||
            context.roadWorldPositions.Count == 0)
        {
            return Vector3.forward;
        }

        Vector3 current = context.roadWorldPositions[index];

        if (index < context.roadWorldPositions.Count - 1)
        {
            Vector3 next = context.roadWorldPositions[index + 1];
            Vector3 dir = next - current;
            dir.y = 0f;

            if (dir.sqrMagnitude > 0.01f)
                return dir.normalized;
        }

        if (index > 0)
        {
            Vector3 prev = context.roadWorldPositions[index - 1];
            Vector3 dir = current - prev;
            dir.y = 0f;

            if (dir.sqrMagnitude > 0.01f)
                return dir.normalized;
        }

        return Vector3.forward;
    }

    private string GetSidewalkKey(Vector3 pos)
    {
        int x = Mathf.RoundToInt(pos.x / duplicateThreshold);
        int z = Mathf.RoundToInt(pos.z / duplicateThreshold);

        return $"{x}_{z}";
    }
}