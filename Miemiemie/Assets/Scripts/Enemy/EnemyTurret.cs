using UnityEngine;
using Spine;
using Spine.Unity;
using System.Collections;

/// <summary>
/// 炮台型敌人
/// 固定不动，玩家进入攻击范围后定期投掷炸弹
/// Spine 动画：breath（呼吸循环）、attack（攻击一次）
/// 冷却独立计算，新攻击会打断旧动画重新播放
/// </summary>
public class EnemyTurret : MonoBehaviour
{
    [Header("攻击参数")]
    [SerializeField] private float attackRange = 8f;        // 攻击范围
    [SerializeField] private float attackCooldown = 2f;     // 攻击冷却（秒），从投掷瞬间开始计时
    [SerializeField] private float attackAnimDuration = 0.5f; // 攻击动画时长（秒）
    [SerializeField] private float spawnDelay = 0.3f;          // ★ 动画开始后多久生成孢子云

    [SerializeField] private GameObject sporeCloudPrefab;   // 爆炸预制体

    [Header("死亡爆炸")]
    [SerializeField] private GameObject deathExplosionPrefab; // 死亡爆炸预制体

    [Header("Spine 动画")]
    [SerializeField] private SkeletonAnimation skeletonAnimation;
    [SpineAnimation]
    [SerializeField] private string breathAnimation = "breath";  // 呼吸动画名
    [SpineAnimation]
    [SerializeField] private string attackAnimation = "attack";  // 攻击动画名

    private Transform player;          // 玩家引用
    private float attackTimer;         // 攻击冷却计时器
    private float attackAnimTimer;     // 攻击动画计时器
    private bool isAttacking;          // 是否正在播放攻击动画

    [Header("音效")]
    [SerializeField] private AudioClip attackSound;
    private AudioSource audioSource;  // 自己的 AudioSource


    void Start()
    {
        // 获取玩家引用
        GameObject playerObj = FixedRoomManager.Instance.GetPlayer();
        if (playerObj != null)
            player = playerObj.transform;

        // 初始化冷却（第一次攻击不用等）
        attackTimer = 0;

        // 订阅死亡事件
        GetComponent<Health>().OnDeath += OnDeathExplosion;

        // 初始播放呼吸动画
        PlayAnimation(breathAnimation, true);

        // 创建自己的 AudioSource
        audioSource = gameObject.GetComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.loop = false;
    }

    // ============================================
    // 每帧更新
    // ============================================
    void Update()
    {
        // 玩家不可用时 → 回到呼吸
        if (player == null || !player.CompareTag("Player"))
        {
            if (isAttacking)
                StopAttack();
            return;
        }

        // 玩家不在攻击范围 → 不攻击
        float dist = Vector2.Distance(transform.position, player.position);
        if (dist > attackRange)
            return;

        // ★ 冷却计时（与动画独立，攻击瞬间就开始计时）
        attackTimer -= Time.deltaTime;

        // 攻击动画计时
        if (isAttacking)
        {
            attackAnimTimer -= Time.deltaTime;
            if (attackAnimTimer <= 0f)
                StopAttack();       // 动画播完 → 回呼吸
        }

        // ★ 冷却好了 → 立刻攻击，哪怕上次动画还没播完也打断重播
        if (attackTimer <= 0f)
        {
            attackTimer = attackCooldown;   // 重置冷却（新冷却从此刻开始）
            StartAttack();
        }
    }

    // ============================================
    // 攻击逻辑
    // ============================================

    /// <summary>
    /// 开始攻击：生成孢子云 + 播放攻击动画
    /// </summary>
    void StartAttack()
    {
        isAttacking = true;
        attackAnimTimer = attackAnimDuration;

        // 播放音效
        if (audioSource != null && attackSound != null)
        {
            audioSource.clip = attackSound;
            audioSource.Play();
        }

        // 播放攻击动画（不循环）
        PlayAnimation(attackAnimation, false);

        // ★ 延迟生成爆炸
        StartCoroutine(SpawnAfterDelay(spawnDelay));
    }

    /// <summary>
    /// 延迟生成孢子云
    /// </summary>
    IEnumerator SpawnAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        Instantiate(sporeCloudPrefab, player.position, Quaternion.identity);
    }

    /// <summary>
    /// 结束攻击，回到呼吸动画
    /// </summary>
    void StopAttack()
    {
        isAttacking = false;
        attackAnimTimer = 0f;
        PlayAnimation(breathAnimation, true);
    }

    // ============================================
    // Spine 动画控制
    // ============================================

    /// <summary>
    /// 播放 Spine 动画
    /// </summary>
    /// <param name="animName">动画名</param>
    /// <param name="loop">是否循环</param>
    void PlayAnimation(string animName, bool loop)
    {
        if (skeletonAnimation == null) return;
        skeletonAnimation.AnimationState.SetAnimation(0, animName, loop);
    }

    // ============================================
    // 死亡
    // ============================================

    /// <summary>
    /// 死亡时生成爆炸特效
    /// </summary>
    public void OnDeathExplosion()
    {
        Instantiate(deathExplosionPrefab, transform.position, Quaternion.identity);
    }

    // ============================================
    // 编辑器可视化
    // ============================================

    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0f, 0f, 0.2f);
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }

    void OnDrawGizmos()
    {
        Gizmos.color = new Color(1f, 0f, 0f, 0.2f);
        Gizmos.DrawSphere(transform.position, attackRange);
    }
}