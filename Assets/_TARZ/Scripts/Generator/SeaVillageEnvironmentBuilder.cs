using System.Collections.Generic;
using UnityEngine;

public class SeaVillageEnvironmentBuilder : MonoBehaviour
{
    [Header("Environment Height")]
    public float groundY = 0.12f;
    public float seaY = -0.05f;
    public float wallY = 0.2f;
    public float boatY = 0.05f;

    public void Build(MapContext context)
    {
        if (context == null || context.theme == null)
        {
            Debug.LogWarning("[SeaVillageEnvironmentBuilder] Context or theme is null.");
            return;
        }

        StageTemplateData template = context != null && context.theme != null
            ? context.theme.GetStageTemplate(context.selectedStage)
            : null;

        BuildSeaPlane(context, template);
        BuildPlazas(context);
        BuildParkingLots(context);
        BuildSeaWalls(context);
        BuildBoats(context);
        BuildHarborObjects(context);
        BuildHarborBuildings(context);
    }

    private void BuildSeaPlane(MapContext context, StageTemplateData template)
    {
        if (template == null || !template.useSeaPlane)
        {
            Debug.Log("[SeaVillageEnvironmentBuilder] Sea plane skipped by stage template.");
            return;
        }

        if (context.theme.seaPlanePrefab == null)
        {
            Debug.LogWarning("[SeaVillageEnvironmentBuilder] seaPlanePrefab is null.");
            return;
        }

        Bounds seaBounds = template.seaPlaneRect.ToBounds();

        GameObject sea = Instantiate(
            context.theme.seaPlanePrefab,
            seaBounds.center,
            Quaternion.identity,
            context.mapRoot
        );

        sea.transform.localScale = new Vector3(
            seaBounds.size.x,
            1f,
            seaBounds.size.z
        );

        sea.name = "SeaVillage_SeaPlane";
    }

    private void BuildPlazas(MapContext context)
    {
        if (context.theme.plazaPrefab == null)
        {
            Debug.LogWarning("[SeaVillageEnvironmentBuilder] plazaPrefab is null.");
            return;
        }

        foreach (Vector3 p in context.plazaPositions)
        {
            Vector3 pos = new Vector3(p.x, groundY, p.z);

            GameObject plaza = Instantiate(
                context.theme.plazaPrefab,
                pos,
                Quaternion.identity,
                context.mapRoot
            );

            plaza.transform.localScale = new Vector3(26f, 0.1f, 20f);
            plaza.name = $"SeaVillage_Plaza_{Mathf.RoundToInt(pos.x)}_{Mathf.RoundToInt(pos.z)}";
        }

        Debug.Log($"[SeaVillageEnvironmentBuilder] Plazas created: {context.plazaPositions.Count}");
    }

    private void BuildParkingLots(MapContext context)
    {
        if (context.theme.parkingLotPrefab == null)
        {
            Debug.LogWarning("[SeaVillageEnvironmentBuilder] parkingLotPrefab is null.");
            return;
        }

        foreach (Vector3 p in context.parkingPositions)
        {
            Vector3 pos = new Vector3(p.x, groundY, p.z);

            GameObject parking = Instantiate(
                context.theme.parkingLotPrefab,
                pos,
                Quaternion.identity,
                context.mapRoot
            );

            parking.transform.localScale = new Vector3(30f, 0.1f, 18f);
            parking.name = $"SeaVillage_Parking_{Mathf.RoundToInt(pos.x)}_{Mathf.RoundToInt(pos.z)}";
        }

        Debug.Log($"[SeaVillageEnvironmentBuilder] Parking lots created: {context.parkingPositions.Count}");
    }

    private void BuildSeaWalls(MapContext context)
    {
        if (context.theme.seaWallPrefab == null)
        {
            Debug.LogWarning("[SeaVillageEnvironmentBuilder] seaWallPrefab is null.");
            return;
        }

        foreach (Vector3 p in context.seaWallPositions)
        {
            Vector3 pos = new Vector3(p.x, wallY, p.z);

            GameObject wall = Instantiate(
                context.theme.seaWallPrefab,
                pos,
                Quaternion.Euler(0f, 90f, 0f),
                context.mapRoot
            );

            wall.name = $"SeaVillage_SeaWall_{Mathf.RoundToInt(pos.x)}_{Mathf.RoundToInt(pos.z)}";
        }

        Debug.Log($"[SeaVillageEnvironmentBuilder] Sea walls created: {context.seaWallPositions.Count}");
    }

    private void BuildBoats(MapContext context)
    {
        if (context.theme.boatPrefab == null)
        {
            Debug.LogWarning("[SeaVillageEnvironmentBuilder] boatPrefab is null.");
            return;
        }

        foreach (Vector3 p in context.boatPositions)
        {
            Vector3 pos = new Vector3(p.x, boatY, p.z);

            GameObject boat = Instantiate(
                context.theme.boatPrefab,
                pos,
                Quaternion.Euler(0f, -20f, 0f),
                context.mapRoot
            );

            boat.name = $"SeaVillage_Boat_{Mathf.RoundToInt(pos.x)}_{Mathf.RoundToInt(pos.z)}";
        }

        Debug.Log($"[SeaVillageEnvironmentBuilder] Boats created: {context.boatPositions.Count}");
    }

    private void BuildHarborObjects(MapContext context)
    {
        if (context.theme.harborObjectPrefabs == null ||
            context.theme.harborObjectPrefabs.Count == 0)
        {
            Debug.LogWarning("[SeaVillageEnvironmentBuilder] harborObjectPrefabs is empty.");
            return;
        }

        if (context.harborObjectPositions == null ||
            context.harborObjectPositions.Count == 0)
        {
            Debug.Log("[SeaVillageEnvironmentBuilder] No harbor object positions.");
            return;
        }

        int totalCreated = 0;
        int clusterIndex = 0;

        foreach (Vector3 p in context.harborObjectPositions)
        {
            Vector3 center = new Vector3(p.x, groundY + 0.2f, p.z);

            int clusterType = clusterIndex % 3;

            if (clusterType == 0)
            {
                totalCreated += CreateContainerYard(context, center);
            }
            else if (clusterType == 1)
            {
                totalCreated += CreateCrateStack(context, center);
            }
            else
            {
                totalCreated += CreateBarrelGroup(context, center);
            }

            clusterIndex++;
        }

        Debug.Log($"[SeaVillageEnvironmentBuilder] Harbor object clusters created: {context.harborObjectPositions.Count}");
        Debug.Log($"[SeaVillageEnvironmentBuilder] Harbor objects created: {totalCreated}");
    }

    private int CreateContainerYard(MapContext context, Vector3 center)
    {
        GameObject containerPrefab = PickHarborObjectPrefabByName(context, "Container");

        if (containerPrefab == null)
            containerPrefab = PickHarborObjectPrefab(context);

        int count = 0;

        float xSpacing = 5f;
        float zSpacing = 4f;

        for (int x = 0; x < 2; x++)
        {
            for (int z = 0; z < 3; z++)
            {
                Vector3 pos = center + new Vector3(
                    (x - 0.5f) * xSpacing,
                    0f,
                    (z - 1f) * zSpacing
                );

                GameObject obj = Instantiate(
                    containerPrefab,
                    pos,
                    Quaternion.Euler(0f, 90f, 0f),
                    context.mapRoot
                );

                obj.name = $"SeaVillage_ContainerYard_{count}";

                Bounds bounds = BoundsUtility.GetObjectBounds(obj);
                context.occupiedBounds.Add(bounds);

                count++;
            }
        }

        return count;
    }

    private int CreateCrateStack(MapContext context, Vector3 center)
    {
        GameObject cratePrefab = PickHarborObjectPrefabByName(context, "Crate");

        if (cratePrefab == null)
            cratePrefab = PickHarborObjectPrefab(context);

        int count = 0;

        Vector3[] offsets =
        {
        new Vector3(-2f, 0f, -2f),
        new Vector3( 2f, 0f, -2f),
        new Vector3(-2f, 0f,  2f),
        new Vector3( 2f, 0f,  2f),
        new Vector3( 0f, 2f,  0f)
    };

        foreach (Vector3 offset in offsets)
        {
            Vector3 pos = center + offset;

            GameObject obj = Instantiate(
                cratePrefab,
                pos,
                Quaternion.Euler(0f, context.random.Next(0, 360), 0f),
                context.mapRoot
            );

            obj.name = $"SeaVillage_CrateStack_{count}";

            Bounds bounds = BoundsUtility.GetObjectBounds(obj);
            context.occupiedBounds.Add(bounds);

            count++;
        }

        return count;
    }

    private int CreateBarrelGroup(MapContext context, Vector3 center)
    {
        GameObject barrelPrefab = PickHarborObjectPrefabByName(context, "Barrel");

        if (barrelPrefab == null)
            barrelPrefab = PickHarborObjectPrefab(context);

        int count = 0;

        Vector3[] offsets =
        {
        new Vector3(-1.5f, 0f, -1.5f),
        new Vector3( 1.5f, 0f, -1.5f),
        new Vector3(-1.5f, 0f,  1.5f),
        new Vector3( 1.5f, 0f,  1.5f),
        new Vector3( 0f,   0f,  0f)
    };

        foreach (Vector3 offset in offsets)
        {
            Vector3 pos = center + offset;

            GameObject obj = Instantiate(
                barrelPrefab,
                pos,
                Quaternion.identity,
                context.mapRoot
            );

            obj.name = $"SeaVillage_BarrelGroup_{count}";

            Bounds bounds = BoundsUtility.GetObjectBounds(obj);
            context.occupiedBounds.Add(bounds);

            count++;
        }

        return count;
    }

    private GameObject PickHarborObjectPrefab(MapContext context)
    {
        if (context == null ||
            context.theme == null ||
            context.theme.harborObjectPrefabs == null ||
            context.theme.harborObjectPrefabs.Count == 0)
        {
            return null;
        }

        int index = context.random.Next(
            0,
            context.theme.harborObjectPrefabs.Count
        );

        return context.theme.harborObjectPrefabs[index];
    }

    private GameObject PickHarborObjectPrefabByName(
    MapContext context,
    string keyword)
    {
        if (context == null ||
            context.theme == null ||
            context.theme.harborObjectPrefabs == null)
        {
            return null;
        }

        foreach (GameObject prefab in context.theme.harborObjectPrefabs)
        {
            if (prefab == null)
                continue;

            if (prefab.name.Contains(keyword))
                return prefab;
        }

        return null;
    }

    private void BuildHarborBuildings(MapContext context)
    {
        if (context.harborBuildingPositions == null ||
            context.harborBuildingPositions.Count == 0)
        {
            Debug.Log("[SeaVillageEnvironmentBuilder] No harbor building positions.");
            return;
        }

        int count = 0;

        foreach (Vector3 p in context.harborBuildingPositions)
        {
            GameObject prefab = PickHarborBuildingPrefab(context);

            if (prefab == null)
            {
                Debug.LogWarning("[SeaVillageEnvironmentBuilder] No harbor building prefab found.");
                continue;
            }

            Vector3 pos = new Vector3(p.x, groundY, p.z);

            GameObject building = Instantiate(
                prefab,
                pos,
                Quaternion.Euler(0f, 90f, 0f),
                context.mapRoot
            );

            building.name =
                $"SeaVillage_HarborBuilding_{Mathf.RoundToInt(pos.x)}_{Mathf.RoundToInt(pos.z)}";

            Bounds bounds = BoundsUtility.GetObjectBounds(building);
            context.occupiedBounds.Add(bounds);

            count++;
        }

        Debug.Log($"[SeaVillageEnvironmentBuilder] Harbor buildings created: {count}");
    }

    private GameObject PickHarborBuildingPrefab(MapContext context)
    {
        if (context == null ||
            context.theme == null ||
            context.theme.buildingDataList == null ||
            context.theme.buildingDataList.Count == 0)
        {
            return null;
        }

        List<BuildingData> candidates = new List<BuildingData>();

        foreach (BuildingData data in context.theme.buildingDataList)
        {
            if (data == null || data.prefab == null)
                continue;

            if (data.buildingType == SeaVillageBuildingType.Warehouse ||
                data.buildingType == SeaVillageBuildingType.HarborOffice ||
                data.buildingType == SeaVillageBuildingType.FishMarket)
            {
                candidates.Add(data);
            }
        }

        if (candidates.Count == 0)
            return null;

        int index = context.random.Next(0, candidates.Count);

        Debug.Log(
            $"[SeaVillageEnvironmentBuilder] Harbor building selected: {candidates[index].buildingType}"
        );

        return candidates[index].prefab;
    }
}
