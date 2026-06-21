using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// 尖刺陷阱
/// 伤害时机：动画播放到第3帧时开启，第1帧时关闭（由动画Event调用）
/// 每次升起只对每个玩家造成一次伤害
/// </summary>
public class SpikeTrap : MonoBehaviour
{
    [Header("动画设置")]
    public Animator animator;           
    public float appearInterval = 3f;   // 两次升起之间的间隔（隐藏时间）
    public float upDuration = 2f;       // 升起停留时间

    [Header("伤害设置")]
    public int damage = 1;              // 每次造成的伤害

    private bool canDamage = false;     // 当前是否可以造成伤害（动画第3帧才设为true）
    private HashSet<GameObject> damagedPlayers = new HashSet<GameObject>();  // 本轮升起已经伤害过的玩家（防止重复扣血）

    void Start()
    {
        if (animator == null)
            animator = GetComponent<Animator>();

        // 初始状态：落下
        animator.SetBool("IsUp", false);

        // 开始循环协程
        StartCoroutine(SpikeCycle());
    }

    IEnumerator SpikeCycle()
    {
        while (true)
        {
            // 第1阶段：等待下一次升起
            yield return new WaitForSeconds(appearInterval);

            // 第2阶段：升起
            animator.SetBool("IsUp", true);
            canDamage = false;              // 刚升起还不造成伤害
            damagedPlayers.Clear();         // 清空伤害记录（新一轮可以伤害了）

            // 第3阶段：保持升起
            yield return new WaitForSeconds(upDuration);

            // 第4阶段：落下
            animator.SetBool("IsUp", false);
            canDamage = false;              // 落下了不能造成伤害
        }
    }

    /// <summary>
    /// 由动画Event调用（第3帧）：开启伤害判定
    /// 同时立刻检测已经在范围内的玩家
    /// </summary>
    public void EnableDamage()
    {
        canDamage = true;

        // 防止玩家在尖刺升起过程中就站在范围内
        // 此时 OnTriggerEnter 已经错过了，需要手动检测一次
        TryDamagePlayersInRange();
    }

    /// <summary>
    /// 由动画Event调用（落下动画第1帧）：关闭伤害判定
    /// </summary>
    public void DisableDamage()
    {
        canDamage = false;
    }

    /// <summary>
    /// 玩家进入触发器
    /// </summary>
    void OnTriggerEnter2D(Collider2D other)
    {
        if (!canDamage) return;                    
        TryDamagePlayer(other.gameObject);
    }

    /// <summary>
    /// 玩家持续在触发器内（每帧触发）
    /// </summary>
    void OnTriggerStay2D(Collider2D other)
    {
        if (!canDamage) return;
        TryDamagePlayer(other.gameObject);
    }

    /// <summary>
    /// 尝试对玩家造成伤害（带重复检查）
    /// </summary>
    /// <param name="player">玩家GameObject</param>
    void TryDamagePlayer(GameObject player)
    {
        // 不是玩家标签 → 忽略
        if (!player.CompareTag("Player")) return;

        // 本轮已经伤害过这个玩家了 → 忽略
        if (damagedPlayers.Contains(player)) return;

        // 获取血量组件
        var health = player.GetComponent<Health>();
        if (health != null)
        {
            health.TakeDamage(damage);          // 扣血
            damagedPlayers.Add(player);         // 标记：这轮已经扣过了
        }
    }

    /// <summary>
    /// 检测当前触发器范围内的所有玩家并造成伤害
    /// 用于 EnableDamage 时，玩家已经在范围内的情况
    /// </summary>
    void TryDamagePlayersInRange()
    {
        // 在自身位置画一个矩形检测区域（大小 = 自己的 BoxCollider2D 尺寸）
        Collider2D[] colliders = Physics2D.OverlapBoxAll(
            transform.position,
            GetComponent<BoxCollider2D>().size,
            0
        );

        // 对所有碰到的物体尝试造成伤害
        foreach (Collider2D col in colliders)
        {
            TryDamagePlayer(col.gameObject);
        }
    }
}