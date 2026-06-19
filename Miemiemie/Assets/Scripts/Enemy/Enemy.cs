using Spine.Unity;
using UnityEngine;
using System.Collections;
public class Enemy : MonoBehaviour,IMovable
{
    // 定义所有状态
    private enum State
    {
        Idle,           // 待机
        Patrol,         // 巡逻
        Chase,          // 追击
        Attack,         // 攻击
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

    // ★ Spine 动画
    // ============================================
    [Header("Spine 动画")]
    [SerializeField] private SkeletonAnimation skeletonAnimation;
    [SpineAnimation]
    [SerializeField] private string walkWithoutHate = "walk_without hate";       // 巡逻
    [SpineAnimation]
    [SerializeField] private string findingPlayer = "finding the player";        // 发现玩家（一次性）
    [SpineAnimation]
    [SerializeField] private string walkAfterFinding = "walk_after finding player"; // 追逐
    [SpineAnimation]
    [SerializeField] private string attackAnim = "attack";                       // 攻击循环

    private bool hasPlayedFinding;  // 是否已播放过"发现玩家"动画
    private string currentAnim;  // 记录当前播放的动画名

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

        // ★ 初始播放巡逻动画
        PlayAnimation(walkWithoutHate, true);
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

            FlipByVelocity(patrolDirection);  // ★ 翻转
            return;
        }

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        // 首次进入追击范围，激活仇恨（永远不脱战）
        if (!hasAggro && distanceToPlayer <= chaseRange)
        {
            hasAggro = true;
            currentState = State.Chase;

            hasPlayedFinding = false;  // 准备播放
        }

        // 还没发现玩家，巡逻
        if (!hasAggro)
        {
            patrolTimer -= Time.deltaTime;
            if (patrolTimer <= 0)
            {
                patrolDirection = Random.insideUnitCircle.normalized;
                patrolTimer = Random.Range(1f, 3f);
            }
            rb.velocity = patrolDirection * moveSpeed * 0.3f;

            FlipByVelocity(patrolDirection);  // ★ 翻转
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

    // 追击：朝玩家走
    void Chase()
    {
        Vector2 dir = (player.position - transform.position).normalized;
        rb.velocity = dir * moveSpeed;

        FlipByVelocity(dir);  // ★ 翻转

        // ★ 动画：先播 findingPlayer（一次），然后 walkAfterFinding
        if (!hasPlayedFinding)
        {
            hasPlayedFinding = true;
            PlayAnimation(findingPlayer, false);
            // findingPlayer 播完后自动切 walkAfterFinding
            StartCoroutine(PlayAfterFinding());
        }
    }

    // 攻击：停住，用子弹系统（怪1）
    void Attack()
    {
        rb.velocity = Vector2.zero;

        // ★ 攻击动画
        PlayAnimation(attackAnim, true);

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

    /// <summary>
    /// findingPlayer 播完后切换到追逐动画
    /// </summary>
    IEnumerator PlayAfterFinding()
    {
        // 等 findingPlayer 播完
        yield return new WaitForSeconds(1f); // 或获取动画时长，这里先写死
        // 如果还在追逐状态，切到追逐动画
        if (hasAggro && currentState == State.Chase)
            PlayAnimation(walkAfterFinding, true);
    }

    // ============================================
    // ★ Spine 动画播放
    // ============================================
    void PlayAnimation(string animName, bool loop)
    {
        if (skeletonAnimation == null) return;
        if (animName == currentAnim) return;  // ★ 同一个动画不重复播
        currentAnim = animName;
        skeletonAnimation.AnimationState.SetAnimation(0, animName, loop);
    }

    /// <summary>
    /// 根据移动方向左右翻转
    /// </summary>
    void FlipByVelocity(Vector2 velocity)
    {
        if (skeletonAnimation == null) return;

        if (velocity.x > 0.1f)
            skeletonAnimation.Skeleton.ScaleX = 1f;
        else if (velocity.x < -0.1f)
            skeletonAnimation.Skeleton.ScaleX = -1f;
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
        if (rb == null) rb = GetComponent<Rigidbody2D>();
        if (rb != null) rb.velocity = Vector2.zero;
    }

    public void ResumeMovement()
    {
        isPaused = false;
    }
}
