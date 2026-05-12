using System.Collections.Generic;
using UnityEngine;

public static class PrefabPicker
{
    public static GameObject Pick(List<GameObject> prefabs, System.Random random)
    {
        if (prefabs == null || prefabs.Count == 0)
            return null;

        int index = random.Next(prefabs.Count);
        return prefabs[index];
    }
}