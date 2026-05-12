using UnityEngine;

public class Enemy : MonoBehaviour
{
    // 定义所有状态
    private enum State
    {
        Idle,       // 待机
        Patrol,     // 巡逻
        Chase,      // 追击
        Attack      // 攻击
    }

    [Header("敌人参数")]
    [SerializeField] private State currentState = State.Patrol;
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float chaseRange = 5f;      // 追击范围
    [SerializeField] private float attackRange = 1.5f;   // 攻击范围

    private Transform player;
    private Rigidbody2D rb;
    private Vector2 patrolDirection;
    private float patrolTimer;

    [Header("子弹发射")]
    [SerializeField] private BulletObject bulletObject;    // 敌人子弹配置
    [SerializeField] private float fireInterval = 0.5f;    // 发射间隔

    private BulletPool bulletPool;
    private float fireTimer;

    void Awake()
    {
        // 初始化子弹对象池
        bulletPool = PoolManager.Instance.GetPool(bulletObject);
    }

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0;
        rb.freezeRotation = true;
        patrolDirection = Random.insideUnitCircle.normalized;
    }

    void Update()
    {
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        // 状态切换逻辑
        switch (currentState)
        {
            case State.Idle:
            case State.Patrol:
                // 玩家进入追击范围 → 追击
                if (distanceToPlayer <= chaseRange)
                    currentState = State.Chase;
                break;

            case State.Chase:
                // 玩家进入攻击范围 → 攻击
                if (distanceToPlayer <= attackRange)
                    currentState = State.Attack;
                // 玩家跑远 → 回去巡逻
                else if (distanceToPlayer > chaseRange)
                    currentState = State.Patrol;
                break;

            case State.Attack:
                // 玩家离开攻击范围 → 继续追
                if (distanceToPlayer > attackRange)
                    currentState = State.Chase;
                break;
        }

        // 执行当前状态的行为
        switch (currentState)
        {
            case State.Idle:
                rb.velocity = Vector2.zero;
                break;

            case State.Patrol:
                Patrol();
                break;

            case State.Chase:
                Chase();
                break;

            case State.Attack:
                Attack();
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

    // 攻击：停住，用之前的孢子或子弹系统
    void Attack()
    {
        rb.velocity = Vector2.zero;
        // 这里调用孢子爆发或子弹发射
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
}
