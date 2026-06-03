using UnityEngine;

public class SeaVillageCombatZoneBuilder : MonoBehaviour
{
    [Header("Combat Zone Settings")]
    public float combatRadius = 10f;
    public float enemySpawnRadius = 6f;
    public int enemySpawnCountPerCombat = 4;

    public void Build(MapContext context)
    {
        if (context == null)
            return;

        if (context.combatPositions == null || context.combatPositions.Count == 0)
        {
            Debug.LogWarning("[SeaVillageCombatZoneBuilder] No combat positions.");
            return;
        }

        int zoneCount = 0;
        int spawnCount = 0;

        foreach (Vector3 combatCenter in context.combatPositions)
        {
            CombatZoneArea zone = new CombatZoneArea(combatCenter, combatRadius, false,false);

            context.combatZones.Add(zone);
            zoneCount++;

            for (int i = 0; i < enemySpawnCountPerCombat; i++)
            {
                float angle = 360f / enemySpawnCountPerCombat * i;

                Vector3 offset = new Vector3(
                    Mathf.Cos(angle * Mathf.Deg2Rad),
                    0f,
                    Mathf.Sin(angle * Mathf.Deg2Rad)
                ) * enemySpawnRadius;

                Vector3 spawnPos = combatCenter + offset;
                context.enemySpawnPositions.Add(spawnPos);
                spawnCount++;
            }
        }

        Debug.Log($"[SeaVillageCombatZoneBuilder] Combat zones added: {zoneCount}");
        Debug.Log($"[SeaVillageCombatZoneBuilder] Enemy spawn positions added: {spawnCount}");
    }
}