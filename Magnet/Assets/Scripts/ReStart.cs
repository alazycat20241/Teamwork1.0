using UnityEngine;

// 挂载在 JIANCI 物体上的触发脚本
public class JianciTrigger : MonoBehaviour
{
    // 在Inspector面板中赋值的引用
    [Header("核心引用")]
    [Tooltip("玩家对象（PLAYER）")]
    public GameObject player;          
    [Tooltip("传送目标位置（STARTPLACE）")]
    public Transform startPlace;

    /*
    [Header("动画设置")]
    public Animator PlayerAnimator;
    [Tooltip("是否播放完动画再传送")]
    public bool teleportAfterAnimation =false;
    [Tooltip("动画播放延迟（秒）")]
    public float animationDelay = 0f;
    */
 
    // 2D触发碰撞进入事件
    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log("111");

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
        Debug.Log("222");
        TeleportPlayer();
       // }
    }

    // 传送玩家到指定位置
    private void TeleportPlayer()
    {
            // 重置玩家位置（保留旋转）
            player.transform.position = startPlace.transform.position;

            Debug.Log("玩家已传送到起始位置！");
    }



   
}