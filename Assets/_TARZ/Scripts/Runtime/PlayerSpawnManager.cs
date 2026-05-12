using UnityEngine;

public class PlayerSpawnManager : MonoBehaviour
{
    public GameObject playerPrefab;
    public Transform runtimeRoot;

    private GameObject currentPlayer;

    public void SpawnPlayer(Vector3 position)
    {
        if (playerPrefab == null)
        {
            Debug.LogError("Player prefab is missing.");
            return;
        }

        if (currentPlayer != null)
            Destroy(currentPlayer);

        currentPlayer = Instantiate(
            playerPrefab,
            position + Vector3.up,
            Quaternion.identity,
            runtimeRoot
        );

        currentPlayer.tag = "Player";

        Camera.main.transform.SetParent(currentPlayer.transform);
        Camera.main.transform.localPosition = new Vector3(0f, 12f, -10f);
        Camera.main.transform.localRotation = Quaternion.Euler(50f, 0f, 0f);
    }
}