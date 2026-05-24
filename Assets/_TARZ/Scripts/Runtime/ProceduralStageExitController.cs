using UnityEngine;
using UnityEngine.SceneManagement;

public class ProceduralStageExitController : MonoBehaviour
{
    [Header("Scene")]
    public string menuSceneName = "MenuScene";

    [Header("Key")]
    public KeyCode exitKey = KeyCode.Escape;

    [Header("Manager")]
    public ProceduralMapManager proceduralMapManager;

    private void Update()
    {
        if (Input.GetKeyDown(exitKey))
        {
            SaveAndReturnToMenu();
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
        else
        {
            StageProgressStorage.Save(
                StageSelectionData.selectedChapter,
                StageSelectionData.selectedStage,
                StageSelectionData.selectedSeed
            );
        }

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        SceneManager.LoadScene(menuSceneName);
    }
}