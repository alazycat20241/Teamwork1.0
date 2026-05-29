using UnityEngine;

public class EnemyTrapper : MonoBehaviour, IMovable
{
    [Header("索敌参数")]
    [SerializeField] private float detectRange = 8f;        // 发现玩家范围
    [SerializeField] private float fleeDistance = 5f;       // 保持距离
    [SerializeField] private float moveSpeed = 3f;

    [Header("陷阱")]
    [SerializeField] private GameObject trapPrefab;         // 陷阱预制体
    [SerializeField] private float trapInterval = 7f;       // 种陷阱间隔
    //[SerializeField] private float trapDamage = 10f;        // 陷阱伤害

    private Transform player;
    private Rigidbody2D rb;
    private float trapTimer;

    private bool hasAggro = false;
    private float patrolTimer;           // 巡逻方向切换计时
    private Vector2 patrolDirection;     // 当前巡逻方向

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
        trapTimer = trapInterval;

        patrolTimer = Random.Range(1f, 3f);
        patrolDirection = Random.insideUnitCircle.normalized;
    }

    void Update()
    {
        if (isPaused||isKnockedBack) return;  // ★ 击退时跳过移动逻辑

        if (player == null) return;

        // 玩家伪装中 → 解除仇恨，巡逻
        if (!player.CompareTag("Player"))
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

        // 定时种陷阱
        trapTimer -= Time.deltaTime;
        if (trapTimer <= 0f)
        {
            trapTimer = trapInterval;   // 重置计时
            PlaceTrap();                // 在脚下放陷阱
        }

        // 首次进入探测范围，激活仇恨
        if (!hasAggro && dist <= detectRange)
        {
            hasAggro = true;
        }

        // 没发现玩家，缓慢巡逻
        if (!hasAggro)
        {
            // 简单巡逻：随机方向慢走
            patrolTimer -= Time.deltaTime;
            if (patrolTimer <= 0)
            {
                patrolDirection = Random.insideUnitCircle.normalized;
                patrolTimer = Random.Range(1f, 3f);
            }
            rb.velocity = patrolDirection * moveSpeed * 0.3f;
            return;
        }

        // 发现玩家后：在安全距离外自由移动
        if (dist < fleeDistance)
        {
            // 太近了 → 远离玩家
            Vector2 fleeDir = (transform.position - player.position).normalized;
            rb.velocity = fleeDir * moveSpeed;
        }
        else
        {
            // 安全距离外 → 自由巡逻
            patrolTimer -= Time.deltaTime;
            if (patrolTimer <= 0)
            {
                patrolDirection = Random.insideUnitCircle.normalized;
                patrolTimer = Random.Range(1f, 3f);
            }
            rb.velocity = patrolDirection * moveSpeed * 0.5f;
        }

    }

    /// <summary>
    /// 在当前位置种一个陷阱
    /// </summary>
    void PlaceTrap()
    {
        Instantiate(trapPrefab, transform.position, Quaternion.identity);
        // 建议用对象池管理陷阱，这里简化
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