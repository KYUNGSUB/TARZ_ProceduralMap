using UnityEngine;

[CreateAssetMenu(menuName = "TARZ/Map/Generation Settings")]
public class MapGenerationSettings : ScriptableObject
{
    [Header("Grid")]
    public float tileSize = 20f;
    public int mainPathLength = 14;
    public int branchCount = 4;
    public int branchMinLength = 2;
    public int branchMaxLength = 5;

    [Header("Placement")]
    public float buildingOffsetFromRoad = 18f;
    public float objectPlacementRadius = 8f;
    public float throwObjectRadius = 10f;

    [Header("Counts")]
    public int objectsPerRoadTile = 5;
    public int throwObjectsPerCombatZone = 8;
    public int enemiesPerCombatZone = 5;

    [Header("Random")]
    [Range(0f, 1f)] public float turnChance = 0.25f;
    [Range(0f, 1f)] public float branchChance = 0.25f;
    [Range(0f, 1f)] public float buildingChance = 0.75f;
}