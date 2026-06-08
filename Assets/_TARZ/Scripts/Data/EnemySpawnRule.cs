using UnityEngine;

[CreateAssetMenu(
    fileName = "EnemySpawnRule",
    menuName = "TARZ/Combat/Enemy Spawn Rule"
)]
public class EnemySpawnRule : ScriptableObject
{
    public string enemyName;
    public GameObject enemyPrefab;
    public EnemyRoleType roleType;

    [Range(0f, 1f)]
    public float spawnWeight = 1f;

    public int minStage = 1;
    public int maxStage = 6;

    public int minCount = 1;
    public int maxCount = 5;
}