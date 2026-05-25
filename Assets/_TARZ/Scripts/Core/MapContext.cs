using System.Collections.Generic;
using UnityEngine;
using Unity.AI.Navigation;

public class MapContext
{
    public int seed;
    public System.Random random;

    public ChapterThemeData theme;
    public MapGenerationSettings settings;
    public CombatZoneRule combatZoneRule;
    public BuildingPlacementRule buildingPlacementRule;

    public Transform mapRoot;
    public Transform runtimeRoot;
    public Transform debugRoot;

    public Vector3 startPosition;
//    public Vector3 bossPosition;

    public Vector3 midBossPosition;
    public Vector3 bossRoomPosition;

    public Vector3 exitPosition;
    public Vector3 secretRoomPosition;

    public List<Vector2Int> roadGridPositions = new List<Vector2Int>();
    public List<Vector3> roadWorldPositions = new List<Vector3>();

    public List<Vector3> combatPositions = new List<Vector3>();
    public List<Vector3> secretPositions = new List<Vector3>();
    public List<Vector3> rewardPositions = new List<Vector3>();
    public List<Vector3> enemySpawnPositions = new List<Vector3>();

    public List<Bounds> occupiedBounds = new List<Bounds>();
    public List<Bounds> roadBounds = new List<Bounds>();
    public List<Bounds> buildingBounds = new List<Bounds>();
    public List<CityBlock> cityBlocks = new List<CityBlock>();
    public List<Bounds> blockBounds = new List<Bounds>();

    public Bounds mapBounds;
    public bool hasMapBounds = false;

    public float maxBuildingHalfExtent = 10f;

    // 추가: POI 영역 관리
    public List<POIArea> poiAreas = new List<POIArea>();

    public NavMeshSurface navMeshSurface;

    public List<LotData> lots = new List<LotData>();

    public int selectedStage;
    public StageNodeType selectedStageType;
    public StageMapShapeType selectedMapShape;

    public List<CombatZoneArea> combatZones = new List<CombatZoneArea>();

    public Vector3 GridToWorld(Vector2Int grid)
    {
        return new Vector3(grid.x * settings.tileSize, 0f, grid.y * settings.tileSize);
    }
}