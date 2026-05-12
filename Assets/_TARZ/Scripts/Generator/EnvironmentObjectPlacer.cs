using System.Collections.Generic;
using UnityEngine;

public class EnvironmentObjectPlacer : MonoBehaviour
{
    public void Place(MapContext context)
    {
        foreach (Vector3 roadPos in context.roadWorldPositions)
        {
            for (int i = 0; i < context.settings.objectsPerRoadTile; i++)
            {
                if (context.random.NextDouble() > context.theme.environmentDensity)
                    continue;

                Vector3 position = GetRandomPosition(context, roadPos, context.settings.objectPlacementRadius);
                TrySpawnObject(context, position);
            }
        }
    }

    private Vector3 GetRandomPosition(MapContext context, Vector3 center, float radius)
    {
        float x = RandomRange(context, -radius, radius);
        float z = RandomRange(context, -radius, radius);

        return center + new Vector3(x, 0f, z);
    }

    private void TrySpawnObject(MapContext context, Vector3 position)
    {
        List<GameObject> candidates = new List<GameObject>();
        candidates.AddRange(context.theme.treePrefabs);
        candidates.AddRange(context.theme.facilityPrefabs);
        candidates.AddRange(context.theme.debrisPrefabs);

        GameObject prefab = PrefabPicker.Pick(candidates, context.random);
        if (prefab == null) return;

        Quaternion rotation = Quaternion.Euler(0f, RandomRange(context, 0f, 360f), 0f);
        GameObject obj = Instantiate(prefab, position, rotation, context.mapRoot);
        obj.name = "EnvironmentObject";

        Bounds bounds = BoundsUtility.GetObjectBounds(obj);

        if (BoundsUtility.IsOverlapping(context.occupiedBounds, bounds))
        {
            Destroy(obj);
            return;
        }

        context.occupiedBounds.Add(bounds);
    }

    private float RandomRange(MapContext context, float min, float max)
    {
        return min + (float)context.random.NextDouble() * (max - min);
    }
}