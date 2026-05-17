using UnityEngine;

[CreateAssetMenu(menuName = "TARZ/Combat/Combat Zone Rule")]
public class CombatZoneRule : ScriptableObject
{
    [Header("Area")]
    public float combatRadius = 12f;
    public float centerClearRadius = 4f;

    [Header("Cover")]
    public int minCoverCount = 3;
    public int maxCoverCount = 6;
    public float coverMinDistanceFromCenter = 5f;
    public float coverMaxDistanceFromCenter = 10f;

    [Header("Throw Objects")]
    public int minThrowObjectCount = 6;
    public int maxThrowObjectCount = 12;
    public float throwObjectMinDistanceFromCenter = 3f;
    public float throwObjectMaxDistanceFromCenter = 11f;

    [Header("Enemy Spawn")]
    public int minEnemySpawnCount = 3;
    public int maxEnemySpawnCount = 6;
    public float enemySpawnMinDistanceFromCenter = 8f;
    public float enemySpawnMaxDistanceFromCenter = 13f;

    [Header("Sight")]
    [Range(0f, 1f)]
    public float maxCoverBlockingRatio = 0.45f;

    public int sightRayCount = 8;
}