using UnityEngine;

public class MapSafetyGroundBuilder : MonoBehaviour
{
    [Header("Safety Ground")]
    public float padding = 5f;
    public float yPosition = -0.05f;
    public float thickness = 0.2f;

    public void Build(MapContext context)
    {
        if (context == null || context.mapRoot == null || context.runtimeRoot == null)
            return;

        Bounds bounds = CalculateMapBounds(context.mapRoot);

        if (bounds.size == Vector3.zero)
            return;

        GameObject ground = new GameObject("MapSafetyGround");
        ground.transform.SetParent(context.runtimeRoot);
        ground.transform.position = new Vector3(
            bounds.center.x,
            yPosition,
            bounds.center.z
        );

        BoxCollider collider = ground.AddComponent<BoxCollider>();
        collider.size = new Vector3(
            bounds.size.x + padding * 2f,
            thickness,
            bounds.size.z + padding * 2f
        );

        collider.isTrigger = false;

        Debug.Log($"[MapSafetyGroundBuilder] Safety ground created. Size={collider.size}");
    }

    private Bounds CalculateMapBounds(Transform root)
    {
        Renderer[] renderers = root.GetComponentsInChildren<Renderer>();

        if (renderers == null || renderers.Length == 0)
            return new Bounds(Vector3.zero, Vector3.zero);

        Bounds bounds = renderers[0].bounds;

        for (int i = 1; i < renderers.Length; i++)
            bounds.Encapsulate(renderers[i].bounds);

        return bounds;
    }
}