using System.Collections.Generic;
using UnityEngine;

public class RoadNetworkGenerator : MonoBehaviour
{
    private HashSet<Vector2Int> roadSet = new HashSet<Vector2Int>();

    public void Generate(MapContext context)
    {
        if (context == null)
        {
            Debug.LogError("[RoadNetworkGenerator] Context is null.");
            return;
        }

        switch (context.selectedMapShape)
        {
            case StageMapShapeType.LinearLongRoad:
                GenerateLinearLongRoad(context);
                break;

            case StageMapShapeType.CurvedRoad:
                GenerateCurvedRoad(context);
                break;

            case StageMapShapeType.CityCorridor:
                GenerateCityCorridor(context);
                break;

            case StageMapShapeType.ObjectArena:
                GenerateObjectArena(context);
                break;

            case StageMapShapeType.BranchSecretPath:
                GenerateBranchSecretPath(context);
                break;

            case StageMapShapeType.BossArena:
                GenerateBossArena(context);
                break;

            default:
                GenerateDefault(context);
                break;
        }
    }

    private GameObject GetRoadPrefab(MapContext context)
    {
        if (context.theme == null ||
            context.theme.roadPrefabs == null ||
            context.theme.roadPrefabs.Count == 0)
        {
            Debug.LogError("[RoadNetworkGenerator] Road prefab is missing.");
            return null;
        }

        int index = context.random.Next(0, context.theme.roadPrefabs.Count);
        return context.theme.roadPrefabs[index];
    }

    private void PlaceRoadTile(
    MapContext context,
    Vector3 position,
    Vector3 forward)
    {
        GameObject roadPrefab = GetRoadPrefab(context);

        if (roadPrefab == null)
            return;

        forward.y = 0f;

        if (forward.sqrMagnitude < 0.01f)
            forward = Vector3.forward;

        forward.Normalize();

        Vector3 side = Vector3.Cross(Vector3.up, forward).normalized;

        Quaternion roadRotation = Quaternion.LookRotation(forward, Vector3.up);

        GameObject road = Instantiate(
            roadPrefab,
            position,
            roadRotation,
            context.mapRoot
        );

        road.name = $"Road_{Mathf.RoundToInt(position.x)}_{Mathf.RoundToInt(position.z)}";

        context.roadWorldPositions.Add(position);

        float sidewalkOffset = context.settings.tileSize * 0.55f;

        if (context.theme.sidewalkStraightPrefab != null)
        {
            Vector3 leftSidewalkPos = position - side * sidewalkOffset;
            Vector3 rightSidewalkPos = position + side * sidewalkOffset;

            GameObject left = Instantiate(
                context.theme.sidewalkStraightPrefab,
                leftSidewalkPos,
                roadRotation,
                context.mapRoot
            );

            left.name = "Sidewalk_Straight_Left";

            GameObject right = Instantiate(
                context.theme.sidewalkStraightPrefab,
                rightSidewalkPos,
                roadRotation,
                context.mapRoot
            );

            right.name = "Sidewalk_Straight_Right";
        }

        Bounds roadBounds = new Bounds(
            position,
            new Vector3(
                context.settings.tileSize * 1.6f,
                4f,
                context.settings.tileSize * 1.6f
            )
        );

        context.roadBounds.Add(roadBounds);
    }

    private void PlaceRoadTile(
    MapContext context,
    Vector3 position,
    Quaternion rotation)
    {
        GameObject roadPrefab = GetRoadPrefab(context);

        if (roadPrefab == null)
            return;

        float sidewalkOffset = context.settings.tileSize * 0.5f;

        // 중앙 도로
        GameObject road = Instantiate(
            roadPrefab,
            position,
            rotation,
            context.mapRoot
        );

        road.name = $"Road_{position.x}_{position.z}";

        // 진행 방향 기준 오른쪽 벡터
        Vector3 right = rotation * Vector3.right;

        // 왼쪽 보도
        if (context.theme.sidewalkStraightPrefab != null)
        {
            Vector3 leftPos =
                position - right * sidewalkOffset;

            Instantiate(
                context.theme.sidewalkStraightPrefab,
                leftPos,
                rotation,
                context.mapRoot
            );
        }

        // 오른쪽 보도
        if (context.theme.sidewalkStraightPrefab != null)
        {
            Vector3 rightPos =
                position + right * sidewalkOffset;

            Instantiate(
                context.theme.sidewalkStraightPrefab,
                rightPos,
                rotation,
                context.mapRoot
            );
        }

        context.roadWorldPositions.Add(position);

        float roadWidth = context.settings.tileSize * 3f;
        float roadDepth = context.settings.tileSize;

        Bounds roadBounds = new Bounds(
            position,
            new Vector3(roadWidth, 4f, roadDepth)
        );

        context.roadBounds.Add(roadBounds);
    }

    // Stage 1: LinearLongRoad
    private void GenerateLinearLongRoad(MapContext context)
    {
        float spacing = context.settings.tileSize;
        Vector3 start = new Vector3(-60f, 0f, -30f);

        List<Vector3> path = new List<Vector3>();

        int length = 14;

        for (int i = 0; i < length; i++)
        {
            float curve = Mathf.Sin(i * 0.45f) * 8f;

            Vector3 pos = start + new Vector3(
                i * spacing,
                0f,
                curve
            );

            path.Add(pos);
        }

        CenterPathToOrigin(path);

        // 도로를 중앙보다 약간 위로 이동
        MovePath(path, new Vector3(0f, 0f, context.settings.tileSize * 2.2f));

        // 반드시 path 보정이 끝난 후 위치 저장
        context.startPosition = path[0];
        context.exitPosition = path[path.Count - 1];

        for (int i = 0; i < path.Count; i++)
        {
            Vector3 forward;

            if (i == 0)
                forward = path[i + 1] - path[i];
            else if (i == path.Count - 1)
                forward = path[i] - path[i - 1];
            else
                forward = path[i + 1] - path[i - 1];

            PlaceRoadTile(context, path[i], forward);
        }

        UpdateMapBoundsFromPath(context, path, spacing * 7.0f);

        Debug.Log("[RoadNetworkGenerator] Generated Stage 1 LinearLongRoad.");
    }

    private void MovePath(List<Vector3> path, Vector3 offset)
    {
        if (path == null)
            return;

        for (int i = 0; i < path.Count; i++)
            path[i] += offset;
    }

    // MapBoundary 기준 Bounds 저장 메서드
    private void UpdateMapBoundsFromPath(
    MapContext context,
    List<Vector3> path,
    float padding)
    {
        if (path == null || path.Count == 0)
            return;

        Bounds bounds = new Bounds(path[0], Vector3.zero);

        for (int i = 1; i < path.Count; i++)
            bounds.Encapsulate(path[i]);

        bounds.Expand(new Vector3(padding, 10f, padding));

        context.mapBounds = bounds;
        context.hasMapBounds = true;
    }

    // Road 경로를 Map 중앙으로 보정
    private void CenterPathToOrigin(List<Vector3> path)
    {
        if (path == null || path.Count == 0)
            return;

        Bounds bounds = new Bounds(path[0], Vector3.zero);

        for (int i = 1; i < path.Count; i++)
            bounds.Encapsulate(path[i]);

        Vector3 offset = new Vector3(
            -bounds.center.x,
            0f,
            -bounds.center.z
        );

        for (int i = 0; i < path.Count; i++)
            path[i] += offset;
    }

    // Stage 2: CurvedRoad
    private void GenerateCurvedRoad(MapContext context)
    {
        float spacing = context.settings.tileSize;
        Vector3 start = new Vector3(-45f, 0f, -35f);

        context.startPosition = start;

        int length = 11;

        for (int i = 0; i < length; i++)
        {
            float x = i * spacing;
            float z = Mathf.Sin(i * 0.8f) * 15f;

            Vector3 pos = start + new Vector3(x, 0f, z);

            float angle = Mathf.Cos(i * 0.8f) * 15f;
            PlaceRoadTile(context, pos, Quaternion.Euler(0f, angle, 0f));
        }

        context.exitPosition = start + new Vector3(
            (length - 1) * spacing,
            0f,
            Mathf.Sin((length - 1) * 0.8f) * 15f
        );

        Debug.Log("[RoadNetworkGenerator] Generated Stage 2 CurvedRoad.");
    }

    // Stage 3: CityCorridor (건물 사이를 지나는 도심 통로형 구조)
    private void GenerateCityCorridor(MapContext context)
    {
        float spacing = context.settings.tileSize;
        Vector3 start = new Vector3(-40f, 0f, -40f);

        context.startPosition = start;

        int length = 12;

        for (int i = 0; i < length; i++)
        {
            Vector3 pos = start + new Vector3(
                0f,
                0f,
                i * spacing
            );

            PlaceRoadTile(context, pos, Quaternion.Euler(0f, 90f, 0f));
        }

        context.exitPosition = start + new Vector3(
            0f,
            0f,
            (length - 1) * spacing
        );

        Debug.Log("[RoadNetworkGenerator] Generated Stage 3 CityCorridor.");
    }

    // Stage 4: ObjectArena (오브젝트 획득 구간과 준보스 전토 공강이 있는 구조)
    private void GenerateObjectArena(MapContext context)
    {
        float spacing = context.settings.tileSize;
        Vector3 start = new Vector3(-45f, 0f, -20f);

        context.startPosition = start;

        for (int i = 0; i < 7; i++)
        {
            Vector3 pos = start + new Vector3(i * spacing, 0f, 0f);
            PlaceRoadTile(context, pos, Quaternion.identity);
        }

        Vector3 arenaCenter = start + new Vector3(7 * spacing, 0f, 0f);

        for (int x = -2; x <= 2; x++)
        {
            for (int z = -2; z <= 2; z++)
            {
                Vector3 pos = arenaCenter + new Vector3(
                    x * spacing,
                    0f,
                    z * spacing
                );

                PlaceRoadTile(context, pos, Quaternion.identity);
            }
        }

        context.midBossPosition = arenaCenter;
        context.exitPosition = arenaCenter + new Vector3(3 * spacing, 0f, 0f);

        Debug.Log("[RoadNetworkGenerator] Generated Stage 4 ObjectArena.");
    }

    // Stage 5: BranchSecretPath (갈림길이 있고, 왼쪽은 비밀방 입구, 오른쪽은 다음 Stage로 진행하는 구조)
    private void GenerateBranchSecretPath(MapContext context)
    {
        float spacing = context.settings.tileSize;
        Vector3 start = new Vector3(-45f, 0f, -30f);

        context.startPosition = start;

        int mainLength = 7;

        for (int i = 0; i < mainLength; i++)
        {
            Vector3 pos = start + new Vector3(i * spacing, 0f, 0f);
            PlaceRoadTile(context, pos, Quaternion.identity);
        }

        Vector3 branchPoint = start + new Vector3(4 * spacing, 0f, 0f);

        // 오른쪽 길: 다음 스테이지로 진행
        for (int i = 1; i <= 5; i++)
        {
            Vector3 pos = branchPoint + new Vector3(
                i * spacing,
                0f,
                i * spacing * 0.6f
            );

            PlaceRoadTile(context, pos, Quaternion.Euler(0f, 25f, 0f));
        }

        context.exitPosition = branchPoint + new Vector3(
            5 * spacing,
            0f,
            5 * spacing * 0.6f
        );

        // 왼쪽 길: 막힌 구간 + 비밀방 입구
        for (int i = 1; i <= 4; i++)
        {
            Vector3 pos = branchPoint + new Vector3(
                i * spacing,
                0f,
                -i * spacing * 0.7f
            );

            PlaceRoadTile(context, pos, Quaternion.Euler(0f, -25f, 0f));
        }

        context.secretRoomPosition = branchPoint + new Vector3(
            4 * spacing,
            0f,
            -4 * spacing * 0.7f
        );

        Debug.Log("[RoadNetworkGenerator] Generated Stage 5 BranchSecretPath.");
    }

    // Stage 6: BossArena (짧은 진입로 후 넓은 보스 전투장)
    private void GenerateBossArena(MapContext context)
    {
        float spacing = context.settings.tileSize;
        Vector3 start = new Vector3(-35f, 0f, 0f);

        context.startPosition = start;

        for (int i = 0; i < 5; i++)
        {
            Vector3 pos = start + new Vector3(i * spacing, 0f, 0f);
            PlaceRoadTile(context, pos, Quaternion.identity);
        }

        Vector3 bossArenaCenter = start + new Vector3(6 * spacing, 0f, 0f);

        for (int x = -3; x <= 3; x++)
        {
            for (int z = -3; z <= 3; z++)
            {
                Vector3 pos = bossArenaCenter + new Vector3(
                    x * spacing,
                    0f,
                    z * spacing
                );

                PlaceRoadTile(context, pos, Quaternion.identity);
            }
        }

        context.bossRoomPosition = bossArenaCenter;
        context.exitPosition = bossArenaCenter + new Vector3(4 * spacing, 0f, 0f);

        Debug.Log("[RoadNetworkGenerator] Generated Stage 6 BossArena.");
    }

    private void GenerateDefault(MapContext context)
    {
        roadSet.Clear();

        Vector2Int current = Vector2Int.zero;
        Vector2Int direction = Vector2Int.up;

        context.startPosition = context.GridToWorld(current);
        AddRoad(context, current);

        for (int i = 1; i < context.settings.mainPathLength; i++)
        {
            Vector2Int nextDirection = ChooseDirection(context, current, direction);
            Vector2Int next = current + nextDirection;

            // 이미 있는 도로라면 교차로로 허용
            if (roadSet.Contains(next))
            {
                current = next;
                direction = nextDirection;
                continue;
            }

            if (!CanPlaceRoad(context, next, current))
                continue;

            current = next;
            direction = nextDirection;

            AddRoad(context, current);

            if (i % 3 == 0 && i < context.settings.mainPathLength - 2)
            {
                context.combatPositions.Add(context.GridToWorld(current));
            }

            if (context.random.NextDouble() < context.settings.branchChance)
                TryCreateBranch(context, current, direction);
        }

        context.bossRoomPosition = context.GridToWorld(current);
        Debug.Log($"Boss position set to last road: {current}");
        CalculateMapBounds(context);
    }

    private void CalculateMapBounds(MapContext context)
    {
        if (context.roadBounds == null || context.roadBounds.Count == 0)
            return;

        Bounds bounds = context.roadBounds[0];

        for (int i = 1; i < context.roadBounds.Count; i++)
        {
            bounds.Encapsulate(context.roadBounds[i]);
        }

        // Road 주변에 건물과 Block이 들어갈 수 있도록 여유 공간 추가
        float padding = 50f + context.maxBuildingHalfExtent;

        bounds.Expand(new Vector3(padding * 2f, 0f, padding * 2f));

        context.mapBounds = bounds;
        context.hasMapBounds = true;

        Debug.Log($"Map Bounds calculated. Center={bounds.center}, Size={bounds.size}");
    }

    private void AddRoad(MapContext context, Vector2Int grid)
    {
        if (roadSet.Contains(grid))
            return;

        roadSet.Add(grid);
        context.roadGridPositions.Add(grid);

        Vector3 worldPosition = context.GridToWorld(grid);
        context.roadWorldPositions.Add(worldPosition);

        GameObject prefab = PrefabPicker.Pick(context.theme.roadPrefabs, context.random);
        if (prefab == null) return;

        GameObject road = Instantiate(prefab, worldPosition, Quaternion.identity, context.mapRoot);
        road.name = $"Road_{grid.x}_{grid.y}";

        Bounds bounds = BoundsUtility.GetObjectBounds(road);
        context.occupiedBounds.Add(bounds);
    }

    private Vector2Int ChooseDirection(MapContext context, Vector2Int current, Vector2Int currentDirection)
    {
        List<Vector2Int> candidates = new List<Vector2Int>();

        // 직진을 우선 후보로 둠
        candidates.Add(currentDirection);

        // 좌우 회전 후보
        if (currentDirection == Vector2Int.up || currentDirection == Vector2Int.down)
        {
            candidates.Add(Vector2Int.left);
            candidates.Add(Vector2Int.right);
        }
        else
        {
            candidates.Add(Vector2Int.up);
            candidates.Add(Vector2Int.down);
        }

        // 낮은 확률로 다른 방향도 허용
        candidates.Add(Vector2Int.up);
        candidates.Add(Vector2Int.right);
        candidates.Add(Vector2Int.down);
        candidates.Add(Vector2Int.left);

        Shuffle(candidates, context.random);

        foreach (Vector2Int dir in candidates)
        {
            if (dir == -currentDirection)
                continue;

            Vector2Int next = current + dir;

            if (roadSet.Contains(next))
                return dir;

            if (CanPlaceRoad(context, next, current))
                return dir;
        }

        return currentDirection;
    }

    private void TryCreateBranch(MapContext context, Vector2Int origin, Vector2Int mainDirection)
    {
        Vector2Int branchDir = GetPerpendicularDirection(context, mainDirection);

        int length = context.random.Next(
            context.settings.branchMinLength,
            context.settings.branchMaxLength + 1
        );

        List<Vector2Int> branchPositions = new List<Vector2Int>();
        Vector2Int current = origin;

        for (int i = 0; i < length; i++)
        {
            Vector2Int next = current + branchDir;

            // 이미 있는 도로와 만나면 교차로로 허용하고 종료
            if (roadSet.Contains(next))
                break;

            if (!CanPlaceRoad(context, next, current))
                break;

            branchPositions.Add(next);
            current = next;
        }

        if (branchPositions.Count == 0)
            return;

        foreach (Vector2Int grid in branchPositions)
            AddRoad(context, grid);

        Vector3 endPosition = context.GridToWorld(branchPositions[branchPositions.Count - 1]);

        if (context.random.NextDouble() < 0.5)
            context.secretPositions.Add(endPosition);
        else
            context.rewardPositions.Add(endPosition);
    }

    private bool CanPlaceRoad(MapContext context, Vector2Int target, Vector2Int from)
    {
        if (roadSet.Contains(target))
            return true;

        // 상하좌우 바로 인접한 기존 도로 검사
        Vector2Int[] neighbors =
        {
            Vector2Int.up,
            Vector2Int.right,
            Vector2Int.down,
            Vector2Int.left
        };

        foreach (Vector2Int n in neighbors)
        {
            Vector2Int neighbor = target + n;

            if (!roadSet.Contains(neighbor))
                continue;

            // 방금 이어지는 이전 도로는 허용
            if (neighbor == from)
                continue;

            // 그 외 인접 기존 도로는 겹쳐 보일 가능성이 있으므로 금지
            return false;
        }

        return true;
    }

    private Vector2Int GetPerpendicularDirection(MapContext context, Vector2Int dir)
    {
        if (dir == Vector2Int.up || dir == Vector2Int.down)
        {
            return context.random.NextDouble() < 0.5
                ? Vector2Int.left
                : Vector2Int.right;
        }

        return context.random.NextDouble() < 0.5
            ? Vector2Int.up
            : Vector2Int.down;
    }

    private void Shuffle(List<Vector2Int> list, System.Random random)
    {
        for (int i = 0; i < list.Count; i++)
        {
            int rand = random.Next(i, list.Count);
            Vector2Int temp = list[i];
            list[i] = list[rand];
            list[rand] = temp;
        }
    }
}