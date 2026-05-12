using UnityEngine;

public class BuildingPlacer : MonoBehaviour
{
    public void Place(MapContext context)
    {
        foreach (Vector3 roadPos in context.roadWorldPositions)
        {
            TryPlaceBuilding(context, roadPos + Vector3.right * context.settings.buildingOffsetFromRoad);
            TryPlaceBuilding(context, roadPos + Vector3.left * context.settings.buildingOffsetFromRoad);
        }
    }

    private void TryPlaceBuilding(MapContext context, Vector3 position)
    {
        double chance =
            context.settings.buildingChance *
            context.theme.buildingDensity;

        if (context.random.NextDouble() > chance)
            return;

        GameObject prefab = PrefabPicker.Pick(context.theme.buildingPrefabs, context.random);
        if (prefab == null) return;

        Quaternion rotation = Quaternion.Euler(0f, context.random.Next(0, 4) * 90f, 0f);
        GameObject building = Instantiate(prefab, position, rotation, context.mapRoot);
        building.name = "Building";

        Bounds bounds = BoundsUtility.GetObjectBounds(building);

        if (BoundsUtility.IsOverlapping(context.occupiedBounds, bounds))
        {
            Destroy(building);
            return;
        }

        context.occupiedBounds.Add(bounds);
    }
}