using UnityEngine;

public class BuildingPlacer : MonoBehaviour
{
    [Header("Placement")]
    public float minGapFromRoad = 2f;
    public int maxTryPerRoad = 2;

    public void Place(MapContext context)
    {
        foreach (Vector3 roadPos in context.roadWorldPositions)
        {
            TryPlaceBuildingSide(context, roadPos, Vector3.right);
            TryPlaceBuildingSide(context, roadPos, Vector3.left);
        }
    }

    private void TryPlaceBuildingSide(MapContext context, Vector3 roadPos, Vector3 sideDirection)
    {
        for (int i = 0; i < maxTryPerRoad; i++)
        {
            double chance =
                context.settings.buildingChance *
                context.theme.buildingDensity;

            if (context.random.NextDouble() > chance)
                return;

            GameObject prefab = PrefabPicker.Pick(context.theme.buildingPrefabs, context.random);
            if (prefab == null)
                return;

            float offset = context.settings.buildingOffsetFromRoad;

            // 약간의 랜덤 간격 추가
            offset += RandomRange(context, 0f, 6f);

            Vector3 position = roadPos + sideDirection.normalized * offset;

            Quaternion rotation = Quaternion.Euler(
                0f,
                context.random.Next(0, 4) * 90f,
                0f
            );

            GameObject building = Instantiate(prefab, position, rotation, context.mapRoot);
            building.name = "Building";

            Bounds bounds = BoundsUtility.GetObjectBounds(building);

            if (BoundsUtility.IsOverlapping(context.occupiedBounds, bounds))
            {
                Destroy(building);
                continue;
            }

            context.occupiedBounds.Add(bounds);
            return;
        }
    }

    private float RandomRange(MapContext context, float min, float max)
    {
        return min + (float)context.random.NextDouble() * (max - min);
    }
}