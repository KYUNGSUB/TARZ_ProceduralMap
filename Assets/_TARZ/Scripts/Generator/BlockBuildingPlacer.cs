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
            //            Vector3 position = GetEdgePointInBlock(context, block);
            Vector3 position = GetInnerPointInBlock(context, block);

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

    private Vector3 GetInnerPointInBlock(MapContext context, CityBlock block)
    {
        // Block 내부 중앙 영역만 사용
        float usableX = block.size.x * 0.25f;
        float usableZ = block.size.y * 0.25f;

        float x = RandomRange(
            context,
            -usableX,
            usableX
        );

        float z = RandomRange(
            context,
            -usableZ,
            usableZ
        );

        return block.center + new Vector3(
            x,
            0f,
            z
        );
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

    private bool CanPlaceBuilding(MapContext context, Bounds buildingBounds)
    {
        // 1. Map 영역 밖이면 금지
        if (context.hasMapBounds && !ContainsBoundsXZ(context.mapBounds, buildingBounds))
            return false;

        // 2. Road Grid 기반 금지 영역 검사
        if (OverlapsAnyRoadTile(context, buildingBounds))
            return false;

        // 3. Start / Boss / Reward POI 위 건물 금지
        foreach (POIArea poi in context.poiAreas)
        {
            if (poi.type != POIType.Start &&
                poi.type != POIType.Boss &&
                poi.type != POIType.Reward)
            {
                continue;
            }

            Bounds expandedPOI = poi.bounds;
            expandedPOI.Expand(new Vector3(2f, 0f, 2f));

            if (IntersectsXZ(expandedPOI, buildingBounds))
                return false;
        }

        // 4. 건물끼리 겹침 금지
        foreach (Bounds b in context.buildingBounds)
        {
            if (IntersectsXZ(b, buildingBounds))
                return false;
        }

        return true;
    }

    private bool OverlapsAnyRoadTile(MapContext context, Bounds buildingBounds)
    {
        float roadSafetyGap = 2.0f;

        foreach (Vector3 roadCenter in context.roadWorldPositions)
        {
            Bounds expandedRoadBounds = new Bounds(
                roadCenter,
                new Vector3(
                    context.settings.tileSize,
                    10f,
                    context.settings.tileSize
                )
            );

            // Building 크기를 고려한 추가 여유 공간
            float buildingMargin =
                Mathf.Max(
                    buildingBounds.size.x,
                    buildingBounds.size.z
                ) * 0.5f;

            expandedRoadBounds.Expand(
                new Vector3(
                    buildingMargin * 1.5f,
                    0f,
                    buildingMargin * 1.5f
                )
            );

            if (IntersectsXZ(expandedRoadBounds, buildingBounds))
                return true;
        }

        return false;
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