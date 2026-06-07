using UnityEngine;
using System.Collections;

public class EnemyDash : MonoBehaviour, IMovable
{
    [Header("状态参数")]
    [SerializeField] private float chaseRange = 8f;        // 发现玩家范围
    [SerializeField] private float attackRange = 5f;        // 触发冲刺范围
    [SerializeField] private float pauseDuration = 0.5f;    // 停顿时间
    [SerializeField] private float dashDistance = 4f;       // 冲刺距离（格数）
    [SerializeField] private float dashSpeed = 15f;         // 冲刺速度
    [SerializeField] private float stunDuration = 1f;       // 冲刺后硬直时间
    [SerializeField] private float contactDamage = 10f;     // 碰撞伤害（半颗心=10）

    [Header("移动")]
    //[SerializeField] private float patrolSpeed = 1f;
    [SerializeField] private float moveSpeed = 3f;

    private enum State { Patrol, Chase, Pause, Dash, Stun }
    private State currentState = State.Patrol;

    private Transform player;
    private Rigidbody2D rb;
    private float stateTimer;

    private bool hasAggro = false;
    private float patrolTimer;           //
    private Vector2 patrolDirection;     //

    private bool isKnockedBack = false;  // ★ 击退标记
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

        patrolTimer = Random.Range(1f, 3f);           //
        patrolDirection = Random.insideUnitCircle.normalized;  //
    }

    void Update()
    {
        if (isPaused||isKnockedBack) return;  // ★ 击退时跳过移动逻辑

        // 玩家伪装中 → 解除仇恨，回到巡逻
        if (player == null || !player.CompareTag("Player"))
        {
            hasAggro = false;
            patrolTimer -= Time.deltaTime;
            if (patrolTimer <= 0)
            {
                patrolDirection = Random.insideUnitCircle.normalized;
                patrolTimer = Random.Range(1f, 3f);
            }
            rb.velocity = patrolDirection * moveSpeed * 0.3f;
            return;
        }

        float dist = Vector2.Distance(transform.position, player.position);

        // 首次发现玩家，激活仇恨（永远不脱战）
        if (!hasAggro && dist <= chaseRange)
        {
            hasAggro = true;
            currentState = State.Chase;
        }

        // 还没发现玩家，待机
        if (!hasAggro)
        {
            patrolTimer -= Time.deltaTime;
            if (patrolTimer <= 0)
            {
                patrolDirection = Random.insideUnitCircle.normalized;
                patrolTimer = Random.Range(1f, 3f);
            }
            rb.velocity = patrolDirection * moveSpeed;
            return;
        }

        // 计时器递减（所有限时状态共用）
        stateTimer -= Time.deltaTime;

        // 状态切换逻辑
        switch (currentState)
        {
            case State.Chase:
                // 追到攻击范围 → 停顿
                if (dist <= attackRange)
                {
                    EnterState(State.Pause);
                }
                break;

            case State.Pause:
                // 停顿结束 → 冲刺
                if (stateTimer <= 0)
                {
                    EnterState(State.Dash);
                }
                break;

            case State.Dash:
                // 冲刺结束 → 硬直
                if (stateTimer <= 0)
                {
                    EnterState(State.Stun);
                }
                break;

            case State.Stun:
                // 硬直结束 → 继续追
                if (stateTimer <= 0)
                {
                    currentState = State.Chase;
                }
                break;
        }

        // 执行当前状态的行为
        switch (currentState)
        {
            case State.Chase:
                Chase();                      // 朝玩家移动
                break;

            case State.Pause:
                rb.velocity = Vector2.zero;   // 停顿不动
                break;

            case State.Dash:
                Dash();                       // 朝玩家方向冲刺
                break;

            case State.Stun:
                rb.velocity = Vector2.zero;   // 硬直不动
                break;
        }
    }

    /// <summary>
    /// 进入新状态并重置计时器
    /// </summary>
    void EnterState(State newState)
    {
        currentState = newState;

        // 根据不同状态设置计时器
        switch (newState)
        {
            case State.Pause: stateTimer = pauseDuration; break;
            case State.Dash: stateTimer = dashDistance / dashSpeed; break;
            case State.Stun: stateTimer = stunDuration; break;
        }
    }

    void Chase()
    {
        Vector2 dir = (player.position - transform.position).normalized;
        rb.velocity = dir * moveSpeed;
    }

    void Dash()
    {
        // 冲刺方向锁定为进入Dash瞬间的玩家方向
        // （实际开发中在EnterState时记录方向，这里简化为持续追踪）
        Vector2 dir = (player.position - transform.position).normalized;
        rb.velocity = dir * dashSpeed;
    }

    /// <summary>
    /// 碰撞到玩家造成伤害
    /// </summary>
    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("FireCol") && currentState == State.Dash)
        {
            Health health = collision.transform.parent.GetComponent<Health>();
            if (health != null)
                health.TakeDamage(contactDamage);
        }
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
        if (rb == null) rb = GetComponent<Rigidbody2D>();
        if (rb != null) rb.velocity = Vector2.zero;
    }

    public void ResumeMovement()
    {
        isPaused = false;
    }

    // ★★★ 在Scene视图中绘制范围 ★★★
    void OnDrawGizmosSelected()
    {
        // 追逐范围（发现玩家范围）
        Gizmos.color = new Color(1f, 0f, 0f, 0.3f);
        Gizmos.DrawWireSphere(transform.position, chaseRange);
    }

    // ★★★ 持续显示范围 ★★★
    void OnDrawGizmos()
    {
        // 追逐范围 - 实心半透明圆
        Gizmos.color = new Color(1f, 1f, 0f, 0.2f);
        Gizmos.DrawSphere(transform.position, chaseRange);
    }
}