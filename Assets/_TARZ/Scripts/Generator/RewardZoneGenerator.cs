using UnityEngine;

public class RewardZoneGenerator : MonoBehaviour
{
    [Header("Reward Prefabs")]
    public GameObject[] rewardObjectPrefabs;

    [Header("Main Reward Box")]
    public GameObject rewardBoxPrefab;

    [Header("Reward Cover")]
    public GameObject rewardCoverPrefab;

    [Header("Placement")]
    public int minObjectCount = 3;
    public int maxObjectCount = 5;
    public float placementRadius = 5f;

    [Header("Validation")]
    public float minDistanceFromPOI = 2f;
    public float occupiedCheckSize = 2f;

    public void Generate(MapContext context)
    {
        if (context == null)
            return;

        if (rewardObjectPrefabs == null || rewardObjectPrefabs.Length == 0)
        {
            Debug.LogWarning("[RewardZoneGenerator] Reward prefabs are missing.");
            return;
        }

        int totalPlaced = 0;

        // 일반 Reward Zone 처리
        if (context.rewardPositions != null)
        {
            foreach (Vector3 rewardPos in context.rewardPositions)
            {
                totalPlaced += GenerateRewardObjects(context, rewardPos);
            }
        }

        // Stage 5 SecretRoomEntrance 처리
        if (context.selectedStageType == StageNodeType.SecretRoomEntrance &&
            context.secretPositions != null)
        {
            foreach (Vector3 secretPos in context.secretPositions)
            {
                totalPlaced += GenerateRewardObjects(context, secretPos);
            }
        }

        Debug.Log($"[RewardZoneGenerator] Reward objects placed: {totalPlaced}");
    }

    private int GenerateRewardObjects(MapContext context, Vector3 center)
    {
        int placed = 0;

        // 1. Reward Zone 중앙에 RewardBox 1개 고정 생성
        if (rewardBoxPrefab != null)
        {
            Vector3 boxPos = center;
            boxPos.y = 6.0f;

            GameObject rewardBox = Instantiate(
                rewardBoxPrefab,
                boxPos,
                Quaternion.identity,
                context.mapRoot
            );

            rewardBox.name = "RewardBox";

            Debug.Log($"RewardBox Position = {boxPos}");

            Bounds boxBounds = BoundsUtility.GetObjectBounds(rewardBox);
            context.occupiedBounds.Add(boxBounds);

            placed++;
        }

        // 2. Stage 5 Secret Room에서는 보상 오브젝트를 강제로 조금 배치
        if (context.selectedStageType == StageNodeType.SecretRoomEntrance)
        {
            int secretCount = context.random.Next(3, 6);

            for (int i = 0; i < secretCount; i++)
            {
                GameObject prefab = PickPrefab(context);

                if (prefab == null)
                    continue;

                Vector3 pos = GetSecretRewardPosition(context, center);

                Quaternion rot = Quaternion.Euler(
                    0f,
                    RandomRange(context, 0f, 360f),
                    0f
                );

                GameObject obj = Instantiate(
                    prefab,
                    pos,
                    rot,
                    context.mapRoot
                );

                obj.name = "SecretRewardObject";

                Debug.Log($"SecretRewardObject = {pos}");

                Bounds bounds = BoundsUtility.GetObjectBounds(obj);
                context.occupiedBounds.Add(bounds);

                placed++;
            }

            CreateRewardCover(context, center);

            return placed;
        }

        // 3. 일반 Reward Zone 처리
        CreateRewardCover(context, center);

        int count = context.random.Next(minObjectCount, maxObjectCount + 1);
        int attempts = 0;
        int maxAttempts = count * 10;

        while (placed < count + 1 && attempts < maxAttempts)
        {
            attempts++;

            Vector3 pos = GetRandomPosition(context, center);

            if (!CanPlace(context, pos))
                continue;

            GameObject prefab = PickPrefab(context);

            if (prefab == null)
                continue;

            Quaternion rot = Quaternion.Euler(
                0f,
                RandomRange(context, 0f, 360f),
                0f
            );

            GameObject obj = Instantiate(
                prefab,
                pos,
                rot,
                context.mapRoot
            );

            obj.name = "RewardObject";

            Bounds bounds = BoundsUtility.GetObjectBounds(obj);
            context.occupiedBounds.Add(bounds);

            placed++;
        }

        return placed;
    }

    private Vector3 GetSecretRewardPosition(
    MapContext context,
    Vector3 center)
    {
        float radius = 4f;

        float angle = RandomRange(context, 0f, Mathf.PI * 2f);
        float distance = RandomRange(context, 1.5f, radius);

        Vector3 offset = new Vector3(
            Mathf.Cos(angle) * distance,
            0f,
            Mathf.Sin(angle) * distance
        );

        Vector3 pos = center + offset;
        pos.y = 4.0f;

        return pos;
    }

    private void CreateRewardCover(
    MapContext context,
    Vector3 center)
    {
        if (rewardCoverPrefab == null)
            return;

        Vector3[] offsets;

        if (context.selectedStageType == StageNodeType.SecretRoomEntrance)
        {
            offsets = new Vector3[]
            {
            new Vector3( 4f, 0f, 0f),
            new Vector3(-4f, 0f, 0f)
            };
        }
        else
        {
            offsets = new Vector3[]
            {
            new Vector3( 4f, 0f, 0f),
            new Vector3(-4f, 0f, 0f),
            new Vector3( 0f, 0f, 4f),
            new Vector3( 0f, 0f,-4f)
            };
        }

        foreach (Vector3 offset in offsets)
        {
            Vector3 pos = center + offset;
            pos.y = 1.0f;

            GameObject cover = Instantiate(
                rewardCoverPrefab,
                pos,
                Quaternion.identity,
                context.mapRoot
            );

            cover.name = "RewardCover";

            Bounds bounds = BoundsUtility.GetObjectBounds(cover);
            context.occupiedBounds.Add(bounds);
        }
    }

    private Vector3 GetRandomPosition(MapContext context, Vector3 center)
    {
        float angle = RandomRange(context, 0f, Mathf.PI * 2f);
        float distance = RandomRange(context, 1.2f, placementRadius);

        Vector3 offset = new Vector3(
            Mathf.Cos(angle) * distance,
            0f,
            Mathf.Sin(angle) * distance
        );

        Vector3 pos = center + offset;
        pos.y = 0f;

        return pos;
    }

    private bool CanPlace(MapContext context, Vector3 position)
    {
        if (context.hasMapBounds && !context.mapBounds.Contains(position))
            return false;

        if (context.poiAreas != null)
        {
            foreach (POIArea poi in context.poiAreas)
            {
                if (Vector3.Distance(poi.center, position) < minDistanceFromPOI)
                    return false;
            }
        }

        Bounds testBounds = new Bounds(
            position,
            new Vector3(occupiedCheckSize, 3f, occupiedCheckSize)
        );

        foreach (Bounds occupied in context.occupiedBounds)
        {
            if (occupied.Intersects(testBounds))
                return false;
        }

        return true;
    }

    private GameObject PickPrefab(MapContext context)
    {
        int index = context.random.Next(0, rewardObjectPrefabs.Length);
        return rewardObjectPrefabs[index];
    }

    private float RandomRange(MapContext context, float min, float max)
    {
        return Mathf.Lerp(
            min,
            max,
            (float)context.random.NextDouble()
        );
    }
}