using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "TARZ/Map/Chapter Theme")]
public class ChapterThemeData : ScriptableObject
{
    [Header("Basic")]
    public string chapterId;
    public string chapterName;

    [Header("Road Prefabs")]
    public List<GameObject> roadPrefabs = new List<GameObject>();

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

    [Header("Characters")]
    public GameObject enemyPrefab;

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