using System.Collections.Generic;
using UnityEngine;

public class SeaVillageRoadBuilder : MonoBehaviour
{
    [Header("Road Prefab")]
    public GameObject roadPrefab;

    [Header("Road Settings")]
    public float roadY = 0.1f;
    public float roadBoundsHeight = 2f;
    public float roadBoundsPadding = 0.5f;

    [Header("Duplicate Check")]
    public float duplicateThreshold = 0.1f;

    [Header("Template Road Alignment")]
    public bool alignToNeighborRoads = true;
    public float neighborTolerance = 1.5f;

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

            Vector3 forward = alignToNeighborRoads
                ? GetRoadForward(context, roadPos)
                : GetRoadForward(context, i);

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

            RegisterRoadBounds(context, pos);

            count++;
        }

        Debug.Log(
            $"[SeaVillageRoadBuilder] Road Tiles Created = {count}, Skipped Duplicates = {skipped}"
        );
    }

    private void RegisterRoadBounds(MapContext context, Vector3 position)
    {
        if (context == null)
            return;

        float spacing = GetRoadSpacing(context);
        Vector3 size = new Vector3(
            spacing + roadBoundsPadding,
            roadBoundsHeight,
            spacing + roadBoundsPadding
        );

        context.roadBounds.Add(new Bounds(position, size));
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

    private Vector3 GetRoadForward(MapContext context, Vector3 current)
    {
        if (context.roadWorldPositions == null ||
            context.roadWorldPositions.Count == 0)
        {
            return Vector3.forward;
        }

        float spacing = GetRoadSpacing(context);
        bool hasLeft = HasRoadNear(context, current + Vector3.left * spacing, spacing);
        bool hasRight = HasRoadNear(context, current + Vector3.right * spacing, spacing);
        bool hasBack = HasRoadNear(context, current + Vector3.back * spacing, spacing);
        bool hasForward = HasRoadNear(context, current + Vector3.forward * spacing, spacing);

        int horizontalCount = (hasLeft ? 1 : 0) + (hasRight ? 1 : 0);
        int verticalCount = (hasBack ? 1 : 0) + (hasForward ? 1 : 0);

        if (horizontalCount > verticalCount)
            return Vector3.right;

        if (verticalCount > horizontalCount)
            return Vector3.forward;

        if (horizontalCount > 0)
            return Vector3.right;

        if (verticalCount > 0)
            return Vector3.forward;

        return Vector3.forward;
    }

    private bool HasRoadNear(MapContext context, Vector3 target, float spacing)
    {
        float tolerance = Mathf.Max(neighborTolerance, spacing * 0.25f);

        for (int i = 0; i < context.roadWorldPositions.Count; i++)
        {
            Vector3 pos = context.roadWorldPositions[i];

            if (Mathf.Abs(pos.x - target.x) <= tolerance &&
                Mathf.Abs(pos.z - target.z) <= tolerance)
            {
                return true;
            }
        }

        return false;
    }

    private float GetRoadSpacing(MapContext context)
    {
        if (context != null &&
            context.settings != null &&
            context.settings.tileSize > 0f)
        {
            return context.settings.tileSize;
        }

        return 8f;
    }

    private string GetRoadKey(Vector3 pos)
    {
        int x = Mathf.RoundToInt(pos.x / duplicateThreshold);
        int z = Mathf.RoundToInt(pos.z / duplicateThreshold);

        return $"{x}_{z}";
    }
}
