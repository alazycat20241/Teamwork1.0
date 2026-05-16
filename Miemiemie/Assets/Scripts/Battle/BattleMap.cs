using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 战斗地图控制器
/// 管理地图节点的显示、选择和连线
/// 起点1个节点 → 中间每层2个节点二选一 → 终点Boss1个节点
/// </summary>
public class BattleMap : SingletonMono<BattleMap>
{
    [Header("地图配置")]
    [SerializeField] int maxLevel;          // 总层数（包含起点和Boss层）
    [SerializeField] EveryLevel[] levels;   // 每一层的容器组件

    [Header("布局参数")]
    [SerializeField] int leftPadding;       // 第一层距离Canvas左边的像素
    [SerializeField] int paddingX;          // 层与层之间的水平间距（像素）
    [SerializeField] int paddingY;          // 整体垂直偏移（像素）

    // 用于随机生成节点类型
    private System.Random random;

    // 二维数组存放所有节点：[层数, 节点索引]
    private MapNode[][] mapNodes;
    // 当前已解锁到第几层（玩家正在做选择的层）
    private int currentLevel = 0;
    // 按选择顺序记录玩家选过的所有节点，用来画连线
    private List<MapNode> selectedPath = new List<MapNode>();

    //记录当前层两条线的引用
    private GameObject lineToUpper;   // 连到上路节点的线
    private GameObject lineToLower;   // 连到下路节点的线
    protected override void Awake()
    {
        base.Awake();
        random = new System.Random();
        Init();
    }

    /// <summary>
    /// 初始化整个地图
    /// </summary>
    private void Init()
    {
        // 第一步：构建二维数组
        mapNodes = new MapNode[maxLevel][];
        int count = 0;

        for (int i = 0; i < maxLevel; i++)
        {
            // 每层的节点数量：第0层1个，最后一层1个，中间层2个
            int nodeCount = levels[i].nodes.Length;
            mapNodes[i] = new MapNode[nodeCount];

            for (int j = 0; j < nodeCount; j++)
            {
                mapNodes[i][j] = levels[i].nodes[j];
                mapNodes[i][j].level = i;
                mapNodes[i][j].value = count++;
                mapNodes[i][j].isUsed = false;
                mapNodes[i][j].gameObject.SetActive(false);
            }

            // 设置这一层节点的固定位置
            levels[i].SetRoomsPosition();
        }

        // 第二步：只显示第0层（起点，1个节点）
        RevealLevel(0);

        // 第三步：水平排列每一层
        SetLevelsPosition();
    }

    /// <summary>
    /// 解锁某一层：让这一层的所有节点显示出来并且可以点击
    /// </summary>
    private void RevealLevel(int level)
    {
        if (level >= maxLevel) return;

        for (int j = 0; j < mapNodes[level].Length; j++)
        {
            MapNode node = mapNodes[level][j];

            // 根据层级决定节点类型
            if (level == 0)
            {
                // 第0层固定为起点
                node.nodeType = NodeType.Start;
            }
            else if (level == maxLevel - 1)
            {
                // 最后一层固定为Boss
                node.nodeType = NodeType.Boss;
            }
            else
            {
                // 中间层：从战斗、事件、商店中随机选一个
                node.nodeType = GetRandomNodeType();
            }

            // 设置图标
            node.SetNodeTypeIcon();

            // 激活节点
            node.isUsed = true;
            node.gameObject.SetActive(true);

            // Boss层不可点击，其他层可点击
            if (level == maxLevel - 1)
            {
                node.SetReachable(false);
            }
            else
            {
                node.SetReachable(true);
            }
        }

        currentLevel = level;

        // 画线：从上一层的选择点到这一层的两个节点
        if (level > 0 && selectedPath.Count > 0)
        {
            MapNode fromNode = selectedPath[selectedPath.Count - 1];

            if (mapNodes[level].Length >= 1)
                lineToUpper = DrawLine(fromNode, mapNodes[level][0], Color.white);

            if (mapNodes[level].Length >= 2)
                lineToLower = DrawLine(fromNode, mapNodes[level][1], Color.white);
        }
    }
    /// <summary>
    /// 从战斗、事件、商店中随机返回一种（不包含Boss和Start）
    /// </summary>
    private NodeType GetRandomNodeType()
    {
        int roll = random.Next(3);      // 0, 1, 2
        switch (roll)
        {
            case 0: return NodeType.Battle;
            case 1: return NodeType.Event;
            case 2: return NodeType.Shop;
            default: return NodeType.Battle;
        }
    }

    /// <summary>
    /// 玩家点击了一个节点后，由OnNodeClicked调用这里
    /// </summary>
    /// <param name="nodeIndex">节点在当前层的索引（0或1）</param>
    public void SelectNode(int nodeIndex)
    {
        if (currentLevel >= maxLevel) return;

        // 拿到玩家选中的节点
        MapNode selected = mapNodes[currentLevel][nodeIndex];

        // 标记为已访问（变暗、不可再点）
        selected.MarkAsVisited();

        // 记录到路径列表
        selectedPath.Add(selected);

        // 如果当前层有多个节点（中间层），另一个节点变灰不可选
        if (mapNodes[currentLevel].Length > 1)
        {
            int otherIndex = (nodeIndex == 0) ? 1 : 0;
            mapNodes[currentLevel][otherIndex].Hide();
        }

        // 删掉没被选中的那条线
        if (nodeIndex == 0 && lineToLower != null)
        {
            Destroy(lineToLower);
            lineToLower = null;
        }
        else if (nodeIndex == 1 && lineToUpper != null)
        {
            Destroy(lineToUpper);
            lineToUpper = null;
        }

        // 如果还没到最后一层，解锁下一层
        if (currentLevel < maxLevel - 1)
        {
            RevealLevel(currentLevel + 1);
        }
    }

    /// <summary>
    /// 获取当前层的节点数组
    /// </summary>
    public MapNode[] GetCurrentLevelNodes()
    {
        if (currentLevel < maxLevel)
            return mapNodes[currentLevel];
        return null;
    }

    /// <summary>
    /// MapNode被点击时的回调
    /// </summary>
    public void OnNodeClicked(MapNode node)
    {
        if (node == null) return;
        if (node.isVisited) return;

        // 找出这个节点在当前层的索引
        MapNode[] currentNodes = mapNodes[currentLevel];
        int nodeIndex = -1;

        for (int i = 0; i < currentNodes.Length; i++)
        {
            if (currentNodes[i] == node)
            {
                nodeIndex = i;
                break;
            }
        }

        if (nodeIndex == -1) return;

        SelectNode(nodeIndex);
    }

    /// <summary>
    /// 把每一层的容器水平排开
    /// </summary>
    private void SetLevelsPosition()
    {
        for (int i = 0; i < levels.Length; i++)
        {
            int x = i * paddingX + leftPadding;
            int y = paddingY;
            levels[i].gameObject.GetComponent<RectTransform>().anchoredPosition = new Vector2(x, y);
        }
    }


    /// <summary>
    /// 在两个节点之间画一条线
    /// </summary>
    private GameObject DrawLine(MapNode a, MapNode b, Color color)
    {
        RectTransform rectA = a.GetComponent<RectTransform>();
        RectTransform rectB = b.GetComponent<RectTransform>();
        RectTransform parentRt = a.transform.parent.parent.GetComponent<RectTransform>(); // Canvas

        // 都转成相对于 Canvas 的坐标
        Vector2 posA = rectA.anchoredPosition + (Vector2)a.transform.parent.GetComponent<RectTransform>().anchoredPosition;
        Vector2 posB = rectB.anchoredPosition + (Vector2)b.transform.parent.GetComponent<RectTransform>().anchoredPosition;

        // 创建线物体，挂到 Canvas 下
        GameObject lineObj = new GameObject("Line");
        lineObj.transform.SetParent(parentRt, false);

        RectTransform lineRt = lineObj.AddComponent<RectTransform>();
        lineRt.pivot = new Vector2(0, 0.5f);

        Image img = lineObj.AddComponent<Image>();
        img.color = color;
        img.raycastTarget = false;

        // 计算方向和距离
        Vector2 dir = posB - posA;
        float distance = dir.magnitude;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

        lineRt.anchoredPosition = posA;
        lineRt.sizeDelta = new Vector2(distance, 3f);
        lineRt.localRotation = Quaternion.Euler(0, 0, angle);

        return lineObj;   // 返回线物体
    }

    /// <summary>
    /// 重置地图
    /// </summary>
    public void ResetMap()
    {
        selectedPath.Clear();

        for (int i = 0; i < maxLevel; i++)
        {
            for (int j = 0; j < mapNodes[i].Length; j++)
            {
                mapNodes[i][j].ResetNode();
            }
        }

        RevealLevel(0);
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
    }
}