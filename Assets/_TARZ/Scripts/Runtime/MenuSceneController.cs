using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class MenuSceneController : MonoBehaviour
{
    [Header("UI")]
    public TMP_Dropdown chapterDropdown;
    public TMP_Dropdown stageDropdown;
    public TMP_InputField seedInputField;
//    public Toggle randomSeedToggle;

    [Header("Buttons")]
    public Button newStartButton;
    public Button continueButton;

    [Header("Scene")]
    public string proceduralStageSceneName = "ProceduralStage";

    private void Start()
    {
        UnlockCursor();

        InitDropdowns();

        if (newStartButton != null)
            newStartButton.onClick.AddListener(StartNewStage);

        if (continueButton != null)
        {
            continueButton.onClick.AddListener(ContinueSavedStage);
            continueButton.interactable = StageProgressStorage.HasSave();
        }
    }

    private void UnlockCursor()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void InitDropdowns()
    {
        if (chapterDropdown != null)
        {
            chapterDropdown.ClearOptions();
            chapterDropdown.AddOptions(new List<string>
            {
                "Chapter 1 - Ruins",
                "Chapter 2 - Beach",
                "Chapter 3 - Harbor",
                "Chapter 4 - Resort",
                "Chapter 5 - Cave",
                "Chapter 6 - Frozen Base",
                "Chapter 7 - Volcano"
            });
        }

        if (stageDropdown != null)
        {
            stageDropdown.ClearOptions();
            stageDropdown.AddOptions(new List<string>
            {
                "Stage 1",
                "Stage 2",
                "Stage 3",
                "Stage 4",
                "Stage 5",
                "Stage 6"
            });
        }

        if (seedInputField != null)
            seedInputField.text = "123456";

//        if (randomSeedToggle != null)
//            randomSeedToggle.isOn = true;

        ApplySavedInfoToUI();
    }

    private void ApplySavedInfoToUI()
    {
        StageProgressSaveData save = StageProgressStorage.Load();

        if (save == null)
            return;

        if (chapterDropdown != null)
            chapterDropdown.value = Mathf.Clamp(save.chapter - 1, 0, chapterDropdown.options.Count - 1);

        if (stageDropdown != null)
            stageDropdown.value = Mathf.Clamp(save.stage - 1, 0, stageDropdown.options.Count - 1);

        if (seedInputField != null)
            seedInputField.text = save.seed.ToString();

//        if (randomSeedToggle != null)
//            randomSeedToggle.isOn = false;
    }

    public void StartNewStage()
    {
        int chapter = chapterDropdown != null ? chapterDropdown.value + 1 : 1;
        int stage = stageDropdown != null ? stageDropdown.value + 1 : 1;

        // New Stage는 무조건 새로운 Seed 사용
        int seed = Random.Range(1, int.MaxValue);

        StageSelectionData.selectedChapter = chapter;
        StageSelectionData.selectedStage = stage;
        StageSelectionData.selectedSeed = seed;
        StageSelectionData.useRandomSeed = true;

        // 기존 저장 진행 정보 삭제
        StageProgressStorage.Clear();

        Debug.Log(
            $"[MenuSceneController] New Stage Start. " +
            $"Chapter={chapter}, Stage={stage}, Seed={seed}"
        );

        SceneManager.LoadScene(proceduralStageSceneName);
    }

    public void ContinueSavedStage()
    {
        StageProgressSaveData save = StageProgressStorage.Load();

        if (save == null)
        {
            Debug.LogWarning("Saved stage data does not exist.");
            return;
        }

        StageSelectionData.selectedChapter = save.chapter;
        StageSelectionData.selectedStage = save.stage;
        StageSelectionData.selectedSeed = save.seed;
        StageSelectionData.useRandomSeed = false;

        Debug.Log(
            $"[MenuSceneController] Continue Saved Stage. " +
            $"Chapter={save.chapter}, Stage={save.stage}, Seed={save.seed}"
        );

        SceneManager.LoadScene(proceduralStageSceneName);
    }
}