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

    [Header("定身效果")]
    [SerializeField] private float stunDuration = 1f;        // 定身时长

    private enum State { Chase, LaserAttack }
    private State currentState = State.Chase;

    private Transform player;
    private Rigidbody2D rb;
    private float cooldownTimer;
    private bool isLaserReady = true;

    private bool hasAggro = false;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
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
        if (!isLaserReady)
        {
            cooldownTimer -= Time.deltaTime;
            if (cooldownTimer <= 0)
            {
                isLaserReady = true;  // 冷却好了
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

        while (timer < laserDuration)
        {
            timer += Time.deltaTime;

            // 更新激光方向（指向玩家）
            Vector2 direction = (player.position - transform.position).normalized;

            // 射线检测障碍物
            RaycastHit2D hit = Physics2D.Raycast(transform.position, direction, 10f, obstacleLayer);

            Vector2 endPoint = hit.collider != null
                ? hit.point
                : (Vector2)transform.position + direction * 10f;

            // 画线
            lineRenderer.SetPosition(0, transform.position);
            lineRenderer.SetPosition(1, endPoint);

            // 检测玩家是否在射线上
            RaycastHit2D playerHit2D = Physics2D.Raycast(transform.position, direction,
                Vector2.Distance(transform.position, endPoint), 1 << player.gameObject.layer);

            if (playerHit2D.collider != null && !playerHit)
            {
                playerHit = true;
                // 对玩家造成伤害
                Health health = player.GetComponent<Health>();
                if (health != null)
                    health.TakeDamage(laserDamagePerSecond * laserDuration);

                // 给玩家施加定身Buff
                // 定身玩家
                PlayerMove playerMove = player.GetComponent<PlayerMove>();
                if (playerMove != null)
                {
                    playerMove.Stun(stunDuration);  // 定身1秒
                }
            }

            yield return null;
        }

        // 射击结束，关闭激光
        lineRenderer.enabled = false;
        cooldownTimer = laserCooldown;
        currentState = State.Chase;
    }
}