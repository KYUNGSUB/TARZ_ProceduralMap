using UnityEngine;

public class ProceduralMapManager : MonoBehaviour
{
    [Header("Data")]
    public ChapterThemeData chapterTheme;
    public MapGenerationSettings settings;

    [Header("Seed")]
    public bool useRandomSeed = true;
    public int seed = 123456;

    [Header("Roots")]
    public Transform mapRoot;
    public Transform runtimeRoot;
    public Transform debugRoot;

    [Header("Generators")]
    public RoadNetworkGenerator roadNetworkGenerator;
    public BuildingPlacer buildingPlacer;
    public POIPlacer poiPlacer;
    public EnvironmentObjectPlacer environmentObjectPlacer;
    public ThrowObjectPlacer throwObjectPlacer;
    public SpawnPointGenerator spawnPointGenerator;

    [Header("Runtime")]
    public PlayerSpawnManager playerSpawnManager;

    private MapContext currentContext;

    private void Start()
    {
        Generate();
    }

    [ContextMenu("Generate Map")]
    public void Generate()
    {
        ClearChildren(mapRoot);
        ClearChildren(runtimeRoot);
        ClearChildren(debugRoot);

        if (useRandomSeed)
            seed = Random.Range(1, int.MaxValue);

        currentContext = new MapContext
        {
            seed = seed,
            random = new System.Random(seed),
            theme = chapterTheme,
            settings = settings,
            mapRoot = mapRoot,
            runtimeRoot = runtimeRoot,
            debugRoot = debugRoot
        };

        if (!ValidateReferences())
            return;

        roadNetworkGenerator.Generate(currentContext);
        buildingPlacer.Place(currentContext);
        poiPlacer.Place(currentContext);
        environmentObjectPlacer.Place(currentContext);
        throwObjectPlacer.Place(currentContext);
        spawnPointGenerator.GenerateEnemySpawns(currentContext);

        if (playerSpawnManager != null)
            playerSpawnManager.SpawnPlayer(currentContext.startPosition);

        Debug.Log($"Map generated. Chapter={chapterTheme.chapterName}, Seed={seed}");
    }

    private bool ValidateReferences()
    {
        if (chapterTheme == null)
        {
            Debug.LogError("ChapterTheme is missing.");
            return false;
        }

        if (settings == null)
        {
            Debug.LogError("MapGenerationSettings is missing.");
            return false;
        }

        if (mapRoot == null || runtimeRoot == null)
        {
            Debug.LogError("Root transforms are missing.");
            return false;
        }

        return true;
    }

    private void ClearChildren(Transform root)
    {
        if (root == null) return;

        for (int i = root.childCount - 1; i >= 0; i--)
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
                DestroyImmediate(root.GetChild(i).gameObject);
            else
                Destroy(root.GetChild(i).gameObject);
#else
            Destroy(root.GetChild(i).gameObject);
#endif
        }
    }
}