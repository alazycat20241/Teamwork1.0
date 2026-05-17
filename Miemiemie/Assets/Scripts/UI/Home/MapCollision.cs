using UnityEngine;

/// <summary>
/// 地图碰撞触发器
/// 玩家进入时打开地图并定身，离开时关闭地图
/// </summary>
public class MapCollision : MonoBehaviour
{
    [Header("触发设置")]
    [SerializeField] private string playerTag = "Player";     // 玩家Tag

    private bool isDestroyed = false;                         // 是否已销毁，防止场景切换报错

    void OnDestroy()
    {
        isDestroyed = true;
    }

    void OnApplicationQuit()
    {
        isDestroyed = true;
    }

    /// <summary>
    /// 玩家进入触发器 → 打开地图并定身
    /// </summary>
    void OnTriggerEnter2D(Collider2D other)
    {
        // 已销毁则不处理
        if (isDestroyed) return;

        // 不是玩家则不处理
        if (!other.CompareTag(playerTag)) return;

        // 管理器不存在则不处理
        if (MapUIManager.Instance == null) return;

        // 获取玩家移动组件
        PlayerMove playerMove = other.GetComponent<PlayerMove>();

        // 打开地图，传入玩家引用用于定身
        MapUIManager.Instance.OpenMap(playerMove);
    }

    /// <summary>
    /// 玩家离开触发器 → 关闭地图
    /// 注意：玩家被定住后通常不会离开
    /// 但如果定身失败或其他情况，仍然做关闭处理
    /// </summary>
    void OnTriggerExit2D(Collider2D other)
    {
        if (isDestroyed) return;
        if (!other.CompareTag(playerTag)) return;
        if (MapUIManager.Instance == null) return;

        MapUIManager.Instance.CloseMap();
    }
}