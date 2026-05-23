using UnityEngine;

public class RoadEndBoundaryBuilder : MonoBehaviour
{
    [Header("Road End Wall")]
    public float wallWidth = 8f;
    public float wallHeight = 4f;
    public float wallThickness = 1f;
    public float yPosition = 2f;

    public void Build(MapContext context)
    {
        if (context == null || context.mapRoot == null || context.runtimeRoot == null)
            return;

        RoadEndMarker[] markers =
            context.mapRoot.GetComponentsInChildren<RoadEndMarker>();

        if (markers == null || markers.Length == 0)
        {
            Debug.LogWarning("[RoadEndBoundaryBuilder] RoadEndMarker가 없습니다.");
            return;
        }

        GameObject root = new GameObject("RoadEndBoundaries");
        root.transform.SetParent(context.runtimeRoot);

        foreach (RoadEndMarker marker in markers)
        {
            CreateWall(root.transform, marker.transform);
        }

        Debug.Log($"[RoadEndBoundaryBuilder] Road end walls created: {markers.Length}");
    }

    private void CreateWall(Transform parent, Transform marker)
    {
        GameObject wall = new GameObject("RoadEndWall");
        wall.transform.SetParent(parent);

        wall.transform.position =
            marker.position + Vector3.up * yPosition;

        wall.transform.rotation = marker.rotation;

        BoxCollider col = wall.AddComponent<BoxCollider>();
        col.size = new Vector3(wallWidth, wallHeight, wallThickness);
        col.isTrigger = false;
    }
}