using UnityEngine;

[CreateAssetMenu(menuName = "TARZ/Building/Lot Placement Rule")]
public class LotPlacementRule : ScriptableObject
{
    [Header("Road / Sidewalk / Lot")]
    public float roadWidth = 20f;
    public float sidewalkWidth = 4f;
    public float lotDepth = 16f;
    public float lotWidth = 16f;

    [Header("Building Placement")]
    [Range(0f, 1f)]
    public float placementChance = 0.85f;

    public float buildingInsetFromLotEdge = 2f;
    public float randomOffsetInLot = 2f;

    [Header("Safety")]
    public float roadSafetyGap = 0.5f;
    public float mapBoundaryGap = 1f;
    public float protectedPOIGap = 2f;

    [Header("Retry")]
    public int maxTryPerSide = 3;
}