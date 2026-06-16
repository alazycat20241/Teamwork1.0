using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Boss 控制器
/// 一阶段：圆形弹幕 + 追击玩家
/// 二阶段（低于33%血量）：投掷炸弹 + 召唤小怪 + 加速追击
/// 实现 IMovable 接口，支持石化/击退/暂停
/// </summary>
public class Boss : MonoBehaviour, IMovable
{
    [Header("Boss参数")]
    [SerializeField] private float phase2HealthPercent = 0.33f; // 低于33%血进入二阶段

    [Header("一阶段：圆形弹幕")]
    [SerializeField] private BulletObject circleBulletConfig;   // 圆形弹幕配置（ScriptableObject）
    [SerializeField] private float circleFireInterval = 1.5f;   // 发射间隔（秒）

    [Header("二阶段：投掷孢子云")]
    [SerializeField] private GameObject sporeCloudPrefab;       // 爆炸预制体
    [SerializeField] private float bombInterval = 2f;// 投弹间隔（秒）

    [Header("二阶段：召唤小怪")]
    [SerializeField] private List<GameObject> minionPrefabs;    // 小怪预制体列表
    [SerializeField] private int initialMinionCount = 2;        // 初始召唤数量下限
    [SerializeField] private int maxInitialMinion = 3;          // 初始召唤数量上限
    [SerializeField] private float summonInterval = 30f;        // 召唤间隔（秒）
    [SerializeField] private int summonCount = 1;               // 每次召唤数量
    [SerializeField] private float summonRadius = 3f;           // 召唤范围半径

    [Header("移动")]
    [SerializeField] private float moveSpeed = 0f;              // 移动速度（只是为了接口）

    [Header("范围显示")]
    [SerializeField] private Color summonRangeColor = new Color(0.5f, 0f, 1f, 0.2f);  // 召唤范围颜色
    [SerializeField] private bool showRange = true;              // 是否显示范围

    // ========== 状态机 ==========
    private enum Phase { One, Two }
    private Phase currentPhase = Phase.One;

    // ========== 组件引用 ==========
    private Transform player;       // 玩家位置
    private Health health;          // 血量组件
    private Rigidbody2D rb;         // 刚体组件

    // ========== 对象池 ==========
    private BulletPool circleBulletPool;    // 圆形弹幕对象池

    // ========== 攻击计时器 ==========
    private float circleFireTimer;  // 圆形弹幕计时器
    private float bombTimer;        // 投弹计时器
    private float summonTimer;      // 召唤计时器

    // ========== 状态标记（IMovable接口使用） ==========
    private bool isKnockedBack = false;     // 是否被击退
    private bool isPaused = false;          // 是否暂停（石化/腐烂号角）

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
            rb.gravityScale = 0;        // 无重力
            rb.freezeRotation = true;   // 锁定旋转
        }

        // 初始化对象池（从PoolManager获取）
        if (circleBulletConfig != null)
            circleBulletPool = PoolManager.Instance.GetPool(circleBulletConfig);

        // 初始化计时器
        circleFireTimer = circleFireInterval;
        bombTimer = bombInterval;
        summonTimer = summonInterval;
    }

    void Update()
    {
        // 暂停/击退时跳过所有逻辑
        if (isPaused || isKnockedBack) return;

        // 安全检查
        if (player == null || health == null || health.IsDead) return;

        // 血量低于阈值 → 进入二阶段
        if (currentPhase == Phase.One && health.CurrentHealth <= health.MaxHealth * phase2HealthPercent)
        {
            EnterPhaseTwo();
        }

        // 根据当前阶段执行不同行为
        switch (currentPhase)
        {
            case Phase.One:
                PhaseOneUpdate();   // 一阶段：弹幕 + 追击
                break;
            case Phase.Two:
                PhaseTwoUpdate();   // 二阶段：炸弹 + 召唤 + 加速追击
                break;
        }
    }

    // ==================== 一阶段 ====================

    /// <summary>
    /// 一阶段每帧更新：朝玩家移动 + 定时发射圆形弹幕
    /// </summary>
    void PhaseOneUpdate()
    {
        // 弹幕计时
        circleFireTimer -= Time.deltaTime;
        if (circleFireTimer <= 0f)
        {
            circleFireTimer = circleFireInterval;
            FireCircleBullets();
        }
    }

    /// <summary>
    /// 发射一圈子弹（数量、角度由BulletObject配置决定）
    /// </summary>
    void FireCircleBullets()
    {
        if (circleBulletPool == null || circleBulletConfig == null) return;

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

    // ==================== 二阶段 ====================

    /// <summary>
    /// 二阶段每帧更新：加速追击 + 投弹 + 召唤小怪
    /// </summary>
    void PhaseTwoUpdate()
    {
        // 投弹计时
        bombTimer -= Time.deltaTime;
        if (bombTimer <= 0f)
        {
            bombTimer = bombInterval;
            ThrowBomb();
        }

        // 召唤计时
        summonTimer -= Time.deltaTime;
        if (summonTimer <= 0f)
        {
            summonTimer = summonInterval;
            SummonMinions(summonCount);
        }
    }

    /// <summary>
    /// 朝玩家方向投掷一枚炸弹
    /// </summary>
    void ThrowBomb()
    {
        if (sporeCloudPrefab == null || player == null) return;

        // 直接在玩家当前位置生成
        Instantiate(sporeCloudPrefab, player.position, Quaternion.identity);
    }

    /// <summary>
    /// 在周围随机位置召唤小怪
    /// </summary>
    /// <param name="count">召唤数量</param>
    void SummonMinions(int count)
    {
        if (minionPrefabs == null || minionPrefabs.Count == 0) return;

        for (int i = 0; i < count; i++)
        {
            // 随机选择小怪类型
            GameObject prefab = minionPrefabs[Random.Range(0, minionPrefabs.Count)];

            // 在召唤半径内随机位置
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

        // 立刻召唤2-3个小怪
        int count = Random.Range(initialMinionCount, maxInitialMinion + 1);
        SummonMinions(count);

        // 重置计时器
        bombTimer = bombInterval;
        summonTimer = summonInterval;
    }

    // ==================== IMovable 接口实现 ====================

    /// <summary>
    /// 获取当前移动速度
    /// </summary>
    public float GetMoveSpeed() => moveSpeed;

    /// <summary>
    /// 设置移动速度
    /// </summary>
    public void SetMoveSpeed(float speed)
    {
        moveSpeed = speed;
    }

    /// <summary>
    /// 开始击退（停止移动）
    /// </summary>
    public void StartKnockback()
    {
        isKnockedBack = true;
        rb.velocity = Vector2.zero;
    }

    /// <summary>
    /// 结束击退（恢复移动）
    /// </summary>
    public void EndKnockback()
    {
        isKnockedBack = false;
        rb.velocity = Vector2.zero;
    }

    /// <summary>
    /// 暂停移动（石化/腐烂号角等效果使用）
    /// </summary>
    public void PauseMovement()
    {
        isPaused = true;
        if (rb == null) rb = GetComponent<Rigidbody2D>();
        if (rb != null) rb.velocity = Vector2.zero;
    }

    /// <summary>
    /// 恢复移动
    /// </summary>
    public void ResumeMovement()
    {
        isPaused = false;
    }

    // ==================== Gizmos 范围可视化 ====================

    /// <summary>
    /// 选中时显示召唤范围线框
    /// </summary>
    void OnDrawGizmosSelected()
    {
        if (!showRange) return;
        Gizmos.color = summonRangeColor;
        Gizmos.DrawWireSphere(transform.position, summonRadius);
    }

    /// <summary>
    /// 始终显示召唤范围半透明球
    /// </summary>
    void OnDrawGizmos()
    {
        if (!showRange) return;
        Gizmos.color = summonRangeColor;
        Gizmos.DrawSphere(transform.position, summonRadius);
    }
}