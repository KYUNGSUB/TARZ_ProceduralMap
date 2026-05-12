using System.Collections.Generic;
using UnityEngine;

public static class BoundsUtility
{
    public static bool IsOverlapping(List<Bounds> boundsList, Bounds target)
    {
        foreach (Bounds b in boundsList)
        {
            if (b.Intersects(target))
                return true;
        }

        return false;
    }

    public static Bounds GetObjectBounds(GameObject obj)
    {
        Collider[] colliders = obj.GetComponentsInChildren<Collider>();

        if (colliders == null || colliders.Length == 0)
        {
            Renderer[] renderers = obj.GetComponentsInChildren<Renderer>();

            if (renderers == null || renderers.Length == 0)
                return new Bounds(obj.transform.position, Vector3.one);

            Bounds bounds = renderers[0].bounds;

            for (int i = 1; i < renderers.Length; i++)
                bounds.Encapsulate(renderers[i].bounds);

            return bounds;
        }

        Bounds result = colliders[0].bounds;

        for (int i = 1; i < colliders.Length; i++)
            result.Encapsulate(colliders[i].bounds);

        return result;
    }
}