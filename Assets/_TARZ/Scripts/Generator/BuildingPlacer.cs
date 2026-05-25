using System.Collections.Generic;
using UnityEngine;

public class BuildingPlacer : MonoBehaviour
{
    [Header("Lot Rule")]
    public LotPlacementRule lotRule;

    public void Place(MapContext context)
    {
        if (context == null || context.theme == null)
        {
            Debug.LogWarning("[BuildingPlacer] Context or theme is null.");
            return;
        }

        switch (context.selectedMapShape)
        {
            case StageMapShapeType.LinearLongRoad:
            case StageMapShapeType.CurvedRoad:
            case StageMapShapeType.CityCorridor:
                PlaceCorridorBuildings(context);
                break;

            case StageMapShapeType.ObjectArena:
                PlaceArenaBuildings(context);
                break;

            case StageMapShapeType.BranchSecretPath:
                PlaceCorridorBuildings(context);
                break;

            case StageMapShapeType.BossArena:
                PlaceBossArenaBuildings(context);
                break;

            default:
                PlaceCorridorBuildings(context);
                break;
        }
    }

    private void PlaceCorridorBuildings(MapContext context)
    {
        if (context.roadWorldPositions == null || context.roadWorldPositions.Count == 0)
        {
            Debug.LogWarning("[BuildingPlacer] No road positions.");
            return;
        }

        if (context.theme.buildingPrefabs == null || context.theme.buildingPrefabs.Count == 0)
        {
            Debug.LogWarning("[BuildingPlacer] No building prefabs.");
            return;
        }

        float tileSize = context.settings.tileSize;

        float buildingOffset = tileSize * 1.15f;
        float placementChance = 1.0f;

        for (int i = 0; i < context.roadWorldPositions.Count; i++)
        {
            Vector3 roadPos = context.roadWorldPositions[i];

            Vector3 forward = GetRoadForward(context, i);
            Vector3 right = Vector3.Cross(Vector3.up, forward).normalized;

            float randomOffsetRight =
                buildingOffset +
                Mathf.Lerp(
                    -tileSize * 0.15f,
                    tileSize * 0.25f,
                    (float)context.random.NextDouble()
                );

            float randomOffsetLeft =
                buildingOffset +
                Mathf.Lerp(
                    -tileSize * 0.15f,
                    tileSize * 0.25f,
                    (float)context.random.NextDouble()
                );

            TryPlaceBuildingAtSide(
                context,
                roadPos + right * randomOffsetRight,
                Quaternion.LookRotation(-right),
                placementChance,
                "Building_Right"
            );

            float secondRowOffsetRight = randomOffsetRight + tileSize * 0.75f;

            TryPlaceBuildingAtSide(
                context,
                roadPos + right * secondRowOffsetRight,
                Quaternion.LookRotation(-right),
                placementChance * 0.9f,
                "Building_Right_Row2"
            );

            TryPlaceBuildingAtSide(
                context,
                roadPos - right * randomOffsetLeft,
                Quaternion.LookRotation(right),
                placementChance,
                "Building_Left"
            );

            float secondRowOffsetLeft = randomOffsetLeft + tileSize * 0.75f;

            TryPlaceBuildingAtSide(
                context,
                roadPos - right * secondRowOffsetLeft,
                Quaternion.LookRotation(right),
                placementChance * 0.65f,
                "Building_Left_Row2"
            );

            // 추가: 현재 Road와 다음 Road 사이 중간 지점에도 건물 배치
            if (i < context.roadWorldPositions.Count - 1)
            {
                Vector3 nextRoadPos = context.roadWorldPositions[i + 1];
                Vector3 midRoadPos = Vector3.Lerp(roadPos, nextRoadPos, 0.5f);

                Vector3 midForward = (nextRoadPos - roadPos).normalized;
                Vector3 midRight = Vector3.Cross(Vector3.up, midForward).normalized;

                float midOffsetRight =
                    buildingOffset +
                    Mathf.Lerp(
                        -tileSize * 0.1f,
                        tileSize * 0.15f,
                        (float)context.random.NextDouble()
                    );

                float midOffsetLeft =
                    buildingOffset +
                    Mathf.Lerp(
                        -tileSize * 0.1f,
                        tileSize * 0.15f,
                        (float)context.random.NextDouble()
                    );

                TryPlaceBuildingAtSide(
                    context,
                    midRoadPos + midRight * midOffsetRight,
                    Quaternion.LookRotation(-midRight),
                    placementChance * 0.55f,
                    "Building_Right_Mid"
                );

                TryPlaceBuildingAtSide(
                    context,
                    midRoadPos - midRight * midOffsetLeft,
                    Quaternion.LookRotation(midRight),
                    placementChance * 0.55f,
                    "Building_Left_Mid"
                );
            }
        }

        Debug.Log("[BuildingPlacer] Corridor buildings placed.");
    }

    // 도로 방향 계산 메소드
    private Vector3 GetRoadForward(MapContext context, int index)
    {
        if (context.roadWorldPositions.Count <= 1)
            return Vector3.forward;

        if (index == 0)
        {
            return (context.roadWorldPositions[1] - context.roadWorldPositions[0]).normalized;
        }

        if (index == context.roadWorldPositions.Count - 1)
        {
            return (context.roadWorldPositions[index] - context.roadWorldPositions[index - 1]).normalized;
        }

        Vector3 prev = context.roadWorldPositions[index] - context.roadWorldPositions[index - 1];
        Vector3 next = context.roadWorldPositions[index + 1] - context.roadWorldPositions[index];

        return (prev + next).normalized;
    }

    // 실제 건물 배치 메소드
    private void TryPlaceBuildingAtSide(
    MapContext context,
    Vector3 position,
    Quaternion rotation,
    float chance,
    string namePrefix)
    {
        if ((float)context.random.NextDouble() > chance)
            return;

        GameObject prefab = PickBuildingPrefab(context);

        if (prefab == null)
            return;

        float randomY = Random.Range(-12f, 12f);

        Quaternion finalRotation =
            rotation * Quaternion.Euler(0f, randomY, 0f);

        GameObject building = Instantiate(
            prefab,
            position,
            finalRotation,
            context.mapRoot
        );

        float randomScale = Random.Range(0.85f, 1.15f);

        building.transform.localScale *= randomScale;

        building.name = $"{namePrefix}_{Mathf.RoundToInt(position.x)}_{Mathf.RoundToInt(position.z)}";

        Bounds bounds = BoundsUtility.GetObjectBounds(building);

        if (IsOverlappingBlockedArea(context, bounds))
        {
            Destroy(building);
            return;
        }

        context.buildingBounds.Add(bounds);
        context.occupiedBounds.Add(bounds);
    }

    // 건물 프리팹 선택 메소드
    private GameObject PickBuildingPrefab(MapContext context)
    {
        List<GameObject> list = context.theme.buildingPrefabs;

        if (list == null || list.Count == 0)
            return null;

        int index = context.random.Next(0, list.Count);
        return list[index];
    }

    private bool IsTooCloseToRoad(
        MapContext context,
        Bounds buildingBounds)
    {
        if (context.roadWorldPositions == null)
            return false;

        float minDistance = context.settings.tileSize * 0.55f;

        foreach (Vector3 roadPos in context.roadWorldPositions)
        {
            float distance = Vector3.Distance(
                roadPos,
                buildingBounds.center
            );

            if (distance < minDistance)
                return true;
        }

        return false;
    }

    private bool IsTooFarFromRoad(MapContext context, Bounds buildingBounds)
    {
        float maxDistance = context.settings.tileSize * 4.5f;

        foreach (Vector3 roadPos in context.roadWorldPositions)
        {
            if (Vector3.Distance(roadPos, buildingBounds.center) <= maxDistance)
                return false;
        }

        return true;
    }

    // 도로/POI와 겹침 방지
    private bool IsOverlappingBlockedArea(MapContext context, Bounds buildingBounds)
    {
        // Road와 너무 가까운지 검사
        if (IsTooCloseToRoad(context, buildingBounds))
            return true;

        // Road로부터 너무 멀리 떨어졌는지 검사
        if (IsTooFarFromRoad(context, buildingBounds))
            return true;

        // Building 간 최소 거리 추가
        if (IsTooCloseToOtherBuildings(context, buildingBounds))
            return true;

        // Combat Zone과 겹치는지 검사
        if (context.combatZones != null)
        {
            foreach (CombatZoneArea zone in context.combatZones)
            {
                Bounds zoneBounds = zone.GetBounds();

                if (zoneBounds.Intersects(buildingBounds))
                    return true;
            }
        }

        // POI 영역과 겹치는지 검사
        if (context.poiAreas != null)
        {
            foreach (POIArea poi in context.poiAreas)
            {
                if (poi.bounds.Intersects(buildingBounds))
                    return true;
            }
        }

        // 기존 점유 영역과 겹치는지 검사
        if (context.occupiedBounds != null)
        {
            Bounds relaxedBounds = new Bounds(
                buildingBounds.center,
                buildingBounds.size * 0.45f
            );

            foreach (Bounds occupied in context.occupiedBounds)
            {
                if (occupied.Intersects(relaxedBounds))
                    return true;
            }
        }

        return false;
    }

    // ObjectArena용 건물 배치
    // 오브젝트 획득과 준보스가 나오는 넓은 전투 구역이므로, 건물을 너무 많이 배치하면 안된다
    private void PlaceArenaBuildings(MapContext context)
    {
        if (context.roadWorldPositions == null || context.roadWorldPositions.Count == 0)
            return;

        float tileSize = context.settings.tileSize;
        float buildingOffset = tileSize * 2.2f;
        float placementChance = context.theme.buildingDensity * 0.5f;

        for (int i = 0; i < context.roadWorldPositions.Count; i += 2)
        {
            Vector3 roadPos = context.roadWorldPositions[i];

            Vector3 forward = GetRoadForward(context, i);
            Vector3 right = Vector3.Cross(Vector3.up, forward).normalized;

            TryPlaceBuildingAtSide(
                context,
                roadPos + right * buildingOffset,
                Quaternion.LookRotation(-right),
                placementChance,
                "ArenaBuilding_Right"
            );

            TryPlaceBuildingAtSide(
                context,
                roadPos - right * buildingOffset,
                Quaternion.LookRotation(right),
                placementChance,
                "ArenaBuilding_Left"
            );
        }

        Debug.Log("[BuildingPlacer] Arena buildings placed.");
    }

    // BossArena용 건물 배치
    // 보스 전투 공간이므로 보스룸 주변은 비워야 한다.
    private void PlaceBossArenaBuildings(MapContext context)
    {
        if (context.roadWorldPositions == null || context.roadWorldPositions.Count == 0)
            return;

        float tileSize = context.settings.tileSize;
        float buildingOffset = tileSize * 2.8f;
        float placementChance = context.theme.buildingDensity * 0.35f;

        for (int i = 0; i < context.roadWorldPositions.Count; i++)
        {
            Vector3 roadPos = context.roadWorldPositions[i];

            if (Vector3.Distance(roadPos, context.bossRoomPosition) < tileSize * 4f)
                continue;

            Vector3 forward = GetRoadForward(context, i);
            Vector3 right = Vector3.Cross(Vector3.up, forward).normalized;

            TryPlaceBuildingAtSide(
                context,
                roadPos + right * buildingOffset,
                Quaternion.LookRotation(-right),
                placementChance,
                "BossStageBuilding_Right"
            );

            TryPlaceBuildingAtSide(
                context,
                roadPos - right * buildingOffset,
                Quaternion.LookRotation(right),
                placementChance,
                "BossStageBuilding_Left"
            );
        }

        Debug.Log("[BuildingPlacer] Boss arena buildings placed.");
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
        // Debris는 Lot 전체를 쓰지 않고 작은 Cluster 영역만 사용
        Bounds debrisClusterBounds = new Bounds(
            lotCenter,
            new Vector3(
                6f,
                5f,
                6f
            )
        );

        if (context.hasMapBounds && !ContainsBoundsXZ(context.mapBounds, debrisClusterBounds))
            return false;

        if (OverlapsAnyRoadTile(context, debrisClusterBounds))
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

    private bool IsTooCloseToOtherBuildings(
    MapContext context,
    Bounds buildingBounds)
    {
        float minDistance =
            context.settings.tileSize * 0.35f;

        foreach (Bounds other in context.buildingBounds)
        {
            float distance = Vector3.Distance(
                other.center,
                buildingBounds.center
            );

            if (distance < minDistance)
                return true;
        }

        return false;
    }
}