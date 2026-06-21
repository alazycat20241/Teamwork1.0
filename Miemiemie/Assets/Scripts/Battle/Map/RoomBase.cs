using UnityEngine;

/// <summary>
/// 所有房间的模板都继承这个
/// 负责：出口开关、玩家出生点

public abstract class RoomBase : MonoBehaviour
{
    [Header("基础设置")]
    public Transform playerSpawnPoint;   // 玩家进入房间时出现在哪

    [Header("两个出口")]
    public ExitTriggerZone leftExit;     // 左边出口
    public ExitTriggerZone rightExit;    // 右边出口

    protected RoomConfig roomConfig;     // 当前房间的数据（哪个地图、哪个类型等）

    /// <summary>
    /// 初始化房间（由 FixedRoomManager 调用）
    /// 1. 配置出口数据
    /// 2. 如果之前已经通关 → 出口直接开
    ///    还没通关 → 出口先关着
    /// </summary>
    public virtual void SetupRoom(RoomConfig config)
    {
        roomConfig = config;                    // 保存房间数据

        SetupExitData();                        // 第1步：根据数据配置左右出口

        // 第2步：检查这个房间之前有没有通关过
        if (FixedRoomManager.Instance.IsRoomCleared(config.roomId))
        {
            ActivateExits();                    // 通关过 → 门开着，可以直接走
        }
        else
        {
            DisableExits();                     // 没通关 → 门关着，必须先打
        }
    }

    /// <summary>
    /// 根据房间数据，决定左出口和右出口是否显示、通向哪里
    /// </summary>
    public void SetupExitData()
    {
        //   左出口数据存在                 而且目标房间ID填了
        if (roomConfig.leftExit != null && !string.IsNullOrEmpty(roomConfig.leftExit.targetRoomId))
        {
            leftExit.gameObject.SetActive(true);
            leftExit.Setup(roomConfig.leftExit);
        }
        else
        {
            // 没有出口数据 → 隐藏左出口
            leftExit.gameObject.SetActive(false);
        }

        // ----- 右出口-----
        if (roomConfig.rightExit != null && !string.IsNullOrEmpty(roomConfig.rightExit.targetRoomId))
        {
            rightExit.gameObject.SetActive(true);
            rightExit.Setup(roomConfig.rightExit);
        }
        else
        {
            rightExit.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// 关闭所有出口（玩家不能离开）
    /// </summary>
    protected void DisableExits()
    {
        if (leftExit != null) leftExit.Deactivate();   // 左门关
        if (rightExit != null) rightExit.Deactivate();  // 右门关
    }

    /// <summary>
    /// 打开所有出口
    /// </summary>
    protected void ActivateExits()
    {
        if (leftExit != null) leftExit.Activate();     // 左门开
        if (rightExit != null) rightExit.Activate();    // 右门开
    }

    /// <summary>
    /// 房间完成时调用（子类重写这个来触发完成逻辑）
    /// </summary>
    protected virtual void OnRoomCompleted()
    {
        // 告诉管理器："这个房间我打完了！"
        FixedRoomManager.Instance.MarkRoomCleared(roomConfig.roomId);

        // 开门！
        ActivateExits();
    }
}