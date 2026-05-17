using System.Collections;
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

    [Header("Retry")]
    public int maxGenerationRetry = 10;

    [Header("Runtime")]
    public PlayerSpawnManager playerSpawnManager;

    private MapContext currentContext;
    private bool isGenerating = false;

    [Header("Combat")]
    public CombatZoneRule combatZoneRule;
    public CombatZoneGenerator combatZoneGenerator;

    private void Start()
    {
        LoadOrCreateSeed();
        StartCoroutine(GenerateWithRetry(seed, true));
    }

    private void Update()
    {
        if (Input.GetKeyDown(regenerateKey))
        {
            if (isGenerating)
            {
                Debug.LogWarning("Map generation is already running. Input ignored.");
                return;
            }

            int newSeed = CreateRandomSeed();
            StartCoroutine(GenerateWithRetry(newSeed, true));
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

    private int CreateRandomSeed()
    {
        return Random.Range(1, int.MaxValue);
    }

    private IEnumerator GenerateWithRetry(int baseSeed, bool saveWhenSuccess)
    {
        if (!ValidateReferences())
            yield break;

        isGenerating = true;

        for (int attempt = 0; attempt < maxGenerationRetry; attempt++)
        {
            int attemptSeed = baseSeed + attempt;

            Debug.Log($"Generating map. Attempt={attempt + 1}/{maxGenerationRetry}, Seed={attemptSeed}");

            ClearChildren(mapRoot);
            ClearChildren(runtimeRoot);
            ClearChildren(debugRoot);

            // 중요: Destroy()가 실제 처리될 시간을 줌
            yield return null;

            bool success = GenerateOnce(attemptSeed);

            if (success)
            {
                seed = attemptSeed;

                if (saveWhenSuccess)
                    SeedStorage.SaveSeed(seed);

                Debug.Log($"Map generated successfully. Chapter={chapterTheme.chapterName}, Seed={seed}");

                isGenerating = false;
                yield break;
            }

            Debug.LogWarning($"Map validation failed. Seed={attemptSeed}");

            // 다음 재시도 전 한 프레임 대기
            yield return null;
        }

        Debug.LogError($"Map generation failed after max retry. Base Seed={baseSeed}");

        isGenerating = false;
    }

    private bool GenerateOnce(int generationSeed)
    {
        currentContext = new MapContext
        {
            seed = generationSeed,
            random = new System.Random(generationSeed),

            theme = chapterTheme,
            settings = settings,
            combatZoneRule = combatZoneRule,

            mapRoot = mapRoot,
            runtimeRoot = runtimeRoot,
            debugRoot = debugRoot,

            navMeshSurface = navMeshSurface
        };

        roadNetworkGenerator.Generate(currentContext);

        // 현재 테스트 중이면 주석 유지 가능
        buildingPlacer.Place(currentContext);

        poiPlacer.Place(currentContext);

        // 현재 테스트 중이면 주석 유지 가능
        environmentObjectPlacer.Place(currentContext);

        // 전투 공간 규칙 적용
        combatZoneGenerator.Generate(currentContext);

        bool valid = true;

        if (mapValidator != null)
            valid = mapValidator.Validate(currentContext);

        if (!valid)
            return false;

        // NavMesh 검증 성공 후 동적 오브젝트 생성
        // throwObjectPlacer.Place(currentContext);
        spawnPointGenerator.GenerateEnemySpawns(currentContext);

        if (playerSpawnManager != null)
            playerSpawnManager.SpawnPlayer(currentContext.startPosition);

        return true;
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

        if (poiPlacer == null)
        {
            Debug.LogError("POIPlacer is missing.");
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
            Destroy(root.GetChild(i).gameObject);
        }
    }
}