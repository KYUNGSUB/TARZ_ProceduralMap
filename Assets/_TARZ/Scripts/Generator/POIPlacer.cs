using UnityEngine;

public class POIPlacer : MonoBehaviour
{
    public void Place(MapContext context)
    {
        Spawn(context, context.theme.startPrefabs, context.startPosition, "POI_Start");

        foreach (Vector3 pos in context.combatPositions)
            Spawn(context, context.theme.combatPrefabs, pos, "POI_Combat");

        foreach (Vector3 pos in context.secretPositions)
            Spawn(context, context.theme.secretPrefabs, pos + Vector3.forward * 5f, "POI_Secret");

        foreach (Vector3 pos in context.rewardPositions)
            Spawn(context, context.theme.rewardPrefabs, pos + Vector3.forward * 5f, "POI_Reward");

        Spawn(context, context.theme.bossPrefabs, context.bossPosition, "POI_Boss");
    }

    private void Spawn(MapContext context, System.Collections.Generic.List<GameObject> list, Vector3 position, string name)
    {
        GameObject prefab = PrefabPicker.Pick(list, context.random);
        if (prefab == null) return;

        GameObject obj = Instantiate(prefab, position, Quaternion.identity, context.mapRoot);
        obj.name = name;

        Bounds bounds = BoundsUtility.GetObjectBounds(obj);
        context.occupiedBounds.Add(bounds);
    }
}