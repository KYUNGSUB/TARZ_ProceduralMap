using UnityEngine;

public enum CityBlockType
{
    Residential,
    Commercial,
    Industrial,
    Park,
    Ruins,
    BeachResort,
    HarborContainer,
    SecretFacility,
    Empty
}

[System.Serializable]
public class CityBlock
{
    public CityBlockType blockType;
    public Vector3 center;
    public Vector2 size;
    public Bounds bounds;

    public CityBlock(CityBlockType blockType, Vector3 center, Vector2 size)
    {
        this.blockType = blockType;
        this.center = center;
        this.size = size;

        this.bounds = new Bounds(
            center,
            new Vector3(size.x, 4f, size.y)
        );
    }
}