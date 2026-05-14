using System.IO;
using UnityEngine;

public static class SeedStorage
{
    private const string SeedFileName = "tarz_map_seed.txt";

    private static string SeedFilePath
    {
        get
        {
            return Path.Combine(Application.persistentDataPath, SeedFileName);
        }
    }

    public static bool HasSavedSeed()
    {
        return File.Exists(SeedFilePath);
    }

    public static int LoadSeed()
    {
        if (!File.Exists(SeedFilePath))
        {
            Debug.LogWarning("Seed file does not exist.");
            return 0;
        }

        string text = File.ReadAllText(SeedFilePath);

        if (int.TryParse(text, out int loadedSeed))
        {
            return loadedSeed;
        }

        Debug.LogWarning("Seed file exists, but seed value is invalid.");
        return 0;
    }

    public static void SaveSeed(int seed)
    {
        string directory = Path.GetDirectoryName(SeedFilePath);

        if (!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        File.WriteAllText(SeedFilePath, seed.ToString());

        Debug.Log($"Seed saved: {seed}");
        Debug.Log($"Seed file path: {SeedFilePath}");
    }

    public static void DeleteSeed()
    {
        if (File.Exists(SeedFilePath))
        {
            File.Delete(SeedFilePath);
            Debug.Log("Seed file deleted.");
        }
    }
}