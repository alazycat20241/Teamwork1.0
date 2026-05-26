using UnityEngine;

public class Enemy : MonoBehaviour,IMovable
{
    // 定义所有状态
    private enum State
    {
        Idle,           // 待机
        Patrol,         // 巡逻
        Chase,          // 追击
        Attack,         // 攻击
        Pause,          // 停顿（怪2用）
        Dash,           // 冲刺（怪2用）
        Stun,           // 硬直（怪2用）
        Flee,           // 远离（怪5用）
        LaserAttack     // 激光射击（怪4用）
    }

    [Header("敌人参数")]
    [SerializeField] private State currentState = State.Patrol;
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float chaseRange = 5f;      // 追击范围
    [SerializeField] private float attackRange = 1.5f;   // 攻击范围

    private bool hasAggro = false;  // 是否已激活仇恨（仇恨不消失）

    private Transform player;
    private Rigidbody2D rb;
    private Vector2 patrolDirection;
    private float patrolTimer;

    [Header("子弹发射")]
    [SerializeField] private BulletObject bulletObject;    // 敌人子弹配置
    [SerializeField] private float fireInterval = 0.5f;    // 发射间隔

    private BulletPool bulletPool;
    private float fireTimer;

    private bool isKnockedBack = false;  // ★ 击退标记

    void Awake()
    {
        // 初始化子弹对象池
        bulletPool = PoolManager.Instance.GetPool(bulletObject);
    }

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
        patrolDirection = Random.insideUnitCircle.normalized;
    }

    void Update()
    {
        if (isPaused||isKnockedBack) return;  // ★ 击退时跳过移动逻辑

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        // 首次进入追击范围，激活仇恨（永远不脱战）
        if (!hasAggro && distanceToPlayer <= chaseRange)
        {
            hasAggro = true;
            currentState = State.Chase;
        }

        // 还没发现玩家，待机不动
        if (!hasAggro)
        {
            patrolTimer -= Time.deltaTime;
            if (patrolTimer <= 0)
            {
                patrolDirection = Random.insideUnitCircle.normalized;
                patrolTimer = Random.Range(1f, 3f);
            }
            rb.velocity = patrolDirection * moveSpeed * 0.3f;
            return;
        }

        // 仇恨激活后：根据距离切换追击/攻击
        if (distanceToPlayer <= attackRange)
        {
            currentState = State.Attack;  // 进入攻击范围，停下来射击
        }
        else
        {
            currentState = State.Chase;   // 不在攻击范围，追过去
        }

        // 执行当前状态的行为
        switch (currentState)
        {
            case State.Chase:
                Chase();   // 朝玩家移动
                break;

            case State.Attack:
                Attack();  // 停住，定时发射子弹
                break;
        }
    }

    // 巡逻：走一段换个方向
    void Patrol()
    {
        patrolTimer -= Time.deltaTime;
        if (patrolTimer <= 0)
        {
            patrolDirection = Random.insideUnitCircle.normalized;
            patrolTimer = Random.Range(1f, 3f);
        }
        rb.velocity = patrolDirection * moveSpeed * 0.5f; // 巡逻比追击慢
    }

    // 追击：朝玩家走
    void Chase()
    {
        Vector2 dir = (player.position - transform.position).normalized;
        rb.velocity = dir * moveSpeed;
    }

    // 攻击：停住，用子弹系统（怪1）
    void Attack()
    {
        rb.velocity = Vector2.zero;
        // 这里调用子弹发射
        fireTimer += Time.deltaTime;
        if (fireTimer >= fireInterval)
        {
            fireTimer = 0f;
            FireBullet();
        }
    }

    void FireBullet()
    {
        if (bulletPool == null) return;

        // 方向指向玩家
        Vector2 dir = (player.position - transform.position).normalized;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

        // 从池中拿子弹
        BulletBehav bullet = bulletPool.GetItem();
        bullet.transform.position = transform.position;
        bullet.transform.rotation = Quaternion.Euler(0, 0, angle);
    }

    // 编辑器里画范围
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, chaseRange);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
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
