using UnityEngine;
using UnityEngine.SceneManagement;

public class ProceduralStageExitController : MonoBehaviour
{
    [Header("Scene")]
    public string menuSceneName = "MenuScene";

    [Header("Keys")]
    public KeyCode unlockCursorKey = KeyCode.Escape;
    public KeyCode returnMenuKey = KeyCode.F10;

    [Header("Manager")]
    public ProceduralMapManager proceduralMapManager;

    private bool cursorUnlocked = false;

    private void Update()
    {
        // ESC ¡æ Cursor Unlock¸¸ ¼öÇà
        if (Input.GetKeyDown(unlockCursorKey))
        {
            ToggleCursor();
        }

        // F10 ¡æ MenuScene º¹±Í
        if (Input.GetKeyDown(returnMenuKey))
        {
            SaveAndReturnToMenu();
        }
    }

    private void ToggleCursor()
    {
        cursorUnlocked = !cursorUnlocked;

        if (cursorUnlocked)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            Debug.Log("[ProceduralStageExitController] Cursor Unlocked.");
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            Debug.Log("[ProceduralStageExitController] Cursor Locked.");
        }
    }

    public void SaveAndReturnToMenu()
    {
        if (proceduralMapManager != null)
        {
            StageProgressStorage.Save(
                StageSelectionData.selectedChapter,
                proceduralMapManager.selectedStage,
                proceduralMapManager.seed
            );
        }

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        SceneManager.LoadScene(menuSceneName);
    }
}