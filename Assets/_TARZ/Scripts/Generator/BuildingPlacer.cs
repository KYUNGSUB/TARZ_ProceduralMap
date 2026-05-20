using System.Collections.Generic;
using UnityEngine;

public class BuildingPlacer : MonoBehaviour
{
    [Header("Lot Rule")]
    public LotPlacementRule lotRule;

    public void Place(MapContext context)
    {
        if (lotRule == null)
        {
            Debug.LogWarning("LotPlacementRule is missing.");
            return;
        }

        if (context.theme == null || context.theme.buildingPrefabs == null || context.theme.buildingPrefabs.Count == 0)
        {
            Debug.LogWarning("Building prefabs are missing.");
            return;
        }

        HashSet<Vector2Int> roadSet = new HashSet<Vector2Int>(context.roadGridPositions);

        int placedCount = 0;

        foreach (Vector2Int roadGrid in context.roadGridPositions)
        {
            Vector3 roadWorld = context.GridToWorld(roadGrid);
            Vector3 roadDirection = EstimateRoadDirection(roadGrid, roadSet);

            Vector3 left = Vector3.Cross(Vector3.up, roadDirection).normalized;
            Vector3 right = -left;

            if (TryPlaceLotBuilding(context, roadWorld, roadDirection, left))
                placedCount++;

            if (TryPlaceLotBuilding(context, roadWorld, roadDirection, right))
                placedCount++;
        }

        Debug.Log($"Lot Buildings placed: {placedCount}");
    }

    private bool TryPlaceLotBuilding(
        MapContext context,
        Vector3 roadWorld,
        Vector3 roadDirection,
        Vector3 sideDirection
    )
    {
        float chance = lotRule.placementChance * context.theme.buildingDensity;

        if (context.random.NextDouble() > chance)
            return false;

        for (int i = 0; i < lotRule.maxTryPerSide; i++)
        {
            LotType lotType = DetermineLotType(context);

            Vector3 lotCenter = CalculateLotCenter(
                context,
                roadWorld,
                sideDirection,
                Vector2.zero
            );

            Quaternion rotation = Quaternion.LookRotation(-sideDirection.normalized, Vector3.up);

            switch (lotType)
            {
                case LotType.Empty:
                    return true;

                case LotType.Debris:
                    if (CanUseLotArea(context, lotCenter))
                    {
                        PlaceDebrisLot(context, lotCenter, roadDirection, sideDirection);
                        return true;
                    }
                    break;

                case LotType.Building:
                    if (TryPlaceBuildingInLot(context, lotCenter, roadDirection, sideDirection))
                        return true;
                    break;
            }
        }

        return false;
    }

    private bool TryPlaceBuildingInLot(
        MapContext context,
        Vector3 lotCenter,
        Vector3 roadDirection,
        Vector3 sideDirection
    )
    {
        GameObject prefab = PrefabPicker.Pick(context.theme.buildingPrefabs, context.random);

        if (prefab == null)
            return false;

        Vector2 footprint = GetBuildingFootprint(prefab);

        Vector3 buildingPosition = GetBuildingPositionInsideLot(
            context,
            lotCenter,
            roadDirection,
            sideDirection,
            footprint
        );

        Quaternion rotation = Quaternion.LookRotation(-sideDirection.normalized, Vector3.up);

        GameObject building = Instantiate(
            prefab,
            buildingPosition,
            rotation,
            context.mapRoot
        );

        building.name = "Building_Lot";

        Bounds bounds = BoundsUtility.GetObjectBounds(building);

        if (!CanPlaceBuilding(context, bounds))
        {
            Destroy(building);
            return false;
        }

        context.buildingBounds.Add(bounds);
        return true;
    }

    private LotType DetermineLotType(MapContext context)
    {
        double value = context.random.NextDouble();

        // 폐허 도시 기준
        // Building 45%, Debris 20%, Empty 35%
        if (value < 0.45)
            return LotType.Building;

        if (value < 0.65)
            return LotType.Debris;

        return LotType.Empty;
    }

    private void PlaceDebrisLot(    // Cluster 방식으로 교체
        MapContext context,
        Vector3 lotCenter,
        Vector3 roadDirection,
        Vector3 sideDirection
    )
    {
        if (context.theme.debrisPrefabs == null || context.theme.debrisPrefabs.Count == 0)
            return;

        // Debris는 Lot 전체가 아니라 Lot 내부 작은 영역에만 생성
        int count = context.random.Next(2, 4);

        // Road에서 조금 멀어지는 방향으로 Cluster 중심 이동
        Vector3 clusterCenter =
            lotCenter +
            sideDirection.normalized * RandomRange(context, 2f, 4f) +
            roadDirection.normalized * RandomRange(context, -1.5f, 1.5f);

        for (int i = 0; i < count; i++)
        {
            GameObject prefab = PrefabPicker.Pick(context.theme.debrisPrefabs, context.random);

            if (prefab == null)
                continue;

            // 기존 -4~4처럼 넓게 퍼뜨리지 말고 작은 더미로 제한
            float forwardOffset = RandomRange(context, -1.5f, 1.5f);
            float sideOffset = RandomRange(context, -1.5f, 1.5f);

            Vector3 position =
                clusterCenter +
                roadDirection.normalized * forwardOffset +
                sideDirection.normalized * sideOffset;

            position.y = 0f;

            Quaternion rotation = Quaternion.Euler(
                0f,
                RandomRange(context, 0f, 360f),
                0f
            );

            GameObject obj = Instantiate(
                prefab,
                position,
                rotation,
                context.mapRoot
            );

            obj.name = "Debris_Cluster";
        }
    }

    private bool CanUseLotArea(MapContext context, Vector3 lotCenter)
    {
        Bounds lotBounds = new Bounds(
            lotCenter,
            new Vector3(
                lotRule.lotWidth,
                5f,
                lotRule.lotDepth
            )
        );

        if (context.hasMapBounds && !ContainsBoundsXZ(context.mapBounds, lotBounds))
            return false;

        if (OverlapsAnyRoadTile(context, lotBounds))
            return false;

        return true;
    }

    private Vector3 CalculateLotCenter(
        MapContext context,
        Vector3 roadWorld,
        Vector3 sideDirection,
        Vector2 buildingFootprint
    )
    {
        float roadHalf = lotRule.roadWidth * 0.5f;
        float sidewalk = lotRule.sidewalkWidth;
        float lotHalfDepth = lotRule.lotDepth * 0.5f;

        float offset =
            roadHalf +
            sidewalk +
            lotHalfDepth;

        Vector3 lotCenter = roadWorld + sideDirection.normalized * offset;
        lotCenter.y = 0f;

        return lotCenter;
    }

    private Vector3 GetBuildingPositionInsideLot(
        MapContext context,
        Vector3 lotCenter,
        Vector3 roadDirection,
        Vector3 sideDirection,
        Vector2 footprint
    )
    {
        float maxForwardOffset =
            Mathf.Max(0f, (lotRule.lotWidth - footprint.x) * 0.5f - lotRule.buildingInsetFromLotEdge);

        float maxSideOffset =
            Mathf.Max(0f, (lotRule.lotDepth - footprint.y) * 0.5f - lotRule.buildingInsetFromLotEdge);

        float forwardOffset = RandomRange(
            context,
            -Mathf.Min(maxForwardOffset, lotRule.randomOffsetInLot),
            Mathf.Min(maxForwardOffset, lotRule.randomOffsetInLot)
        );

        float sideOffset = RandomRange(
            context,
            -Mathf.Min(maxSideOffset, lotRule.randomOffsetInLot),
            Mathf.Min(maxSideOffset, lotRule.randomOffsetInLot)
        );

        Vector3 position =
            lotCenter +
            roadDirection.normalized * forwardOffset +
            sideDirection.normalized * sideOffset;

        position.y = 0f;

        return position;
    }

    private bool CanPlaceBuilding(MapContext context, Bounds buildingBounds)
    {
        if (context.hasMapBounds)
        {
            Bounds safeMapBounds = context.mapBounds;
            safeMapBounds.Expand(new Vector3(
                -lotRule.mapBoundaryGap * 2f,
                0f,
                -lotRule.mapBoundaryGap * 2f
            ));

            if (!ContainsBoundsXZ(safeMapBounds, buildingBounds))
                return false;
        }

        if (OverlapsAnyRoadTile(context, buildingBounds))
            return false;

        foreach (POIArea poi in context.poiAreas)
        {
            if (poi.type != POIType.Start &&
                poi.type != POIType.Boss)
            {
                continue;
            }

            Bounds protectedBounds = poi.bounds;
            protectedBounds.Expand(new Vector3(
                lotRule.protectedPOIGap * 2f,
                0f,
                lotRule.protectedPOIGap * 2f
            ));

            if (IntersectsXZ(protectedBounds, buildingBounds))
                return false;
        }

        foreach (Bounds b in context.buildingBounds)
        {
            if (IntersectsXZ(b, buildingBounds))
                return false;
        }

        return true;
    }

    private bool OverlapsAnyRoadTile(MapContext context, Bounds buildingBounds)
    {
        foreach (Vector3 roadCenter in context.roadWorldPositions)
        {
            Bounds roadBounds = new Bounds(
                roadCenter,
                new Vector3(
                    lotRule.roadWidth + lotRule.roadSafetyGap * 2f,
                    10f,
                    lotRule.roadWidth + lotRule.roadSafetyGap * 2f
                )
            );

            if (IntersectsXZ(roadBounds, buildingBounds))
                return true;
        }

        return false;
    }

    private Vector3 EstimateRoadDirection(Vector2Int grid, HashSet<Vector2Int> roadSet)
    {
        bool hasUp = roadSet.Contains(grid + Vector2Int.up);
        bool hasDown = roadSet.Contains(grid + Vector2Int.down);
        bool hasRight = roadSet.Contains(grid + Vector2Int.right);
        bool hasLeft = roadSet.Contains(grid + Vector2Int.left);

        int vertical = 0;
        if (hasUp) vertical++;
        if (hasDown) vertical++;

        int horizontal = 0;
        if (hasRight) horizontal++;
        if (hasLeft) horizontal++;

        if (horizontal > vertical)
            return Vector3.right;

        return Vector3.forward;
    }

    private Vector2 GetBuildingFootprint(GameObject prefab)
    {
        Renderer[] renderers = prefab.GetComponentsInChildren<Renderer>();

        if (renderers == null || renderers.Length == 0)
            return new Vector2(8f, 8f);

        Bounds bounds = renderers[0].bounds;

        for (int i = 1; i < renderers.Length; i++)
            bounds.Encapsulate(renderers[i].bounds);

        float x = Mathf.Max(bounds.size.x, 1f);
        float z = Mathf.Max(bounds.size.z, 1f);

        return new Vector2(x, z);
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