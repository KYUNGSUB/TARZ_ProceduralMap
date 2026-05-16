using UnityEngine;
using Unity.AI.Navigation;

public class ProceduralMapManager : MonoBehaviour
{
    [Header("Data")]
    public ChapterThemeData chapterTheme;
    public MapGenerationSettings settings;

    [Header("Seed")]
    public bool useSavedSeed = true;
    public int seed = 123456;

    [Header("Input")]
    public KeyCode regenerateKey = KeyCode.Slash;

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

    [Header("Validation")]
    public NavMeshSurface navMeshSurface;
    public MapValidator mapValidator;
    public int maxGenerationRetry = 10;

    [Header("Runtime")]
    public PlayerSpawnManager playerSpawnManager;

    private MapContext currentContext;

    private void Start()
    {
        LoadOrCreateSeed();

        bool success = Generate();

        if (!success)
        {
            Debug.LogWarning($"Saved seed failed. Creating new seed. Failed Seed={seed}");

            seed = CreateRandomSeed();
            success = Generate();

            if (success)
            {
                SeedStorage.SaveSeed(seed);
                Debug.Log($"Recovered with new valid seed: {seed}");
            }
            else
            {
                Debug.LogError("Failed to recover with new seed.");
            }
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(regenerateKey))
        {
            GenerateNewRandomSeedAndMap();
        }
    }

    private void LoadOrCreateSeed()
    {
        if (useSavedSeed && SeedStorage.HasSavedSeed())
        {
            seed = SeedStorage.LoadSeed();
            Debug.Log($"Loaded saved seed: {seed}");
            return;
        }

        seed = CreateRandomSeed();
        SeedStorage.SaveSeed(seed);

        Debug.Log($"Created new seed: {seed}");
    }

    private void GenerateNewRandomSeedAndMap()
    {
        int maxNewSeedTry = 20;

        for (int i = 0; i < maxNewSeedTry; i++)
        {
            seed = CreateRandomSeed();

            Debug.Log($"Trying new random seed: {seed} ({i + 1}/{maxNewSeedTry})");

            bool success = Generate();

            if (success)
            {
                SeedStorage.SaveSeed(seed);
                Debug.Log($"New valid seed saved: {seed}");
                return;
            }

            Debug.LogWarning($"New seed failed: {seed}");
        }

        Debug.LogError("Failed to generate a valid map after trying multiple new seeds.");
    }

    private int CreateRandomSeed()
    {
        return Random.Range(1, int.MaxValue);
    }

    [ContextMenu("Generate Map")]
    public bool Generate()
    {
        if (!ValidateReferences())
            return false;

        for (int attempt = 0; attempt < maxGenerationRetry; attempt++)
        {
            ClearChildren(mapRoot);
            ClearChildren(runtimeRoot);
            ClearChildren(debugRoot);

            int attemptSeed = seed + attempt;

            currentContext = new MapContext
            {
                seed = attemptSeed,
                random = new System.Random(attemptSeed),

                theme = chapterTheme,
                settings = settings,

                mapRoot = mapRoot,
                runtimeRoot = runtimeRoot,
                debugRoot = debugRoot,

                navMeshSurface = navMeshSurface
            };

            roadNetworkGenerator.Generate(currentContext);
            buildingPlacer.Place(currentContext);
            poiPlacer.Place(currentContext);
            environmentObjectPlacer.Place(currentContext);
            throwObjectPlacer.Place(currentContext);
            spawnPointGenerator.GenerateEnemySpawns(currentContext);

            bool valid = true;

            if (mapValidator != null)
            {
                valid = mapValidator.Validate(currentContext);
            }

            if (!valid)
            {
                Debug.LogWarning(
                    $"Map validation failed. Retry {attempt + 1}/{maxGenerationRetry}, Seed={attemptSeed}"
                );

                continue;
            }

            seed = attemptSeed;

            if (playerSpawnManager != null)
            {
                playerSpawnManager.SpawnPlayer(currentContext.startPosition);
            }

            Debug.Log($"Map generated. Chapter={chapterTheme.chapterName}, Seed={seed}");

            return true;
        }

        Debug.LogError($"Map generation failed after max retry. Base Seed={seed}");

        return false;
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

        if (mapRoot == null)
        {
            Debug.LogError("MapRoot is missing.");
            return false;
        }

        if (runtimeRoot == null)
        {
            Debug.LogError("RuntimeRoot is missing.");
            return false;
        }

        if (debugRoot == null)
        {
            Debug.LogError("DebugRoot is missing.");
            return false;
        }

        if (roadNetworkGenerator == null)
        {
            Debug.LogError("RoadNetworkGenerator is missing.");
            return false;
        }

        if (buildingPlacer == null)
        {
            Debug.LogError("BuildingPlacer is missing.");
            return false;
        }

        if (poiPlacer == null)
        {
            Debug.LogError("POIPlacer is missing.");
            return false;
        }

        if (environmentObjectPlacer == null)
        {
            Debug.LogError("EnvironmentObjectPlacer is missing.");
            return false;
        }

        if (throwObjectPlacer == null)
        {
            Debug.LogError("ThrowObjectPlacer is missing.");
            return false;
        }

        if (spawnPointGenerator == null)
        {
            Debug.LogError("SpawnPointGenerator is missing.");
            return false;
        }

        if (navMeshSurface == null)
        {
            Debug.LogError("NavMeshSurface is missing.");
            return false;
        }

        if (mapValidator == null)
        {
            Debug.LogError("MapValidator is missing.");
            return false;
        }

        return true;
    }

    private void ClearChildren(Transform root)
    {
        if (root == null)
            return;

        for (int i = root.childCount - 1; i >= 0; i--)
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                DestroyImmediate(root.GetChild(i).gameObject);
            }
            else
            {
                Destroy(root.GetChild(i).gameObject);
            }
#else
            Destroy(root.GetChild(i).gameObject);
#endif
        }
    }
}