using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "TARZ/Stage/Stage Template")]
public class StageTemplateData : ScriptableObject
{
    [Header("Identity")]
    public int stageNumber = 1;
    public string templateId;
    public string templateName;

    [Header("Stage Rule")]
    public StageNodeType stageNodeType = StageNodeType.NormalBattle;
    public StageMapShapeType mapShapeType = StageMapShapeType.CoastalRoad;

    [Header("Main Anchors")]
    public StageTemplatePoint startPoint = new StageTemplatePoint(new Vector3(0f, 0f, -45f));
    public StageTemplatePoint exitPoint = new StageTemplatePoint(new Vector3(0f, 0f, 45f));

    [Header("Gameplay Anchors")]
    public List<StageTemplatePoint> combatPoints = new List<StageTemplatePoint>();
    public List<StageTemplatePoint> rewardPoints = new List<StageTemplatePoint>();
    public List<StageTemplatePoint> secretPoints = new List<StageTemplatePoint>();
    public List<StageTemplatePoint> enemySpawnPoints = new List<StageTemplatePoint>();

    [Header("Boss Anchors")]
    public bool hasMidBossPoint = false;
    public StageTemplatePoint midBossPoint = new StageTemplatePoint(Vector3.zero);
    public bool hasBossRoomPoint = false;
    public StageTemplatePoint bossRoomPoint = new StageTemplatePoint(Vector3.zero);

    [Header("Road Structure")]
    public List<StageTemplateRoadSegment> roadSegments = new List<StageTemplateRoadSegment>();

    [Header("Stage Bounds")]
    public bool useStageBounds = false;
    public StageTemplateRect stageBounds = new StageTemplateRect(Vector2.zero, new Vector2(120f, 120f));

    [Header("Rect Zone Layout")]
    public List<StageTemplateRectZone> roadRects = new List<StageTemplateRectZone>();
    public List<StageTemplateRectZone> buildingZones = new List<StageTemplateRectZone>();
    public List<StageTemplateRectZone> blockedZones = new List<StageTemplateRectZone>();
    public List<StageTemplateRectZone> parkingZones = new List<StageTemplateRectZone>();
    public List<StageTemplateRectZone> decorZones = new List<StageTemplateRectZone>();

    [Header("Boss Arena Zone")]
    public bool hasBossArenaZone = false;
    public StageTemplateRectZone bossArenaZone = new StageTemplateRectZone();

    public void ApplyTo(MapContext context)
    {
        if (context == null)
            return;

        ClearLayout(context);

        context.selectedStage = stageNumber;
        context.selectedStageType = stageNodeType;
        context.selectedMapShape = mapShapeType;

        context.startPosition = startPoint.Resolve(context.random);
        context.exitPosition = exitPoint.Resolve(context.random);

        AddResolvedPoints(combatPoints, context.combatPositions, context.random);
        AddResolvedPoints(rewardPoints, context.rewardPositions, context.random);
        AddResolvedPoints(secretPoints, context.secretPositions, context.random);
        AddResolvedPoints(enemySpawnPoints, context.enemySpawnPositions, context.random);

        if (hasMidBossPoint)
            context.midBossPosition = midBossPoint.Resolve(context.random);

        if (hasBossRoomPoint)
            context.bossRoomPosition = bossRoomPoint.Resolve(context.random);

        for (int i = 0; i < roadSegments.Count; i++)
        {
            StageTemplateRoadSegment segment = roadSegments[i];

            if (segment == null)
                continue;

            Vector3 from = segment.from.Resolve(context.random);
            Vector3 to = segment.to.Resolve(context.random);
            AddRoadLine(context, from, to, segment.pointCount);
        }

        AddRoadRects(context);
        AddBlockedZoneBounds(context);

        Debug.Log(
            $"[StageTemplateData] Applied {name}. " +
            $"Stage={stageNumber}, Roads={context.roadWorldPositions.Count}, " +
            $"Combats={context.combatPositions.Count}, Rewards={context.rewardPositions.Count}"
        );
    }

    private void ClearLayout(MapContext context)
    {
        context.roadGridPositions.Clear();
        context.roadWorldPositions.Clear();
        context.combatPositions.Clear();
        context.secretPositions.Clear();
        context.rewardPositions.Clear();
        context.enemySpawnPositions.Clear();
        context.combatZones.Clear();

        context.secretRoomPosition = Vector3.zero;
        context.secretRoomBounds = new Bounds();
        context.hasSecretRoomBounds = false;
        context.hasMapBounds = false;

        if (useStageBounds)
        {
            context.mapBounds = stageBounds.ToBounds();
            context.hasMapBounds = true;
        }
    }

    private void AddResolvedPoints(
        List<StageTemplatePoint> source,
        List<Vector3> target,
        System.Random random
    )
    {
        if (source == null || target == null)
            return;

        for (int i = 0; i < source.Count; i++)
        {
            StageTemplatePoint point = source[i];

            if (point == null)
                continue;

            target.Add(point.Resolve(random));
        }
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
            AddRoadPosition(context, RoundToLayoutGrid(pos));
        }
    }

    private void AddRoadPosition(MapContext context, Vector3 pos)
    {
        for (int i = 0; i < context.roadWorldPositions.Count; i++)
        {
            if (Vector3.Distance(context.roadWorldPositions[i], pos) < 0.1f)
                return;
        }

        context.roadWorldPositions.Add(pos);
    }

    private void AddRoadRects(MapContext context)
    {
        if (roadRects == null || context.settings == null)
            return;

        float tileSize = Mathf.Max(1f, context.settings.tileSize);

        for (int i = 0; i < roadRects.Count; i++)
        {
            StageTemplateRectZone zone = roadRects[i];

            if (zone == null || !zone.enabled)
                continue;

            AddRoadRectCenterLine(context, zone.rect, tileSize);
        }
    }

    private void AddRoadRectCenterLine(MapContext context, StageTemplateRect rect, float spacing)
    {
        if (rect == null)
            return;

        float width = Mathf.Abs(rect.size.x);
        float height = Mathf.Abs(rect.size.y);

        if (width >= height)
        {
            AddAxisRoadLine(
                context,
                new Vector3(rect.center.x - width * 0.5f, 0f, rect.center.y),
                new Vector3(rect.center.x + width * 0.5f, 0f, rect.center.y),
                spacing
            );
        }
        else
        {
            AddAxisRoadLine(
                context,
                new Vector3(rect.center.x, 0f, rect.center.y - height * 0.5f),
                new Vector3(rect.center.x, 0f, rect.center.y + height * 0.5f),
                spacing
            );
        }
    }

    private void AddAxisRoadLine(MapContext context, Vector3 from, Vector3 to, float spacing)
    {
        float distance = Vector3.Distance(from, to);
        int count = Mathf.Max(2, Mathf.RoundToInt(distance / Mathf.Max(1f, spacing)) + 1);

        for (int i = 0; i < count; i++)
        {
            float t = i / (float)(count - 1);
            AddRoadPosition(context, RoundToLayoutGrid(Vector3.Lerp(from, to, t)));
        }
    }

    private void AddBlockedZoneBounds(MapContext context)
    {
        AddZoneBounds(context, blockedZones);
        AddZoneBounds(context, parkingZones);

        if (hasBossArenaZone && bossArenaZone != null && bossArenaZone.enabled)
            context.occupiedBounds.Add(bossArenaZone.rect.ToBounds());
    }

    private void AddZoneBounds(MapContext context, List<StageTemplateRectZone> zones)
    {
        if (zones == null)
            return;

        for (int i = 0; i < zones.Count; i++)
        {
            StageTemplateRectZone zone = zones[i];

            if (zone == null || !zone.enabled)
                continue;

            context.occupiedBounds.Add(zone.rect.ToBounds());
        }
    }

    private Vector3 RoundToLayoutGrid(Vector3 pos)
    {
        pos.x = Mathf.Round(pos.x * 100f) / 100f;
        pos.y = 0f;
        pos.z = Mathf.Round(pos.z * 100f) / 100f;
        return pos;
    }
}

[System.Serializable]
public class StageTemplatePoint
{
    public Vector3 position;
    [Min(0f)] public float randomRadius = 0f;

    public StageTemplatePoint()
    {
    }

    public StageTemplatePoint(Vector3 position)
    {
        this.position = position;
    }

    public Vector3 Resolve(System.Random random)
    {
        if (randomRadius <= 0f || random == null)
            return SnapY(position);

        float angle = (float)(random.NextDouble() * Mathf.PI * 2f);
        float distance = (float)random.NextDouble() * randomRadius;

        Vector3 offset = new Vector3(
            Mathf.Cos(angle) * distance,
            0f,
            Mathf.Sin(angle) * distance
        );

        return SnapY(position + offset);
    }

    private Vector3 SnapY(Vector3 value)
    {
        value.y = 0f;
        return value;
    }
}

[System.Serializable]
public class StageTemplateRoadSegment
{
    public StageTemplatePoint from = new StageTemplatePoint(Vector3.zero);
    public StageTemplatePoint to = new StageTemplatePoint(Vector3.forward * 10f);
    [Min(2)] public int pointCount = 4;

    public StageTemplateRoadSegment()
    {
    }
}

[System.Serializable]
public class StageTemplateRect
{
    public Vector2 center;
    public Vector2 size = new Vector2(10f, 10f);

    public StageTemplateRect()
    {
    }

    public StageTemplateRect(Vector2 center, Vector2 size)
    {
        this.center = center;
        this.size = size;
    }

    public Bounds ToBounds()
    {
        return new Bounds(
            new Vector3(center.x, 0f, center.y),
            new Vector3(Mathf.Abs(size.x), 4f, Mathf.Abs(size.y))
        );
    }

    public IEnumerable<Vector3> EnumeratePoints(float spacing)
    {
        spacing = Mathf.Max(0.1f, spacing);

        float halfX = Mathf.Abs(size.x) * 0.5f;
        float halfZ = Mathf.Abs(size.y) * 0.5f;

        int xCount = Mathf.Max(1, Mathf.FloorToInt((halfX * 2f) / spacing) + 1);
        int zCount = Mathf.Max(1, Mathf.FloorToInt((halfZ * 2f) / spacing) + 1);

        for (int x = 0; x < xCount; x++)
        {
            float posX = center.x - halfX + x * spacing;

            for (int z = 0; z < zCount; z++)
            {
                float posZ = center.y - halfZ + z * spacing;
                yield return new Vector3(posX, 0f, posZ);
            }
        }
    }
}

[System.Serializable]
public class StageTemplateRectZone
{
    public bool enabled = true;
    public string zoneId;
    public StageTemplateRect rect = new StageTemplateRect();
    [Min(0f)] public float randomJitter = 0f;
}
