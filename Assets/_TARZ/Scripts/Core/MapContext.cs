using System.Collections.Generic;
using UnityEngine;

public class MapContext
{
    public int seed;
    public System.Random random;

    public ChapterThemeData theme;
    public MapGenerationSettings settings;

    public Transform mapRoot;
    public Transform runtimeRoot;
    public Transform debugRoot;

    public Vector3 startPosition;
    public Vector3 bossPosition;

    public List<Vector2Int> roadGridPositions = new List<Vector2Int>();
    public List<Vector3> roadWorldPositions = new List<Vector3>();

    public List<Vector3> combatPositions = new List<Vector3>();
    public List<Vector3> secretPositions = new List<Vector3>();
    public List<Vector3> rewardPositions = new List<Vector3>();
    public List<Vector3> enemySpawnPositions = new List<Vector3>();

    public List<Bounds> occupiedBounds = new List<Bounds>();

    // 추가: POI 영역 관리
    public List<POIArea> poiAreas = new List<POIArea>();

    public Vector3 GridToWorld(Vector2Int grid)
    {
        return new Vector3(grid.x * settings.tileSize, 0f, grid.y * settings.tileSize);
    }
}