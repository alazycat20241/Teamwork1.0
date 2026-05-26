using UnityEngine;
using System.Collections;

public class EnemyLaser : MonoBehaviour, IMovable
{
    [Header("索敌参数")]
    [SerializeField] private float chaseRange = 10f;        // 发现玩家范围
    [SerializeField] private float attackRange = 6f;         // 开始射击范围
    [SerializeField] private float moveSpeed = 2f;           // 移动速度

    [Header("激光攻击")]
    [SerializeField] private float laserDuration = 1f;       // 射击持续时间
    [SerializeField] private float laserCooldown = 3f;       // 射击冷却时间
    [SerializeField] private float laserDamagePerSecond = 15f; // 每秒伤害
    [SerializeField] private LayerMask obstacleLayer;        // 障碍物层（墙、障碍物）
    [SerializeField] private LineRenderer lineRenderer;      // 激光线渲染器
    [SerializeField] public float maxLength = 20f;           // 激光最大长度

    [Header("定身效果")]
    [SerializeField] private float stunDuration = 1f;        // 定身时长

    // 状态机
    private enum State { Chase, LaserAttack }
    private State currentState = State.Chase;

    // 组件引用
    private Transform player;        // 玩家位置
    private Rigidbody2D rb;          // 刚体组件
    private float cooldownTimer;     // 冷却计时器
    private bool isLaserReady = true; // 激光是否准备就绪

    private bool hasAggro = false;   // 是否已激活仇恨

    // 特效相关（使用单例，不再需要拖拽）
    private GameObject currentSpark; // 当前墙上的火花实例

    private bool isKnockedBack = false;  // ★ 击退标记

    void Start()
    {
        // 获取玩家引用（从房间管理器）
        GameObject playerObj = FixedRoomManager.Instance.GetPlayer();
        if (playerObj != null)
        {
            player = playerObj.transform;
        }
        else
        {
            Debug.LogError("EnemyLaser: 无法找到玩家！");
        }

        // 初始化刚体
        rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.gravityScale = 0;      // 不受重力影响
            rb.freezeRotation = true; // 锁定旋转
        }

        // 初始化激光渲染器
        if (lineRenderer != null)
        {
            lineRenderer.enabled = false;
        }
    }

    void Update()
    {
        if (isPaused||isKnockedBack) return;  // ★ 击退时跳过移动逻辑

        // 安全检查：玩家不存在时不做任何操作
        if (player == null) return;

        float dist = Vector2.Distance(transform.position, player.position);

        // 首次发现玩家，激活仇恨
        if (!hasAggro && dist <= chaseRange)
        {
            hasAggro = true;
            currentState = State.Chase;
        }

        // 未发现玩家时，保持静止
        if (!hasAggro)
        {
            rb.velocity = Vector2.zero;
            return;
        }

        // 冷却计时逻辑
        if (!isLaserReady)
        {
            cooldownTimer -= Time.deltaTime;
            // 冷却结束且不在攻击状态时，重置激光准备标记
            if (cooldownTimer <= 0 && currentState != State.LaserAttack)
            {
                isLaserReady = true;
            }
        }

        // 状态机行为
        switch (currentState)
        {
            case State.Chase:
                // 进入攻击范围 且 冷却完成 → 开始激光攻击
                if (dist <= attackRange && isLaserReady)
                {
                    EnterState(State.LaserAttack);
                }
                else
                {
                    Chase(); // 继续追击玩家
                }
                break;

            case State.LaserAttack:
                // 攻击状态下保持静止（移动由协程控制）
                rb.velocity = Vector2.zero;
                break;
        }
    }

    /// <summary>
    /// 追击玩家
    /// </summary>
    void Chase()
    {
        Vector2 dir = (player.position - transform.position).normalized;
        rb.velocity = dir * moveSpeed;
    }

    /// <summary>
    /// 进入指定状态
    /// </summary>
    void EnterState(State newState)
    {
        currentState = newState;
        switch (newState)
        {
            case State.LaserAttack:
                isLaserReady = false;
                StartCoroutine(LaserAttack());
                break;
        }
    }


    /// <summary>
    /// 激光攻击协程：持续射击一段时间，命中玩家则造成伤害并定身
    /// </summary>
    IEnumerator LaserAttack()
    {
        // 攻击准备
        isLaserReady = false;
        rb.velocity = Vector2.zero;

        // 开启激光渲染
        if (lineRenderer != null)
        {
            lineRenderer.enabled = true;
            lineRenderer.widthMultiplier = 1f; // 重置宽度
        }

        float timer = 0f;
        bool playerHit = false;      // 本次攻击是否已命中玩家（只命中一次）
        currentSpark = null;         // 重置火花引用

        // 持续射击直到持续时间结束
        while (timer < laserDuration)
        {
            timer += Time.deltaTime;

            // 更新激光方向（持续指向玩家当前位置）
            Vector2 direction = (player.position - transform.position).normalized;

            // 射线检测：从敌人位置沿激光方向发射，检测障碍物
            RaycastHit2D hit = Physics2D.Raycast(transform.position, direction, maxLength, obstacleLayer);

            Vector2 endPoint; // 激光终点
            if (hit.collider != null)
            {
                // ========== 碰到障碍物（墙）：激光截断 ==========
                endPoint = hit.point;

                // --- 火花特效逻辑（使用单例对象池）---
                // 如果还没有火花实例，从对象池获取一个
                if (currentSpark == null && EffectPool.Instance != null)
                {
                    currentSpark = EffectPool.Instance.Get("LaserSpark");  // 改1
                }

                // 更新火花位置和朝向
                if (currentSpark != null)
                {
                    currentSpark.transform.position = endPoint;
                    // 火花朝向：垂直于墙面（hit.normal 是墙面的法线方向）
                    float angle = Mathf.Atan2(hit.normal.y, hit.normal.x) * Mathf.Rad2Deg;
                    currentSpark.transform.rotation = Quaternion.Euler(0, 0, angle - 90f);
                }
            }
            else
            {
                // ========== 没有碰到障碍物：激光延伸到最大长度 ==========
                // 回收火花特效（如果有）
                if (currentSpark != null && EffectPool.Instance != null)
                {
                    EffectPool.Instance.Release("LaserSpark", currentSpark);  // 改2
                    currentSpark = null;
                }
                endPoint = (Vector2)transform.position + direction * maxLength;
            }

            // 绘制激光线
            if (lineRenderer != null)
            {
                lineRenderer.SetPosition(0, transform.position);
                lineRenderer.SetPosition(1, endPoint);
            }

            // ========== 玩家伤害判定 ==========
            // 计算激光实际长度
            float beamLength = Vector2.Distance(transform.position, endPoint);
            // 发射射线检测玩家（只检测玩家层）
            RaycastHit2D playerHit2D = Physics2D.Raycast(transform.position, direction, beamLength,
                1 << player.gameObject.layer);

            // 如果激光命中玩家且本次攻击还未造成伤害
            if (playerHit2D.collider != null && !playerHit)
            {
                playerHit = true;

                // 造成伤害（总伤害 = 每秒伤害 × 持续时间）
                Health health = player.GetComponent<Health>();
                if (health != null)
                {
                    health.TakeDamage(laserDamagePerSecond * laserDuration);
                }
                // ★ 检查玩家是否还活跃
                if (player.gameObject.activeInHierarchy)
                {
                    PlayerMove playerMovement = player.GetComponent<PlayerMove>();
                    if (playerMovement != null)
                    {
                        playerMovement.Stun(stunDuration);
                    }
                }
                // 定身玩家
            }

            yield return null; // 等待下一帧
        }

        // ========== 激光攻击结束，清理资源 ==========
        // 回收火花特效
        // 激光结束：回收火花
        if (currentSpark != null && EffectPool.Instance != null)
        {
            EffectPool.Instance.Release("LaserSpark", currentSpark);  // 改3
            currentSpark = null;
        }

        // 射击结束 → 变细消失
        float fadeOutDuration = 0.15f;
        float elapsed = 0f;
        float startWidth = lineRenderer.widthMultiplier;

        while (elapsed < fadeOutDuration)
        {
            elapsed += Time.deltaTime;
            lineRenderer.widthMultiplier = Mathf.Lerp(startWidth, 0f, elapsed / fadeOutDuration);
            yield return null;
        }

        lineRenderer.widthMultiplier = startWidth;  // 恢复宽度
        lineRenderer.enabled = false;
        cooldownTimer = laserCooldown;
        isLaserReady = false;
        currentState = State.Chase;
    }
    /// <summary>
    /// 清理激光特效（供外部调用，比如敌人死亡时）
    /// </summary>
    public void CleanupLaser()
    {
        // 回收火花
        if (currentSpark != null && EffectPool.Instance != null)
        {
            EffectPool.Instance.Release("LaserSpark", currentSpark);  // 改4
            currentSpark = null;
        }

        // 关闭激光
        if (lineRenderer != null)
        {
            lineRenderer.enabled = false;
        }

        // 停止协程
        StopAllCoroutines();
    }

    public float GetMoveSpeed() => moveSpeed;

    public void SetMoveSpeed(float speed)
    {
        moveSpeed = speed;
    }

    public void StartKnockback()
    {
        isKnockedBack = true;  // 暂停移动
    }

    public void EndKnockback()
    {
        isKnockedBack = false;
        rb.velocity = Vector2.zero;  // 击退结束
    }

    private bool isPaused = false;

    public void PauseMovement()
    {
        isPaused = true;
        rb.velocity = Vector2.zero;
    }

    public void ResumeMovement()
    {
        isPaused = false;
    }
}