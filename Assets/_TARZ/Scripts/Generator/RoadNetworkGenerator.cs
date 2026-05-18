using System.Collections.Generic;
using UnityEngine;

public class RoadNetworkGenerator : MonoBehaviour
{
    private HashSet<Vector2Int> roadSet = new HashSet<Vector2Int>();

    public void Generate(MapContext context)
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

        context.bossPosition = context.GridToWorld(current);
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
        float padding = 35f;

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