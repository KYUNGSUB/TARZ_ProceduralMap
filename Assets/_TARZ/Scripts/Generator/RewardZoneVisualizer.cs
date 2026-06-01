using UnityEngine;

public class RewardZoneVisualizer : MonoBehaviour
{
    [Header("Material")]
    public Material rewardMaterial;

    [Header("Display")]
    public float radius = 5f;
    public float yOffset = 0.08f;
    public bool parentToDebugRoot = true;

    public void Visualize(MapContext context)
    {
        if (context == null || context.rewardPositions == null)
            return;

        Transform parent =
            parentToDebugRoot && context.debugRoot != null
                ? context.debugRoot
                : context.mapRoot;

        foreach (Vector3 rewardPos in context.rewardPositions)
        {
            CreateRewardZoneObject(parent, rewardPos);
        }
    }

    private void CreateRewardZoneObject(
        Transform parent,
        Vector3 rewardPos)
    {
        GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Cylinder);

        obj.name = "RewardZone";

        obj.transform.SetParent(parent);

        obj.transform.position =
            rewardPos + Vector3.up * yOffset;

        obj.transform.localScale = new Vector3(
            radius * 2f,
            0.01f,
            radius * 2f
        );

        Collider col = obj.GetComponent<Collider>();
        if (col != null)
            Destroy(col);

        Renderer renderer = obj.GetComponent<Renderer>();

        if (renderer != null && rewardMaterial != null)
        {
            renderer.material = rewardMaterial;
        }
    }
}