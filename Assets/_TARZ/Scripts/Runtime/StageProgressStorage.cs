using UnityEngine;

public static class StageProgressStorage
{
    private const string SaveKey = "TARZ_STAGE_PROGRESS";

    public static void Save(int chapter, int stage, int seed)
    {
        StageProgressSaveData data = new StageProgressSaveData
        {
            chapter = chapter,
            stage = stage,
            seed = seed
        };

        string json = JsonUtility.ToJson(data);
        PlayerPrefs.SetString(SaveKey, json);
        PlayerPrefs.Save();

        Debug.Log($"[StageProgressStorage] Saved: Chapter={chapter}, Stage={stage}, Seed={seed}");
    }

    public static bool HasSave()
    {
        return PlayerPrefs.HasKey(SaveKey);
    }

    public static StageProgressSaveData Load()
    {
        if (!HasSave())
            return null;

        string json = PlayerPrefs.GetString(SaveKey);
        return JsonUtility.FromJson<StageProgressSaveData>(json);
    }

    public static void Clear()
    {
        PlayerPrefs.DeleteKey(SaveKey);
        PlayerPrefs.Save();
    }
}