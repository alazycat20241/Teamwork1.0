using Cinemachine;
using UnityEngine;

// 挂载在 JIANCI 物体上的触发脚本
public class ReStart : MonoBehaviour
{
    // 在Inspector面板中赋值的引用
    [Header("核心引用")]
    [Tooltip("玩家对象（PLAYER）")]
    public GameObject player;
    [Tooltip("传送目标位置（STARTPLACE）")]
    public Transform startPlace;
    [Header("抖动设置")]
    public float force;
    private CinemachineImpulseSource ImpulseSource;

    private void Start()
    {
        ImpulseSource = GetComponent<CinemachineImpulseSource>();
        // 配置抖动参数
        ImpulseSource.m_ImpulseDefinition.m_AmplitudeGain = force;//强度
    }

    // 2D触发碰撞进入事件
    private void OnTriggerEnter2D(Collider2D other)
    {
        ImpulseSource.GenerateImpulse();
        // 执行核心逻辑
        ExecuteTriggerLogic();
    }

    // 执行触发逻辑
    private void ExecuteTriggerLogic()
    {
        // 暂停游戏
        Time.timeScale = 0f;
        // 延迟1秒执行恢复游戏和传送逻辑
        Invoke(nameof(ResumeAndTeleport), 1f);
    }

    // 恢复游戏并传送玩家
    private void ResumeAndTeleport()
    {
        // 恢复游戏运行
        Time.timeScale = 1f;
        // 传送玩家到指定位置
        TeleportPlayer();
    }

    // 传送玩家到指定位置
    private void TeleportPlayer()
    {
        // 重置玩家位置（保留旋转）
        player.transform.position = startPlace.transform.position;
    }
}