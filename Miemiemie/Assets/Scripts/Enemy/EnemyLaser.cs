using UnityEngine;
using System.Collections;

public class EnemyLaser : MonoBehaviour
{
    [Header("索敌参数")]
    [SerializeField] private float chaseRange = 10f;        // 发现玩家范围
    [SerializeField] private float attackRange = 6f;         // 开始射击范围
    [SerializeField] private float moveSpeed = 2f;

    [Header("激光攻击")]
    [SerializeField] private float laserDuration = 1f;       // 射击持续时间
    [SerializeField] private float laserCooldown = 3f;       // 射击冷却
    [SerializeField] private float laserDamagePerSecond = 15f;
    [SerializeField] private LayerMask obstacleLayer;        // 障碍物层
    [SerializeField] private LineRenderer lineRenderer;      // 画线组件
    public float maxLength;

    [Header("定身效果")]
    [SerializeField] private float stunDuration = 1f;        // 定身时长

    private enum State { Chase, LaserAttack }
    private State currentState = State.Chase;

    private Transform player;
    private Rigidbody2D rb;
    private float cooldownTimer;
    private bool isLaserReady = true;

    private bool hasAggro = false;

    [Header("墙火花特效")]
    [SerializeField] private EffectPool wallSparkPool;          // 火花对象池（拖场景里的WallSparkPoolManager）
    private GameObject currentSpark;                            // 当前墙上的火花实例

    void Start()
    {
        GameObject playerObj = FixedRoomManager.Instance.GetPlayer();
        if (playerObj != null)
        {
            player = playerObj.transform;
        }
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0;
        rb.freezeRotation = true;
        lineRenderer.enabled = false;
    }

    void Update()
    {
        float dist = Vector2.Distance(transform.position, player.position);

        // 首次发现玩家，激活仇恨
        if (!hasAggro && dist <= chaseRange)
        {
            hasAggro = true;
            currentState = State.Chase;
        }

        // 没发现玩家，不动
        if (!hasAggro)
        {
            rb.velocity = Vector2.zero;
            return;
        }

        // 冷却计时
        // 冷却计时
        if (!isLaserReady)
        {
            cooldownTimer -= Time.deltaTime;
            if (cooldownTimer <= 0 && currentState != State.LaserAttack)  // 确保不在攻击中
            {
                isLaserReady = true;
            }
        }

        // 状态行为
        switch (currentState)
        {
            case State.Chase:
                // 进入攻击范围 且 冷却好了 → 开始激光射击
                if (dist <= attackRange && isLaserReady)
                {
                    EnterState(State.LaserAttack);
                }
                else
                {
                    Chase();  // 否则继续追玩家
                }
                break;

            case State.LaserAttack:
                rb.velocity = Vector2.zero;  // 射击时不移动（协程控制）
                break;
        }
    }

    void Chase()
    {
        Vector2 dir = (player.position - transform.position).normalized;
        rb.velocity = dir * moveSpeed;
    }

    void EnterState(State newState)
    {
        currentState = newState;
        switch (newState)
        {
            case State.LaserAttack:
                isLaserReady = false;    // 加这行
                StartCoroutine(LaserAttack());
                break;
        }
    }

    /// <summary>
    /// 激光攻击协程：持续射击一段时间，命中玩家则定身
    /// </summary>
    IEnumerator LaserAttack()
    {
        isLaserReady = false;
        rb.velocity = Vector2.zero;
        lineRenderer.enabled = true;

        float timer = 0f;
        bool playerHit = false;  // 是否已经命中玩家
        currentSpark = null;  // 重置火花引用

        while (timer < laserDuration)
        {
            timer += Time.deltaTime;

            // 更新激光方向（指向玩家）
            Vector2 direction = (player.position - transform.position).normalized;

            // 射线检测障碍物
            RaycastHit2D hit = Physics2D.Raycast(transform.position, direction, maxLength, obstacleLayer);

            Vector2 endPoint;
            if (hit.collider != null)
            {
                // 碰到墙：截断激光
                endPoint = hit.point;

                // === 火花逻辑 ===
                // 如果还没有火花，从池里取一个
                if (currentSpark == null && wallSparkPool != null)
                {
                    currentSpark = wallSparkPool.Get();
                }
                // 火花跟随碰撞点
                if (currentSpark != null)
                {
                    currentSpark.transform.position = endPoint;
                    // 火花朝向：垂直于墙面（hit.normal 是墙面法线方向）
                    float angle = Mathf.Atan2(hit.normal.y, hit.normal.x) * Mathf.Rad2Deg;
                    currentSpark.transform.rotation = Quaternion.Euler(0, 0, angle - 90f);
                }
            }
            else
            {
                // 没碰到墙：回收火花
                if (currentSpark != null && wallSparkPool != null)
                {
                    wallSparkPool.Release(currentSpark);
                    currentSpark = null;
                }
                endPoint = (Vector2)transform.position + direction * maxLength;
            }
            // 画线
            lineRenderer.SetPosition(0, transform.position);
            lineRenderer.SetPosition(1, endPoint);

            // 检测玩家是否在射线上
            float beamLength = Vector2.Distance(transform.position, endPoint);
            RaycastHit2D playerHit2D = Physics2D.Raycast(transform.position, direction, beamLength,
                1 << player.gameObject.layer);

            if (playerHit2D.collider != null && !playerHit)
            {
                playerHit = true;
                Health health = player.GetComponent<Health>();
                if (health != null)
                    health.TakeDamage(laserDamagePerSecond * laserDuration);

                // 定身玩家
                PlayerMove playerMovement = player.GetComponent<PlayerMove>();
                if (playerMovement != null)
                    playerMovement.Stun(stunDuration);
            }

            yield return null;
        }
        // 激光结束：回收火花
        if (currentSpark != null && wallSparkPool != null)
        {
            wallSparkPool.Release(currentSpark);
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
}