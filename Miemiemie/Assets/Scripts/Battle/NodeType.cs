using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 战斗节点类型
/// </summary>
public enum NodeType
{
    Battle,      // 战斗
    Event,       // 事件
    Shop,        // 商店
    Boss,        // Boss（终点固定）
    Start        // 起点
}

/// <summary>
/// 单个路线节点的数据
/// </summary>
[System.Serializable]
public class RouteNode
{
    public NodeType nodeType;           // 节点类型
    public Vector2Int position;         // 节点位置（列, 行），用于地图布局
    public List<RouteNode> nextNodes;   // 下一层可选的节点（通常2-3个）
    public bool isVisited;              // 是否已访问
    public bool isAvailable;            // 当前是否可选择（到达该节点所在层时变为true）

    // 战斗节点的敌人配置（如果是战斗类型）
    public string enemyConfigID;        // 敌人配置ID（从数据库读取）

    // 事件节点的配置（如果是事件类型）
    public string eventConfigID;        // 事件配置ID

    // 商店节点的配置（如果是商店类型）
    public string shopConfigID;         // 商店配置ID
}