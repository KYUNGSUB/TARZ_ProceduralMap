using UnityEngine;

public class CombatZoneVisualizer : MonoBehaviour
{
    public Material normalMaterial;
    public Material bossMaterial;
    public Material midBossMaterial;

    public float yOffset = 0.05f;

    public void Visualize(MapContext context)
    {
        if (context == null || context.combatZones == null)
            return;

        foreach (CombatZoneArea zone in context.combatZones)
        {
            CreateZoneObject(context, zone);
        }
    }

    private void CreateZoneObject(
        MapContext context,
        CombatZoneArea zone)
    {
        GameObject obj = GameObject.CreatePrimitive(
            PrimitiveType.Cylinder
        );

        obj.name = "CombatZone";

        obj.transform.SetParent(context.mapRoot);

        obj.transform.position =
            zone.center + Vector3.up * yOffset;

        obj.transform.localScale = new Vector3(
            zone.radius * 2f,
            0.02f,
            zone.radius * 2f
        );

        Destroy(obj.GetComponent<Collider>());

        Renderer renderer =
            obj.GetComponent<Renderer>();

        if (renderer != null)
        {
            if (zone.isBossZone && bossMaterial != null)
            {
                renderer.material = bossMaterial;
            }
            else if (zone.isMidBossZone && midBossMaterial != null)
            {
                renderer.material = midBossMaterial;
            }
            else if (normalMaterial != null)
            {
                renderer.material = normalMaterial;
            }
        }
    }
}