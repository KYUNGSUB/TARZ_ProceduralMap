using UnityEngine;

public class RoadNetworkGenerator : MonoBehaviour
{
    public void Generate(MapContext context)
    {
        Vector2Int current = Vector2Int.zero;
        Vector2Int direction = Vector2Int.up;

        context.startPosition = context.GridToWorld(current);

        for (int i = 0; i < context.settings.mainPathLength; i++)
        {
            AddRoad(context, current);

            if (i > 0 && i % 3 == 0)
                context.combatPositions.Add(context.GridToWorld(current));

            if (ShouldCreateBranch(context))
                CreateBranch(context, current, direction);

            if (ShouldTurn(context))
                direction = GetNewDirection(context, direction);

            current += direction;
        }

        context.bossPosition = context.GridToWorld(current);
        AddRoad(context, current);
    }

    private void AddRoad(MapContext context, Vector2Int grid)
    {
        if (context.roadGridPositions.Contains(grid))
            return;

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

    private void CreateBranch(MapContext context, Vector2Int origin, Vector2Int mainDirection)
    {
        Vector2Int branchDir = GetPerpendicularDirection(context, mainDirection);
        int length = context.random.Next(
            context.settings.branchMinLength,
            context.settings.branchMaxLength + 1
        );

        Vector2Int current = origin + branchDir;

        for (int i = 0; i < length; i++)
        {
            AddRoad(context, current);
            current += branchDir;
        }

        Vector3 endPosition = context.GridToWorld(current - branchDir);

        if (context.random.NextDouble() < 0.5)
            context.secretPositions.Add(endPosition);
        else
            context.rewardPositions.Add(endPosition);
    }

    private bool ShouldTurn(MapContext context)
    {
        return context.random.NextDouble() < context.settings.turnChance;
    }

    private bool ShouldCreateBranch(MapContext context)
    {
        return context.random.NextDouble() < context.settings.branchChance;
    }

    private Vector2Int GetNewDirection(MapContext context, Vector2Int current)
    {
        Vector2Int[] candidates =
        {
            Vector2Int.up,
            Vector2Int.right,
            Vector2Int.left
        };

        return candidates[context.random.Next(candidates.Length)];
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
}