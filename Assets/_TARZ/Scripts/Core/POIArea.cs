using UnityEngine;

public enum POIType
{
    Start,
    Combat,
    Secret,
    Reward,
    Boss
}

[System.Serializable]
public class POIArea
{
    public POIType type;
    public Vector3 center;
    public float radius;
    public Bounds bounds;

    public POIArea(POIType type, Vector3 center, float radius)
    {
        this.type = type;
        this.center = center;
        this.radius = radius;

        Vector3 size = new Vector3(radius * 2f, 1f, radius * 2f);
        this.bounds = new Bounds(center, size);
    }
}