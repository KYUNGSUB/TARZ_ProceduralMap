using UnityEngine;

public class StageFlowApplier : MonoBehaviour
{
    public void Apply(MapContext context)
    {
        if (context == null || context.theme == null)
        {
            Debug.LogWarning("[StageFlowApplier] Context or theme is null.");
            return;
        }

        ChapterThemeData theme = context.theme;

        if (theme.stageFlow == null || theme.stageFlow.Count == 0)
        {
            Debug.LogWarning("[StageFlowApplier] StageFlow is empty.");
            return;
        }

        for (int i = 0; i < theme.stageFlow.Count; i++)
        {
            StageNodeType nodeType = theme.stageFlow[i];

            Debug.Log($"[StageFlowApplier] Stage {i + 1}: {nodeType}");

            switch (nodeType)
            {
                case StageNodeType.Start:
                    ApplyStartStage(context, i);
                    break;

                case StageNodeType.NormalBattle:
                    ApplyNormalBattleStage(context, i);
                    break;

                case StageNodeType.ObjectReward:
                    ApplyRewardStage(context, i);
                    break;

                case StageNodeType.Event:
                    ApplyEventStage(context, i);
                    break;

                case StageNodeType.MidBoss:
                    ApplyMidBossStage(context, i);
                    break;

                case StageNodeType.SecretRoomEntrance:
                case StageNodeType.SecretRoom:
                    ApplySecretStage(context, i);
                    break;

                case StageNodeType.BossRoom:
                    ApplyBossStage(context, i);
                    break;

                case StageNodeType.Exit:
                    ApplyExitStage(context, i);
                    break;
            }
        }
    }

    public void Apply(MapContext context, int selectedStage)
    {
        if (context == null || context.theme == null)
        {
            Debug.LogWarning("[StageFlowApplier] Context or theme is null.");
            return;
        }

        ChapterThemeData theme = context.theme;

        if (theme.stageFlow == null || theme.stageFlow.Count == 0)
        {
            Debug.LogWarning("[StageFlowApplier] StageFlow is empty.");
            return;
        }

        int index = Mathf.Clamp(selectedStage - 1, 0, theme.stageFlow.Count - 1);
        StageNodeType nodeType = theme.stageFlow[index];

        Debug.Log($"[StageFlowApplier] Selected Stage {selectedStage}: {nodeType}");

        switch (nodeType)
        {
            case StageNodeType.Start:
                Debug.Log("[StageFlowApplier] Start stage selected.");
                break;

            case StageNodeType.NormalBattle:
                Debug.Log("[StageFlowApplier] Normal battle stage selected.");
                break;

            case StageNodeType.ObjectReward:
                Debug.Log("[StageFlowApplier] Object reward stage selected.");
                break;

            case StageNodeType.Event:
                Debug.Log("[StageFlowApplier] Event stage selected.");
                break;

            case StageNodeType.MidBoss:
                Debug.Log("[StageFlowApplier] Mid boss stage selected.");
                break;

            case StageNodeType.SecretRoomEntrance:
            case StageNodeType.SecretRoom:
                Debug.Log("[StageFlowApplier] Secret room stage selected.");
                break;

            case StageNodeType.BossRoom:
                Debug.Log("[StageFlowApplier] Boss room stage selected.");
                break;

            case StageNodeType.Exit:
                Debug.Log("[StageFlowApplier] Exit stage selected.");
                break;
        }
    }

    private void ApplyStartStage(MapContext context, int index)
    {
        Debug.Log("[StageFlowApplier] Start stage applied.");
    }

    private void ApplyNormalBattleStage(MapContext context, int index)
    {
        Debug.Log("[StageFlowApplier] Normal battle stage applied.");
    }

    private void ApplyRewardStage(MapContext context, int index)
    {
        Debug.Log("[StageFlowApplier] Reward stage applied.");
    }

    private void ApplyEventStage(MapContext context, int index)
    {
        Debug.Log("[StageFlowApplier] Event stage applied.");
    }

    private void ApplyMidBossStage(MapContext context, int index)
    {
        Debug.Log("[StageFlowApplier] Mid boss stage applied.");
    }

    private void ApplySecretStage(MapContext context, int index)
    {
        Debug.Log("[StageFlowApplier] Secret room stage applied.");
    }

    private void ApplyBossStage(MapContext context, int index)
    {
        Debug.Log("[StageFlowApplier] Boss stage applied.");
    }

    private void ApplyExitStage(MapContext context, int index)
    {
        Debug.Log("[StageFlowApplier] Exit stage applied.");
    }
}