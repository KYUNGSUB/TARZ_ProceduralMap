using UnityEngine;

public class PlayerSpawnManager : MonoBehaviour
{
    public GameObject playerPrefab;
    public Transform runtimeRoot;

    [Header("Camera")]
    public Camera mainCamera;
    public Vector3 cameraOffset = new Vector3(0f, 12f, -10f);
    public Vector3 cameraRotation = new Vector3(50f, 0f, 0f);

    private GameObject currentPlayer;

    public void SpawnPlayer(Vector3 position)
    {
        if (playerPrefab == null)
        {
            Debug.LogError("Player prefab is missing.");
            return;
        }

        if (runtimeRoot == null)
        {
            Debug.LogError("RuntimeRoot is missing.");
            return;
        }

        if (currentPlayer != null)
        {
            Destroy(currentPlayer);
            currentPlayer = null;
        }

        currentPlayer = Instantiate(
            playerPrefab,
            position + Vector3.up,
            Quaternion.identity,
            runtimeRoot
        );

        currentPlayer.tag = "Player";

        SetupCamera();
    }

    private void SetupCamera()
    {
        if (mainCamera == null)
            mainCamera = Camera.main;

        if (mainCamera == null)
        {
            Debug.LogWarning("Main Camera is missing. Please assign Main Camera to PlayerSpawnManager.");
            return;
        }

        // 중요: 카메라를 Player의 자식으로 넣지 않음
        mainCamera.transform.SetParent(null);
        mainCamera.transform.position = currentPlayer.transform.position + cameraOffset;
        mainCamera.transform.rotation = Quaternion.Euler(cameraRotation);
    }
}