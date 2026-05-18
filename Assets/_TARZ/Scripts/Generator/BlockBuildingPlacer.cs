using System.Collections.Generic;
using UnityEngine;

public class BlockBuildingPlacer : MonoBehaviour
{
    [Header("Buildings Per Block")]
    public int minBuildingsPerBlock = 2;
    public int maxBuildingsPerBlock = 5;

    [Header("Placement")]
    public float edgePadding = 3f;
    public int maxTryPerBuilding = 10;

    public void Place(MapContext context)
    {
        foreach (CityBlock block in context.cityBlocks)
        {
            PlaceBuildingsInBlock(context, block);
        }
    }

    private void PlaceBuildingsInBlock(MapContext context, CityBlock block)
    {
        int count = context.random.Next(minBuildingsPerBlock, maxBuildingsPerBlock + 1);

        for (int i = 0; i < count; i++)
        {
            TryPlaceBuilding(context, block);
        }
    }

    private void TryPlaceBuilding(MapContext context, CityBlock block)
    {
        for (int i = 0; i < maxTryPerBuilding; i++)
        {
            GameObject prefab = PickBuildingForBlock(context, block.blockType);

            if (prefab == null)
                return;

//            Vector3 position = GetRandomPointInBlock(context, block);
            Vector3 position = GetEdgePointInBlock(context, block);

            Quaternion rotation = Quaternion.Euler(
                0f,
                context.random.Next(0, 4) * 90f,
                0f
            );

            GameObject building = Instantiate(prefab, position, rotation, context.mapRoot);
            building.name = $"Building_{block.blockType}";

            Bounds bounds = BoundsUtility.GetObjectBounds(building);

            if (!CanPlaceBuilding(context, bounds))
            {
                Destroy(building);
                continue;
            }

            context.buildingBounds.Add(bounds);
            return;
        }
    }

    private Vector3 GetEdgePointInBlock(MapContext context, CityBlock block)
    {
        float halfX = block.size.x * 0.5f - edgePadding;
        float halfZ = block.size.y * 0.5f - edgePadding;

        int edge = context.random.Next(0, 4);

        switch (edge)
        {
            // North
            case 0:
                {
                    float x = RandomRange(context, -halfX, halfX);

                    return block.center + new Vector3(
                        x,
                        0f,
                        halfZ
                    );
                }

            // South
            case 1:
                {
                    float x = RandomRange(context, -halfX, halfX);

                    return block.center + new Vector3(
                        x,
                        0f,
                        -halfZ
                    );
                }

            // East
            case 2:
                {
                    float z = RandomRange(context, -halfZ, halfZ);

                    return block.center + new Vector3(
                        halfX,
                        0f,
                        z
                    );
                }

            // West
            default:
                {
                    float z = RandomRange(context, -halfZ, halfZ);

                    return block.center + new Vector3(
                        -halfX,
                        0f,
                        z
                    );
                }
        }
    }

    private GameObject PickBuildingForBlock(MapContext context, CityBlockType type)
    {
        List<GameObject> candidates = new List<GameObject>();

        switch (type)
        {
            case CityBlockType.Residential:
                candidates.AddRange(context.theme.residentialBuildingPrefabs);
                break;

            case CityBlockType.Commercial:
                candidates.AddRange(context.theme.commercialBuildingPrefabs);
                break;

            case CityBlockType.Industrial:
                candidates.AddRange(context.theme.industrialBuildingPrefabs);
                break;

            case CityBlockType.HarborContainer:
                candidates.AddRange(context.theme.harborBuildingPrefabs);
                break;

            case CityBlockType.BeachResort:
                candidates.AddRange(context.theme.beachBuildingPrefabs);
                break;
        }

        // Type별 Prefab이 없으면 공통 Building Prefab 사용
        if (candidates.Count == 0)
            candidates.AddRange(context.theme.buildingPrefabs);

        return PrefabPicker.Pick(candidates, context.random);
    }

    private Vector3 GetRandomPointInBlock(MapContext context, CityBlock block)
    {
        float halfX = block.size.x * 0.5f - edgePadding;
        float halfZ = block.size.y * 0.5f - edgePadding;

        float x = RandomRange(context, -halfX, halfX);
        float z = RandomRange(context, -halfZ, halfZ);

        return block.center + new Vector3(x, 0f, z);
    }

    private bool CanPlaceBuilding(MapContext context, Bounds bounds)
    {
        // 1. Map 영역 밖이면 금지
        if (context.hasMapBounds && !ContainsBoundsXZ(context.mapBounds, bounds))
            return false;

        // 2. Road와 겹치면 금지
        float roadSafetyGap = 1.5f;

        foreach (Bounds road in context.roadBounds)
        {
            Bounds expandedRoad = road;
            expandedRoad.Expand(new Vector3(roadSafetyGap * 2f, 0f, roadSafetyGap * 2f));

            if (IntersectsXZ(expandedRoad, bounds))
                return false;
        }

        // 3. 건물끼리 겹치면 금지
        foreach (Bounds b in context.buildingBounds)
        {
            if (IntersectsXZ(b, bounds))
                return false;
        }

        return true;
    }

    private bool IntersectsXZ(Bounds a, Bounds b)
    {
        bool overlapX = a.min.x <= b.max.x && a.max.x >= b.min.x;
        bool overlapZ = a.min.z <= b.max.z && a.max.z >= b.min.z;

        return overlapX && overlapZ;
    }

    private bool ContainsBoundsXZ(Bounds outer, Bounds inner)
    {
        return inner.min.x >= outer.min.x &&
               inner.max.x <= outer.max.x &&
               inner.min.z >= outer.min.z &&
               inner.max.z <= outer.max.z;
    }

    private float RandomRange(MapContext context, float min, float max)
    {
        return min + (float)context.random.NextDouble() * (max - min);
    }
}