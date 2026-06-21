using System;
using System.Collections.Generic;
using UnityEngine;

// 房间配置数据
[Serializable]
public class RoomConfig
{
    [Header("基本信息")]
    public string roomId;            // 房间唯一ID
    public string roomName;          // 显示名称
    public RoomType roomType;        // 房间类型

    [Header("自定义房间预制体(可选)")]
    public GameObject customRoomPrefab;  // 如果为空，使用默认预制体

    [Header("地图坐标")]
    public Vector2Int mapPosition;   // 小地图上的位置

    [Header("运行状态")]
    public bool isVisited = false;   // 是否访问过

    [Header("固定出口")]
    public RoomExit leftExit;        // 左路出口
    public RoomExit rightExit;       // 右路出口

    [Header("战斗配置")]
    public BattleRoomSetting battleSetting;
    [Header("掉落配置")]
    public DropItem[] dropItems;  // 该房间胜利后的掉落物

    [Header("事件配置")]
    public EventData customEventData;  // 拖入具体事件
}

// 出口配置
[Serializable]
public class RoomExit
{
    public string targetRoomId;      // 目标房间ID
    public string exitName;          // 出口名称
    public Vector2 exitPosition;     // 出口在房间内的坐标
}

// 战斗房间设置
[Serializable]
public class BattleRoomSetting
{
    public List<EnemySpawnInfo> enemies = new List<EnemySpawnInfo>();
    public int rewardGold = 10;
}

// 敌人生成信息
[Serializable]
public class EnemySpawnInfo
{
    public GameObject enemyPrefab;   // 敌人预制体
    public Vector2 spawnPosition;    // 生成位置
}

// 掉落物
[Serializable]
public class DropItem
{
    public GameObject prefab;              // 掉落物预制体
    [Range(0f, 1f)] public float dropChance = 0.5f;  // 掉落概率
    public int Amount = 1;
}