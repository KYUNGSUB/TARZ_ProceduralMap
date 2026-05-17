using UnityEngine;

[CreateAssetMenu(menuName = "TARZ/Building/Building Placement Rule")]
public class BuildingPlacementRule : ScriptableObject
{
    [Header("Distance")]
    public float minGapFromRoad = 2f;
    public float randomExtraOffset = 6f;

    [Header("Placement Chance")]
    [Range(0f, 1f)]
    public float basePlacementChance = 0.7f;

    [Header("POI Avoidance")]
    public float poiExtraAvoidanceRadius = 4f;

    [Header("Combat Zone Avoidance")]
    public float combatZoneAvoidanceRadius = 10f;
    [Range(0f, 1f)]
    public float combatZoneBuildingChanceMultiplier = 0.35f;

    [Header("Retry")]
    public int maxTryPerRoadSide = 3;
}