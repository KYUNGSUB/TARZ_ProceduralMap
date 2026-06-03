using System.Collections.Generic;
using UnityEngine;

public class SeaVillageRoadBuilder : MonoBehaviour
{
    [Header("Road Prefab")]
    public GameObject roadPrefab;

    [Header("Road Settings")]
    public float roadY = 0.1f;

    [Header("Duplicate Check")]
    public float duplicateThreshold = 0.1f;

    public void Build(MapContext context)
    {
        if (context == null)
            return;

        if (roadPrefab == null)
        {
            Debug.LogError("[SeaVillageRoadBuilder] Road Prefab is null.");
            return;
        }

        if (context.roadWorldPositions == null ||
            context.roadWorldPositions.Count == 0)
        {
            Debug.LogWarning("[SeaVillageRoadBuilder] No road positions.");
            return;
        }

        HashSet<string> createdKeys = new HashSet<string>();

        int count = 0;
        int skipped = 0;

        for (int i = 0; i < context.roadWorldPositions.Count; i++)
        {
            Vector3 roadPos = context.roadWorldPositions[i];

            Vector3 pos = new Vector3(
                roadPos.x,
                roadY,
                roadPos.z
            );

            string key = GetRoadKey(pos);

            if (createdKeys.Contains(key))
            {
                skipped++;
                continue;
            }

            createdKeys.Add(key);

            Vector3 forward = GetRoadForward(context, i);

            Quaternion rotation = Quaternion.identity;

            if (forward.sqrMagnitude > 0.01f)
            {
                rotation = Quaternion.LookRotation(forward);
            }

            GameObject road = Instantiate(
                roadPrefab,
                pos,
                rotation,
                context.mapRoot
            );

            road.name =
                $"Road_{Mathf.RoundToInt(pos.x)}_{Mathf.RoundToInt(pos.z)}";

            count++;
        }

        Debug.Log(
            $"[SeaVillageRoadBuilder] Road Tiles Created = {count}, Skipped Duplicates = {skipped}"
        );
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

    private string GetRoadKey(Vector3 pos)
    {
        int x = Mathf.RoundToInt(pos.x / duplicateThreshold);
        int z = Mathf.RoundToInt(pos.z / duplicateThreshold);

        return $"{x}_{z}";
    }
}