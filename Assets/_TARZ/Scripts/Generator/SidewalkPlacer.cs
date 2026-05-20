using System.Collections.Generic;
using UnityEngine;

public class SidewalkPlacer : MonoBehaviour
{
    [Header("Sidewalk Settings")]
    public float sidewalkWidth = 4f;
    public float sidewalkHeight = 0.05f;
    public float yOffset = 0.12f;

    public void Place(MapContext context)
    {
        if (context.theme.sidewalkStraightPrefab == null)
        {
            Debug.LogWarning("Sidewalk_Straight prefab is missing in ChapterThemeData.");
            return;
        }

        HashSet<Vector2Int> roadSet = new HashSet<Vector2Int>(context.roadGridPositions);

        foreach (Vector2Int roadGrid in context.roadGridPositions)
        {
            PlaceSidewalkForRoadTile(context, roadGrid, roadSet);
        }
    }

    private void PlaceSidewalkForRoadTile(
        MapContext context,
        Vector2Int roadGrid,
        HashSet<Vector2Int> roadSet
    )
    {
        Vector3 roadWorld = context.GridToWorld(roadGrid);

        bool hasUp = roadSet.Contains(roadGrid + Vector2Int.up);
        bool hasDown = roadSet.Contains(roadGrid + Vector2Int.down);
        bool hasLeft = roadSet.Contains(roadGrid + Vector2Int.left);
        bool hasRight = roadSet.Contains(roadGrid + Vector2Int.right);

        bool vertical = hasUp || hasDown;
        bool horizontal = hasLeft || hasRight;

        // 세로 도로: 좌우에 보도 생성
        if (vertical && !horizontal)
        {
            // 왼쪽이 비어 있을 때만
            if (!roadSet.Contains(roadGrid + Vector2Int.left))
            {
                PlaceStraightSidewalk(
                    context,
                    roadWorld,
                    Vector3.left,
                    Quaternion.identity
                );
            }

            // 오른쪽이 비어 있을 때만
            if (!roadSet.Contains(roadGrid + Vector2Int.right))
            {
                PlaceStraightSidewalk(
                    context,
                    roadWorld,
                    Vector3.right,
                    Quaternion.identity
                );
            }

            return;
        }

        // 가로 도로: 위아래에 보도 생성
        if (horizontal && !vertical)
        {
            if (!roadSet.Contains(roadGrid + Vector2Int.up))
            {
                PlaceStraightSidewalk(
                    context,
                    roadWorld,
                    Vector3.forward,
                    Quaternion.Euler(0f, 90f, 0f)
                );
            }

            if (!roadSet.Contains(roadGrid + Vector2Int.down))
            {
                PlaceStraightSidewalk(
                    context,
                    roadWorld,
                    Vector3.back,
                    Quaternion.Euler(0f, 90f, 0f)
                );
            }

            return;
        }

        // 코너 또는 교차로: 우선 4방향에 짧은 보도 생성
        if (vertical && horizontal)
        {
            /*
            PlaceStraightSidewalk(context, roadWorld, Vector3.left, Quaternion.identity);
            PlaceStraightSidewalk(context, roadWorld, Vector3.right, Quaternion.identity);
            PlaceStraightSidewalk(context, roadWorld, Vector3.forward, Quaternion.Euler(0f, 90f, 0f));
            PlaceStraightSidewalk(context, roadWorld, Vector3.back, Quaternion.Euler(0f, 90f, 0f));

            PlaceCornerSidewalks(context, roadWorld);
            */
            return;
        }

        // 외딴 도로 또는 끝 도로
        PlaceStraightSidewalk(context, roadWorld, Vector3.left, Quaternion.identity);
        PlaceStraightSidewalk(context, roadWorld, Vector3.right, Quaternion.identity);
    }

    private void PlaceStraightSidewalk(
        MapContext context,
        Vector3 roadWorld,
        Vector3 sideDirection,
        Quaternion rotation
    )
    {
        float roadHalf = context.settings.tileSize * 0.5f;
        float sidewalkHalf = sidewalkWidth * 0.5f;

        Vector3 position =
            roadWorld +
            sideDirection.normalized * (roadHalf + sidewalkHalf);

        position.y = yOffset;

        GameObject obj = Instantiate(
            context.theme.sidewalkStraightPrefab,
            position,
            rotation,
            context.mapRoot
        );

        obj.name = "Sidewalk_Straight";
    }

    private void PlaceCornerSidewalks(MapContext context, Vector3 roadWorld)
    {
        if (context.theme.sidewalkCornerPrefab == null)
            return;

        float roadHalf = context.settings.tileSize * 0.5f;
        float sidewalkHalf = sidewalkWidth * 0.5f;

        Vector3[] cornerDirs =
        {
            new Vector3(1f, 0f, 1f).normalized,
            new Vector3(-1f, 0f, 1f).normalized,
            new Vector3(1f, 0f, -1f).normalized,
            new Vector3(-1f, 0f, -1f).normalized
        };

        float offset = roadHalf + sidewalkHalf;

        for (int i = 0; i < cornerDirs.Length; i++)
        {
            Vector3 position = roadWorld + cornerDirs[i] * offset;
            position.y = yOffset;

            Quaternion rotation = Quaternion.Euler(0f, i * 90f, 0f);

            GameObject obj = Instantiate(
                context.theme.sidewalkCornerPrefab,
                position,
                rotation,
                context.mapRoot
            );

            obj.name = "Sidewalk_Corner";
        }
    }
}