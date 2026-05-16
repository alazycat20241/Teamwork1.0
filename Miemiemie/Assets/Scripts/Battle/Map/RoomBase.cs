using UnityEngine;

public abstract class RoomBase : MonoBehaviour
{
    [Header("基础设置")]
    public Transform playerSpawnPoint;

    [Header("两个出口")]
    public ExitTriggerZone leftExit;
    public ExitTriggerZone rightExit;

    protected RoomConfig roomConfig;

    public virtual void SetupRoom(RoomConfig config)
    {
        Debug.Log($"[RoomBase] 房间:{config.roomName}, 已通关:{FixedRoomManager.Instance.IsRoomCleared(config.roomId)}");
        roomConfig = config;

        // 先设置出口数据
        SetupExitData();

        if (FixedRoomManager.Instance.IsRoomCleared(config.roomId))
        {
            ActivateExits();
        }
        else
        {
            DisableExits();
        }
    }

    private void SetupExitData()
    {
        if (roomConfig.leftExit != null && !string.IsNullOrEmpty(roomConfig.leftExit.targetRoomId))
        {
            leftExit.gameObject.SetActive(true);
            leftExit.Setup(roomConfig.leftExit);
        }
        else
        {
            leftExit.gameObject.SetActive(false);
        }

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

    protected void DisableExits()
    {
        if (leftExit != null) leftExit.Deactivate();
        if (rightExit != null) rightExit.Deactivate();
    }

    protected void ActivateExits()
    {
        if (leftExit != null) leftExit.Activate();
        if (rightExit != null) rightExit.Activate();
    }

    protected virtual void OnRoomCompleted()
    {
        FixedRoomManager.Instance.MarkRoomCleared(roomConfig.roomId);
        ActivateExits();
    }
}