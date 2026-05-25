using UnityEngine;

public class MapBoundsExpander : MonoBehaviour
{
    [Header("Minimum Map Size")]
    public float minWidth = 320f;
    public float minDepth = 110f;

    [Header("Padding")]
    public float extraPaddingX = 25f;
    public float extraPaddingZ = 25f;
    public float height = 10f;

    public void Expand(MapContext context)
    {
        if (context == null)
            return;

        bool hasBounds = false;
        Bounds bounds = new Bounds(Vector3.zero, Vector3.zero);

        AddBoundsList(ref bounds, ref hasBounds, context.roadBounds);
        AddBoundsList(ref bounds, ref hasBounds, context.buildingBounds);
        AddBoundsList(ref bounds, ref hasBounds, context.occupiedBounds);

        if (!hasBounds)
            return;

        bounds.Expand(new Vector3(extraPaddingX, height, extraPaddingZ));

        Vector3 size = bounds.size;
        size.x = Mathf.Max(size.x, minWidth);
        size.z = Mathf.Max(size.z, minDepth);
        size.y = Mathf.Max(size.y, height);

        // Road 전체 중심을 기준으로 Map 중앙 보정
        Vector3 center = GetRoadCenter(context);
        center.y = 0f;

        bounds = new Bounds(center, size);

        context.mapBounds = bounds;
        context.hasMapBounds = true;

        Debug.Log($"[MapBoundsExpander] Final MapBounds Center={bounds.center}, Size={bounds.size}");
    }

    private void AddBoundsList(ref Bounds bounds, ref bool hasBounds, System.Collections.Generic.List<Bounds> list)
    {
        if (list == null)
            return;

        foreach (Bounds b in list)
        {
            if (!hasBounds)
            {
                bounds = b;
                hasBounds = true;
            }
            else
            {
                bounds.Encapsulate(b);
            }
        }
    }

    private Vector3 GetRoadCenter(MapContext context)
    {
        if (context.roadWorldPositions == null || context.roadWorldPositions.Count == 0)
            return Vector3.zero;

        Vector3 sum = Vector3.zero;

        foreach (Vector3 p in context.roadWorldPositions)
            sum += p;

        return sum / context.roadWorldPositions.Count;
    }
}