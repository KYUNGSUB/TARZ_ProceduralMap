using UnityEngine;

public class CombatZoneVisualizer : MonoBehaviour
{
    public Material normalMaterial;
    public Material bossMaterial;
    public Material midBossMaterial;

    public float yOffset = 0.08f;
    public bool parentToDebugRoot = true;

    public void Visualize(MapContext context)
    {
        if (context == null || context.combatZones == null)
            return;

        Transform parent =
            parentToDebugRoot && context.debugRoot != null
                ? context.debugRoot
                : context.mapRoot;

        foreach (CombatZoneArea zone in context.combatZones)
        {
            CreateZoneObject(parent, zone);
        }
    }

    private void CreateZoneObject(
        Transform parent,
        CombatZoneArea zone)
    {
        GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Cylinder);

        obj.name = zone.isBossZone
            ? "CombatZone_Boss"
            : zone.isMidBossZone
                ? "CombatZone_MidBoss"
                : "CombatZone_Normal";

        obj.transform.SetParent(parent);

        obj.transform.position =
            zone.center + Vector3.up * yOffset;

        obj.transform.localScale = new Vector3(
            zone.radius * 3f,
            0.01f,
            zone.radius * 3f
        );

        Collider col = obj.GetComponent<Collider>();
        if (col != null)
            Destroy(col);

        Renderer renderer = obj.GetComponent<Renderer>();

        if (renderer != null)
        {
            if (zone.isBossZone && bossMaterial != null)
                renderer.material = bossMaterial;
            else if (zone.isMidBossZone && midBossMaterial != null)
                renderer.material = midBossMaterial;
            else if (normalMaterial != null)
                renderer.material = normalMaterial;
        }

        Debug.Log(
            $"[CombatZoneVisualizer] {obj.name}, " +
            $"Boss={zone.isBossZone}, MidBoss={zone.isMidBossZone}, Radius={zone.radius}"
        );
    }
}