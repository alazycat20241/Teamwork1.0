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
    /*
    [Header("动画设置")]
    public Animator PlayerAnimator;
    [Tooltip("是否播放完动画再传送")]
    public bool teleportAfterAnimation =false;
    [Tooltip("动画播放延迟（秒）")]
    public float animationDelay = 0f;
    */
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
        /*
        // 1. 播放指定动画
        if (PlayerAnimator != null)
        {
            PlayerAnimator.SetTrigger("ReStart");
        }

        // 2. 处理传送逻辑
        if (teleportAfterAnimation)
        {
            // 延迟传送（等待动画播放）
            Invoke(nameof(TeleportPlayer), animationDelay);
        }
        else
        {
        */
        // 立即传送
        TeleportPlayer();
       // }
    }

    // 传送玩家到指定位置
    private void TeleportPlayer()
    {
            // 重置玩家位置（保留旋转）
            player.transform.position = startPlace.transform.position;
    }
}