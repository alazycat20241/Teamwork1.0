using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Spine;
using Spine.Unity;

/// <summary>
/// Boss 控制器
/// 一阶段：圆形弹幕（每次发射播一次 attack_eightDirections）
/// 二阶段（低于33%血量）：投掷炸弹 + 召唤小怪（每次攻击播一次 attack_summoning）
/// 实现 IMovable 接口，支持石化/击退/暂停
/// </summary>
public class Boss : MonoBehaviour, IMovable
{
    [Header("Boss参数")]
    [SerializeField] private float phase2HealthPercent = 0.33f; // 低于33%血进入二阶段

    [Header("一阶段：圆形弹幕")]
    [SerializeField] private BulletObject circleBulletConfig;   // 弹幕配置
    [SerializeField] private float circleFireInterval = 1.5f;   // 发射间隔（秒）

    [Header("二阶段：投掷孢子云")]
    [SerializeField] private GameObject sporeCloudPrefab;       // 炸弹预制体
    [SerializeField] private float bombInterval = 2f;           // 投弹间隔（秒）

    [Header("二阶段：召唤小怪")]
    [SerializeField] private List<GameObject> minionPrefabs;    // 小怪预制体列表
    [SerializeField] private int initialMinionCount = 2;        // 初始召唤数量下限
    [SerializeField] private int maxInitialMinion = 3;          // 初始召唤数量上限
    [SerializeField] private float summonInterval = 30f;        // 召唤间隔（秒）
    [SerializeField] private int summonCount = 1;               // 每次召唤数量
    [SerializeField] private float summonRadius = 3f;           // 召唤范围半径

    [Header("Spine 动画")]
    [SerializeField] private SkeletonAnimation skeletonAnimation;
    [SpineAnimation]
    [SerializeField] private string phaseOneAnim = "attack_eightDirections";   // 一阶段攻击动画
    [SpineAnimation]
    [SerializeField] private string phaseTwoAnim = "attack_summoning";         // 二阶段攻击动画

    [Header("范围显示")]
    [SerializeField] private Color summonRangeColor = new Color(0.5f, 0f, 1f, 0.2f);
    [SerializeField] private bool showRange = true;

    [Header("移动")]
    [SerializeField] private float moveSpeed = 0f;

    // ========== 状态机 ==========
    private enum Phase { One, Two }
    private Phase currentPhase = Phase.One;

    // ========== 组件引用 ==========
    private Transform player;
    private Health health;
    private Rigidbody2D rb;

    // ========== 对象池 ==========
    private BulletPool circleBulletPool;

    // ========== 攻击计时器 ==========
    private float circleFireTimer;  // 弹幕冷却
    private float bombTimer;        // 投弹冷却
    private float summonTimer;      // 召唤冷却

    // ========== 动画 ==========
    private string currentAnim;     // 当前播放的动画名（去重用）

    // ========== 状态标记（IMovable接口） ==========
    private bool isKnockedBack = false;
    private bool isPaused = false;

    [Header("音效")]
    [SerializeField] private AudioClip attackSound;    // 攻击音效（循环）

    private AudioSource attackAudioSource;  // 攻击音效的循环播放器
    // ==================== 生命周期 ====================

    void Start()
    {
        // 获取玩家引用
        GameObject playerObj = FixedRoomManager.Instance.GetPlayer();
        if (playerObj != null) player = playerObj.transform;

        // 获取自身组件
        health = GetComponent<Health>();
        rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.gravityScale = 0;
            rb.freezeRotation = true;
        }

        // 初始化对象池
        if (circleBulletConfig != null)
            circleBulletPool = PoolManager.Instance.GetPool(circleBulletConfig);

        // 初始化计时器
        circleFireTimer = circleFireInterval;
        bombTimer = bombInterval;
        summonTimer = summonInterval;

        attackAudioSource = gameObject.GetComponent<AudioSource>();
        attackAudioSource.playOnAwake = false;
        attackAudioSource.loop = false;
    }

    void Update()
    {
        // 暂停/击退时跳过
        if (isPaused || isKnockedBack) return;
        if (player == null || health == null || health.IsDead) return;

        // 血量低于阈值 → 进入二阶段
        if (currentPhase == Phase.One && health.CurrentHealth <= health.MaxHealth * phase2HealthPercent)
            EnterPhaseTwo();

        // 根据阶段执行攻击逻辑
        switch (currentPhase)
        {
            case Phase.One:
                PhaseOneUpdate();
                break;
            case Phase.Two:
                PhaseTwoUpdate();
                break;
        }
    }

    // ==================== 一阶段：圆形弹幕 ====================

    void PhaseOneUpdate()
    {
        circleFireTimer -= Time.deltaTime;
        // 提前 0.6 秒播放动画
        if (circleFireTimer <= 1f && circleFireTimer > 0f)
        {
            PlayAttackAnim(phaseOneAnim);
        }

        if (circleFireTimer <= 0f)
        {
            circleFireTimer = circleFireInterval;
            FireCircleBullets();
        }
    }

    /// <summary>
    /// 发射一圈圆形弹幕
    /// </summary>
    void FireCircleBullets()
    {
        if (circleBulletPool == null || circleBulletConfig == null) return;

        // 播放音效
        if (attackAudioSource != null && attackSound != null)
        {
            attackAudioSource.PlayOneShot(attackSound);
        }

        // 按配置生成一圈子弹
        for (int i = 0; i < circleBulletConfig.LineCount; i++)
        {
            float angle = i * circleBulletConfig.LineAngle;
            Quaternion rotation = Quaternion.Euler(0, 0, angle);

            BulletBehav bullet = circleBulletPool.GetItem();
            if (bullet != null)
            {
                bullet.transform.position = transform.position;
                bullet.transform.rotation = rotation;
            }
        }
    }

    // ==================== 二阶段：投弹 + 召唤 ====================

    void PhaseTwoUpdate()
    {
        // 投弹
        bombTimer -= Time.deltaTime;
        if (bombTimer <= 0f)
        {
            bombTimer = bombInterval;
            ThrowBomb();
        }

        // 召唤小怪
        summonTimer -= Time.deltaTime;
        if (summonTimer <= 0f)
        {
            summonTimer = summonInterval;
            SummonMinions(summonCount);
        }
    }

    /// <summary>
    /// 朝玩家位置投掷炸弹 + 播放二阶段攻击动画
    /// </summary>
    void ThrowBomb()
    {
        if (sporeCloudPrefab == null || player == null) return;

        // 播放攻击动画（每次投弹播一下）
        PlayAttackAnim(phaseTwoAnim);
        Instantiate(sporeCloudPrefab, player.position, Quaternion.identity);
    }

    /// <summary>
    /// 在周围随机位置召唤小怪 + 播放二阶段攻击动画
    /// </summary>
    void SummonMinions(int count)
    {
        if (minionPrefabs == null || minionPrefabs.Count == 0) return;

        // 播放攻击动画（每次召唤播一下）
        PlayAttackAnim(phaseTwoAnim);

        for (int i = 0; i < count; i++)
        {
            GameObject prefab = minionPrefabs[Random.Range(0, minionPrefabs.Count)];
            Vector2 offset = Random.insideUnitCircle.normalized * summonRadius;
            Vector3 spawnPos = transform.position + (Vector3)offset;
            Instantiate(prefab, spawnPos, Quaternion.identity);
        }
    }

    // ==================== 阶段切换 ====================

    /// <summary>
    /// 进入二阶段：立即召唤一波小怪，重置计时器
    /// </summary>
    void EnterPhaseTwo()
    {
        currentPhase = Phase.Two;

        int count = Random.Range(initialMinionCount, maxInitialMinion + 1);
        SummonMinions(count);

        bombTimer = bombInterval;
        summonTimer = summonInterval;
    }

    // ==================== Spine 动画控制 ====================

    /// <summary>
    /// 播放攻击动画（清缓存强制重播）
    /// </summary>
    void PlayAttackAnim(string animName)
    {
        if (skeletonAnimation == null) return;
        currentAnim = "";  // 清缓存，确保每次攻击都重播
        skeletonAnimation.AnimationState.SetAnimation(0, animName, false);
    }

    // ==================== IMovable 接口 ====================

    public float GetMoveSpeed() => moveSpeed;
    public void SetMoveSpeed(float speed) { moveSpeed = speed; }

    public void StartKnockback()
    {
        isKnockedBack = true;
        rb.velocity = Vector2.zero;
    }

    public void EndKnockback()
    {
        isKnockedBack = false;
        rb.velocity = Vector2.zero;
    }

    public void PauseMovement()
    {
        isPaused = true;
        if (rb == null) rb = GetComponent<Rigidbody2D>();
        if (rb != null) rb.velocity = Vector2.zero;
    }

    public void ResumeMovement() { isPaused = false; }

    // ==================== Gizmos 可视化 ====================

    void OnDrawGizmosSelected()
    {
        if (!showRange) return;
        Gizmos.color = summonRangeColor;
        Gizmos.DrawWireSphere(transform.position, summonRadius);
    }

    void OnDrawGizmos()
    {
        if (!showRange) return;
        Gizmos.color = summonRangeColor;
        Gizmos.DrawSphere(transform.position, summonRadius);
    }
}