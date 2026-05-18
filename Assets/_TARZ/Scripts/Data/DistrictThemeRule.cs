using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "TARZ/District/District Theme Rule")]
public class DistrictThemeRule : ScriptableObject
{
    [Header("Block Size")]
    public Vector2 minBlockSize = new Vector2(18f, 18f);
    public Vector2 maxBlockSize = new Vector2(34f, 34f);

    [Header("Placement")]
    public float blockOffsetFromRoad = 28f;
    public float roadAvoidanceGap = 2f;
    public int maxBlocksPerRoad = 2;
    public int maxTryPerRoad = 4;

    [Header("Density")]
    [Range(0f, 1f)]
    public float blockCreationChance = 0.75f;

    [Header("Block Type Weights")]
    public List<CityBlockWeight> blockWeights = new List<CityBlockWeight>();

    public CityBlockType PickBlockType(System.Random random)
    {
        if (blockWeights == null || blockWeights.Count == 0)
            return CityBlockType.Ruins;

        float total = 0f;

        foreach (var item in blockWeights)
            total += Mathf.Max(0f, item.weight);

        if (total <= 0f)
            return CityBlockType.Empty;

        float value = (float)random.NextDouble() * total;

        foreach (var item in blockWeights)
        {
            value -= Mathf.Max(0f, item.weight);

            if (value <= 0f)
                return item.blockType;
        }

        return blockWeights[0].blockType;
    }
}

[System.Serializable]
public class CityBlockWeight
{
    public CityBlockType blockType;
    public float weight = 1f;
}