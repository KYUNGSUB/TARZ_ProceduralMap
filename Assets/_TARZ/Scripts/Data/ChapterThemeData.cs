using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "TARZ/Map/Chapter Theme")]
public class ChapterThemeData : ScriptableObject
{
    [Header("Basic")]
    public string chapterId;
    public string chapterName;

    [Header("TARZ Chapter Info")]
    public int chapterNumber;
    public string chapterDisplayName;
    [TextArea(2, 5)]
    public string chapterDescription;

    [Header("Stage Flow")]
    public int stageCount = 6;
    public List<StageNodeType> stageFlow = new List<StageNodeType>();

    [Header("Stage Map Shape")]
    public List<StageMapShapeType> stageMapShapes = new List<StageMapShapeType>();

    [Header("Road Prefabs")]
    public List<GameObject> roadPrefabs = new List<GameObject>();

    [Header("Sidewalk Prefabs")]
    public GameObject sidewalkStraightPrefab;
    public GameObject sidewalkCornerPrefab;

    [Header("Building Prefabs")]
    public List<GameObject> buildingPrefabs = new List<GameObject>();

    [Header("Environment Prefabs")]
    public List<GameObject> treePrefabs = new List<GameObject>();
    public List<GameObject> facilityPrefabs = new List<GameObject>();
    public List<GameObject> debrisPrefabs = new List<GameObject>();

    [Header("Throw Object Prefabs")]
    public List<GameObject> throwObjectPrefabs = new List<GameObject>();

    [Header("POI Prefabs")]
    public List<GameObject> startPrefabs = new List<GameObject>();
    public List<GameObject> combatPrefabs = new List<GameObject>();
    public List<GameObject> secretPrefabs = new List<GameObject>();
    public List<GameObject> rewardPrefabs = new List<GameObject>();
    public List<GameObject> bossPrefabs = new List<GameObject>();
    public List<GameObject> exitPrefabs = new List<GameObject>();

    [Header("Characters - Legacy")]
    public GameObject enemyPrefab;

    [Header("Enemy Spawn Rules")]
    public List<EnemySpawnRule> enemySpawnRules = new List<EnemySpawnRule>();

    [Header("Boss")]
    public GameObject chapterBossPrefab;
    public string bossName;
    [TextArea(2, 5)]
    public string bossDescription;

    [Header("Secret Room")]
    public bool hasSecretRoom = true;
    public List<GameObject> secretRoomPrefabs = new List<GameObject>();

    [Header("Theme Density")]
    [Range(0f, 1f)] public float buildingDensity = 0.7f;
    [Range(0f, 1f)] public float environmentDensity = 0.6f;
    [Range(0f, 1f)] public float throwObjectDensity = 0.8f;

    [Header("District Rule")]
    public DistrictThemeRule districtThemeRule;

    [Header("District Building Prefabs")]
    public List<GameObject> residentialBuildingPrefabs = new List<GameObject>();
    public List<GameObject> commercialBuildingPrefabs = new List<GameObject>();
    public List<GameObject> industrialBuildingPrefabs = new List<GameObject>();
    public List<GameObject> harborBuildingPrefabs = new List<GameObject>();
    public List<GameObject> beachBuildingPrefabs = new List<GameObject>();
}