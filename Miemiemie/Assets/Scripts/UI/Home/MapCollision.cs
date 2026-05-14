using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapCollision : MonoBehaviour
{
    [Header("触发设置")]
    [SerializeField] private string playerTag = "Player";     // 玩家的Tag，用于识别玩家

    /// <summary>
    /// 有物体进入触发器
    /// </summary>
    /// <param name="other">进入的碰撞体</param>
    void OnTriggerEnter2D(Collider2D other)
    {
        // 判断是否是玩家
        if (other.CompareTag(playerTag))
        {
            // 打开地图面板
            MapUIManager.Instance.OpenMap();
        }
    }

    /// <summary>
    /// 有物体离开触发器
    /// </summary>
    /// <param name="other">离开的碰撞体</param>
    void OnTriggerExit2D(Collider2D other)
    {
        // 判断是否是玩家
        if (other.CompareTag(playerTag))
        {
            // 关闭地图面板
            MapUIManager.Instance.CloseMap();
        }
    }
}
