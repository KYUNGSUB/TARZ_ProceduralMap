using UnityEngine;

public class MapBoundaryColliderBuilder : MonoBehaviour
{
    [Header("Boundary")]
    public float wallHeight = 5f;
    public float wallThickness = 2f;
    public float padding = 3f;

    [Header("Debug")]
    public bool showBoundaryObjects = true;

    public void Build(MapContext context)
    {
        if (context == null || context.mapRoot == null || context.runtimeRoot == null)
        {
            Debug.LogWarning("[MapBoundaryColliderBuilder] Context or root is null.");
            return;
        }

        Bounds bounds = CalculateMapBounds(context.mapRoot);

        if (bounds.size == Vector3.zero)
        {
            Debug.LogWarning("[MapBoundaryColliderBuilder] Map bounds is zero.");
            return;
        }

        GameObject root = new GameObject("MapBoundaryColliders");
        root.transform.SetParent(context.runtimeRoot);

        float minX = bounds.min.x - padding;
        float maxX = bounds.max.x + padding;
        float minZ = bounds.min.z - padding;
        float maxZ = bounds.max.z + padding;

        float centerX = bounds.center.x;
        float centerZ = bounds.center.z;

        float width = maxX - minX;
        float depth = maxZ - minZ;

        CreateWall(root.transform, "NorthWall",
            new Vector3(centerX, wallHeight / 2f, maxZ),
            new Vector3(width, wallHeight, wallThickness));

        CreateWall(root.transform, "SouthWall",
            new Vector3(centerX, wallHeight / 2f, minZ),
            new Vector3(width, wallHeight, wallThickness));

        CreateWall(root.transform, "EastWall",
            new Vector3(maxX, wallHeight / 2f, centerZ),
            new Vector3(wallThickness, wallHeight, depth));

        CreateWall(root.transform, "WestWall",
            new Vector3(minX, wallHeight / 2f, centerZ),
            new Vector3(wallThickness, wallHeight, depth));

        Debug.Log($"[MapBoundaryColliderBuilder] Boundary created. Bounds={bounds}");
    }

    private Bounds CalculateMapBounds(Transform root)
    {
        Renderer[] renderers = root.GetComponentsInChildren<Renderer>();

        if (renderers == null || renderers.Length == 0)
            return new Bounds(Vector3.zero, Vector3.zero);

        Bounds bounds = renderers[0].bounds;

        for (int i = 1; i < renderers.Length; i++)
        {
            bounds.Encapsulate(renderers[i].bounds);
        }

        return bounds;
    }

    private void CreateWall(Transform parent, string name, Vector3 position, Vector3 size)
    {
        GameObject wall = new GameObject(name);
        wall.transform.SetParent(parent);
        wall.transform.position = position;

        BoxCollider collider = wall.AddComponent<BoxCollider>();
        collider.size = size;
        collider.isTrigger = false;

        if (showBoundaryObjects)
        {
            MeshRenderer renderer = wall.AddComponent<MeshRenderer>();
            MeshFilter filter = wall.AddComponent<MeshFilter>();
            filter.mesh = CreateCubeMesh();
        }
    }

    private Mesh CreateCubeMesh()
    {
        GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        Mesh mesh = cube.GetComponent<MeshFilter>().sharedMesh;
        Destroy(cube);
        return mesh;
    }
}