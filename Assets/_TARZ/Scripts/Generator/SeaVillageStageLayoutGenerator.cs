using System.Collections.Generic;
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
        context.roadWorldPositions.Clear();
        context.combatPositions.Clear();

        // Chapter 2 Stage 1:
        // 하단 시작 → 중앙 전투 → 상단 종료
        Vector3 start = new Vector3(0, 0, -45);
        Vector3 combat = new Vector3(0, 0, 0);
        Vector3 reward = new Vector3(25, 0, 10);
        Vector3 exit = new Vector3(-5, 0, 45);

        context.startPosition = start;
        context.exitPosition = exit;
        context.combatPositions.Add(combat);
        context.rewardPositions.Add(reward);

        // 메인 도로 생성용 좌표
        AddRoadLine(context, start, new Vector3(0, 0, -20), 6);
        AddRoadLine(context, new Vector3(0, 0, -20), new Vector3(12, 0, 0), 4);
        AddRoadLine(context, new Vector3(12, 0, 0), new Vector3(0, 0, 25), 5);
        AddRoadLine(context, new Vector3(0, 0, 25), exit, 5);

        // 마을 느낌을 위한 짧은 골목길
        AddRoadLine(context, new Vector3(0, 0, -20), new Vector3(-22, 0, -20), 4);
        AddRoadLine(context, new Vector3(12, 0, 0), new Vector3(32, 0, 0), 4);
        AddRoadLine(context, new Vector3(0, 0, 25), new Vector3(-25, 0, 25), 4);

        Debug.Log("[SeaVillageStageLayoutGenerator] Chapter 2 Stage 1 layout generated.");
        Debug.Log($"Road Positions Count = {context.roadWorldPositions.Count}");
    }

    private void GenerateStage2(MapContext context)
    {
        context.roadWorldPositions.Clear();
        context.combatPositions.Clear();
        context.rewardPositions.Clear();
        context.enemySpawnPositions.Clear();
        context.combatZones.Clear();

        Vector3 start = new Vector3(8, 0, -45);
        Vector3 combat1 = new Vector3(-8, 0, -5);
        Vector3 combat2 = new Vector3(10, 0, 25);
        Vector3 reward = new Vector3(-22, 0, 18);
        Vector3 exit = new Vector3(-28, 0, 45);

        context.startPosition = start;
        context.exitPosition = exit;

        context.combatPositions.Add(combat1);
        context.combatPositions.Add(combat2);

        context.rewardPositions.Add(reward);

        AddRoadLine(context, start, new Vector3(8, 0, -25), 5);
        AddRoadLine(context, new Vector3(8, 0, -25), combat1, 5);
        AddRoadLine(context, combat1, new Vector3(-18, 0, 20), 6);
        AddRoadLine(context, new Vector3(-18, 0, 20), exit, 6);

        AddRoadLine(context, combat1, new Vector3(25, 0, -5), 5);
        AddRoadLine(context, new Vector3(-18, 0, 20), combat2, 5);
        AddRoadLine(context, new Vector3(8, 0, -25), new Vector3(-25, 0, -25), 5);

        Debug.Log("[SeaVillageStageLayoutGenerator] Chapter 2 Stage 2 layout generated.");
        Debug.Log($"Combat Positions Count = {context.combatPositions.Count}");
        Debug.Log($"Road Positions Count = {context.roadWorldPositions.Count}");
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