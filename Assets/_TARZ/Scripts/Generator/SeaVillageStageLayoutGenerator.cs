using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class SeaVillageStageLayoutGenerator : MonoBehaviour
{
    [Header("Sea Village Stage Settings")]
    public float roadSpacing = 8f;
    public float roadWidth = 6f;

    public void Generate(MapContext context, int stageNumber)
    {
        if (context == null)
        {
            Debug.LogError("[SeaVillageStageLayoutGenerator] MapContext is null");
            return;
        }

        switch (stageNumber)
        {
            case 1:
                GenerateStage1(context);
                break;

            case 2:
                GenerateStage2(context);
                break;

            case 3:
                GenerateStage3(context);
                break;

            case 4:
                GenerateStage4(context);
                break;

            case 5:
                GenerateStage5(context);
                break;

            case 6:
                GenerateStage6(context);
                break;

            default:
                GenerateStage1(context);
                break;
        }
    }

    private void GenerateStage1(MapContext context)
    {
        // 기존 데이터 초기화
        ClearStageContext(context);

        /*
         * 설계서 Stage 1 구조
         *
         *  Exit : 좌상단
         *  Start: 우하단
         *
         *  ┌────── Exit
         *  │
         *  │      ┌───── 보조도로
         *  │      │
         *  ├──────┤
         *  │      │
         *  │      │
         *  └──────┘
         *          Start
         */

        Vector3 start = new Vector3(30f, 0f, -45f);
        Vector3 exit = new Vector3(-35f, 0f, 45f);
        Vector3 combat = new Vector3(0f, 0f, 5f);
        Vector3 reward = new Vector3(18f, 0f, 15f);

        context.startPosition = start;
        context.exitPosition = exit;

        context.combatPositions.Add(combat);
        context.rewardPositions.Add(reward);

        // 도시 구성 요소 위치
        context.plazaPositions.Add(new Vector3(0f, 0f, 0f));
        context.parkingPositions.Add(new Vector3(-35f, 0f, -25f));

        for (float z = -50f; z <= 50f; z += 20f)
        {
            context.seaWallPositions.Add(new Vector3(45f, 0f, z));
        }

        context.boatPositions.Add(new Vector3(65f, 0f, -30f));
        context.boatPositions.Add(new Vector3(68f, 0f, -10f));
        context.boatPositions.Add(new Vector3(66f, 0f, 10f));
        context.boatPositions.Add(new Vector3(70f, 0f, 30f));

        // 1. 우하단 Start에서 중앙으로 진입하는 도로
        AddRoadLine(context, start, new Vector3(30f, 0f, -25f), 5);
        AddRoadLine(context, new Vector3(30f, 0f, -25f), new Vector3(10f, 0f, -25f), 5);

        // 2. 중앙 세로 메인 도로
        AddRoadLine(context, new Vector3(10f, 0f, -25f), new Vector3(10f, 0f, 30f), 12);

        // 3. 좌상단 Exit로 이동하는 도로
        AddRoadLine(context, new Vector3(10f, 0f, 30f), new Vector3(-20f, 0f, 30f), 7);
        AddRoadLine(context, new Vector3(-20f, 0f, 30f), exit, 5);

        // 4. 설계서처럼 블록감을 주는 가로 도로
        AddRoadLine(context, new Vector3(-35f, 0f, 15f), new Vector3(35f, 0f, 15f), 15);
        AddRoadLine(context, new Vector3(-30f, 0f, -5f), new Vector3(30f, 0f, -5f), 13);
        AddRoadLine(context, new Vector3(-25f, 0f, -35f), new Vector3(30f, 0f, -35f), 12);

        // 5. 보조 세로 도로: 마을 블록 느낌 강화
        AddRoadLine(context, new Vector3(-20f, 0f, -35f), new Vector3(-20f, 0f, 30f), 14);
        AddRoadLine(context, new Vector3(30f, 0f, -45f), new Vector3(30f, 0f, 15f), 13);

        Debug.Log("[SeaVillageStageLayoutGenerator] Chapter 2 Stage 1 block layout generated.");
        Debug.Log($"Road Positions Count = {context.roadWorldPositions.Count}");
        Debug.Log($"Combat Positions Count = {context.combatPositions.Count}");
    }

    private void GenerateStage2(MapContext context)
    {
        ClearStageContext(context);

        Vector3 start = new Vector3(8f, 0f, -45f);
        Vector3 combat1 = new Vector3(-18f, 0f, -18f);
        Vector3 combat2 = new Vector3(10f, 0f, 20f);

        Vector3 harborDistrictCenter = new Vector3(42f, 0f, 25f);

        context.harborDistrictCenter = harborDistrictCenter;
        context.hasHarborDistrictCenter = true;

        Vector3 reward = new Vector3(-22f, 0f, 18f);
        Vector3 exit = new Vector3(-28f, 0f, 45f);

        context.startPosition = start;
        context.exitPosition = exit;

        context.combatPositions.Add(combat1);
        context.combatPositions.Add(combat2);
        context.rewardPositions.Add(reward);

        // Stage 2: 주거지 + 항구 진입 구역
        context.plazaPositions.Add(new Vector3(-10f, 0f, 5f));
        context.plazaPositions.Add(new Vector3(8f, 0f, 20f));
        context.parkingPositions.Add(new Vector3(22f, 0f, -25f));

        for (float z = -50f; z <= 50f; z += 20f)
        {
            context.seaWallPositions.Add(new Vector3(55f, 0f, z));
        }

        context.boatPositions.Add(new Vector3(76f, 0f, -35f));
        context.boatPositions.Add(new Vector3(78f, 0f, -15f));
        context.boatPositions.Add(new Vector3(80f, 0f, 10f));
        context.boatPositions.Add(new Vector3(82f, 0f, 35f));

        // harborDistrictCenter 기준 항구 오브젝트 배치
        Vector3 harbor = context.harborDistrictCenter;

        context.harborObjectPositions.Add(harbor + new Vector3(-8f, 0f, -10f));
        context.harborObjectPositions.Add(harbor + new Vector3(8f, 0f, -8f));
        context.harborObjectPositions.Add(harbor + new Vector3(-8f, 0f, 8f));
        context.harborObjectPositions.Add(harbor + new Vector3(8f, 0f, 10f));
        context.harborObjectPositions.Add(harbor + new Vector3(0f, 0f, 16f));

        // harborDistrictCenter 기준 항구 건물 배치
        context.harborBuildingPositions.Add(harbor + new Vector3(-12f, 0f, -10f));
        context.harborBuildingPositions.Add(harbor + new Vector3(-12f, 0f, 8f));
        context.harborBuildingPositions.Add(harbor + new Vector3(2f, 0f, 18f));

        AddRoadLine(context, start, new Vector3(8f, 0f, -35f), 4);
        AddRoadLine(context, new Vector3(8f, 0f, -35f), combat1, 6);
        AddRoadLine(context, combat1, new Vector3(0f, 0f, 0f), 5);
        AddRoadLine(context, new Vector3(0f, 0f, 0f), combat2, 6);
        AddRoadLine(context, combat2, new Vector3(-10f, 0f, 35f), 6);
        AddRoadLine(context, new Vector3(-10f, 0f, 35f), exit, 5);

        // 분기 도로
        AddRoadLine(context, combat1, new Vector3(-30f, 0f, -25f), 5);
        AddRoadLine(context, new Vector3(0f, 0f, 0f), new Vector3(25f, 0f, -5f), 5);
        AddRoadLine(context, combat2, new Vector3(32f, 0f, 20f), 5);
        AddRoadLine(context, new Vector3(32f, 0f, 20f), harbor, 3);

        Debug.Log("[SeaVillageStageLayoutGenerator] Chapter 2 Stage 2 layout generated.");
        Debug.Log($"Combat Positions Count = {context.combatPositions.Count}");
        Debug.Log($"Road Positions Count = {context.roadWorldPositions.Count}");
        Debug.Log($"Harbor District Center = {context.harborDistrictCenter}");
    }

    private void GenerateStage3(MapContext context)
    {
        GenerateStage1(context);
    }

    private void GenerateStage4(MapContext context)
    {
        GenerateStage1(context);
    }

    private void GenerateStage5(MapContext context)
    {
        GenerateStage1(context);
    }

    private void GenerateStage6(MapContext context)
    {
        GenerateStage1(context);
    }

    private void ClearStageContext(MapContext context)
    {
        context.roadWorldPositions.Clear();
        context.combatPositions.Clear();
        context.rewardPositions.Clear();
        context.secretPositions.Clear();
        context.enemySpawnPositions.Clear();
        context.combatZones.Clear();

        context.plazaPositions.Clear();
        context.parkingPositions.Clear();
        context.seaWallPositions.Clear();
        context.boatPositions.Clear();
        context.harborObjectPositions.Clear();
        context.hasHarborDistrictCenter = false;
        context.harborBuildingPositions.Clear();
    }

    private void AddRoadLine(MapContext context, Vector3 from, Vector3 to, int count)
    {
        if (count <= 1)
        {
            AddRoadPosition(context, from);
            return;
        }

        for (int i = 0; i < count; i++)
        {
            float t = i / (float)(count - 1);
            Vector3 pos = Vector3.Lerp(from, to, t);

            pos.x = Mathf.Round(pos.x * 100f) / 100f;
            pos.y = 0f;
            pos.z = Mathf.Round(pos.z * 100f) / 100f;

            AddRoadPosition(context, pos);
        }
    }

    private void AddRoadPosition(MapContext context, Vector3 pos)
    {
        foreach (Vector3 existing in context.roadWorldPositions)
        {
            if (Vector3.Distance(existing, pos) < 0.1f)
                return;
        }

        context.roadWorldPositions.Add(pos);
    }
}