using UnityEngine;

[System.Serializable]
public class CombatZoneArea
{
    public Vector3 center;
    public float radius;
    public bool isBossZone;
    public bool isMidBossZone;

    public CombatZoneArea(
        Vector3 center,
        float radius,
        bool isBossZone = false,
        bool isMidBossZone = false)
    {
        this.center = center;
        this.radius = radius;
        this.isBossZone = isBossZone;
        this.isMidBossZone = isMidBossZone;
    }

    public Bounds GetBounds()
    {
        return new Bounds(
            center,
            new Vector3(radius * 2.4f, 4f, radius * 2.4f)
        );
    }
}