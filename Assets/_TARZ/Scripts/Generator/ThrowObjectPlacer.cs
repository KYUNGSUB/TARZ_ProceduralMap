using UnityEngine;

public class ThrowObjectPlacer : MonoBehaviour
{
    public void Place(MapContext context)
    {
        foreach (Vector3 combatPos in context.combatPositions)
        {
            for (int i = 0; i < context.settings.throwObjectsPerCombatZone; i++)
            {
                if (context.random.NextDouble() > context.theme.throwObjectDensity)
                    continue;

                Vector3 position = GetRandomCirclePoint(context, combatPos, context.settings.throwObjectRadius);
                SpawnThrowObject(context, position);
            }
        }
    }

    private Vector3 GetRandomCirclePoint(MapContext context, Vector3 center, float radius)
    {
        float angle = (float)context.random.NextDouble() * Mathf.PI * 2f;
        float distance = (float)context.random.NextDouble() * radius;

        return center + new Vector3(
            Mathf.Cos(angle) * distance,
            0f,
            Mathf.Sin(angle) * distance
        );
    }

    private void SpawnThrowObject(MapContext context, Vector3 position)
    {
        GameObject prefab = PrefabPicker.Pick(context.theme.throwObjectPrefabs, context.random);
        if (prefab == null) return;

        GameObject obj = Instantiate(prefab, position, Quaternion.identity, context.mapRoot);
        obj.name = "ThrowObject";

        if (obj.GetComponent<ThrowObject>() == null)
            obj.AddComponent<ThrowObject>();
    }
}