using UnityEngine;
using System.Collections;
using Spine;
using Spine.Unity;

/// <summary>
/// 触手型敌人
/// 巡逻/追逐时显示第一帧静止，攻击时朝玩家方向甩出手臂
/// 手臂通过 PathConstraint 延长，根骨骼可旋转
///   右手骨骼：-15° 为向上
///   左手骨骼：-195° 为向上
/// 攻击流程：朝玩家旋转手臂 → 正向播放 attack_kongzhi 甩出 → 反向播放收回
/// 手臂碰撞框检测到玩家 → 造成伤害 + 定身（一次攻击只命中一次）
/// 
/// 使用 bodyCenter 作为身体逻辑中心点，避免因骨骼偏移导致判断错误
/// </summary>
public class EnemyTentacle : MonoBehaviour, IMovable
{
    [Header("索敌参数")]
    [SerializeField] private float chaseRange = 10f;        // 发现玩家范围
    [SerializeField] private float attackRange = 6f;         // 开始攻击范围
    [SerializeField] private float moveSpeed = 2f;           // 移动速度

    [Header("攻击参数")]
    [SerializeField] private float attackCooldown = 3f;     // 攻击冷却（秒）
    [SerializeField] private float stunDuration = 1f;        // 定身时长

    [Header("Spine 动画")]
    [SerializeField] private SkeletonAnimation skeletonAnimation;
    [SpineAnimation]
    [SerializeField] private string attackAnimName = "attack_kongzhi";  // 攻击动画名

    [Header("身体中心点（拖一个空物体，放在怪物身体正中心）")]
    [SerializeField] private Transform bodyCenter;           // ★ 逻辑中心

    [Header("手臂骨骼 Transform（挂 SkeletonUtilityBone 的 GameObject）")]
    [SerializeField] private Transform rightArmTransform;   // 右手臂路径根骨骼
    [SerializeField] private Transform leftArmTransform;    // 左手臂路径根骨骼

    // 状态机
    private enum State { Patrol, Chase, Attack, Cooldown }
    private State currentState = State.Patrol;

    // 组件引用
    private Transform player;
    private Rigidbody2D rb;
    private float cooldownTimer;
    private bool hasAggro = false;

    // 巡逻
    private Vector2 patrolDirection;
    private float patrolTimer;

    // 击退/暂停
    private bool isKnockedBack = false;
    private bool isPaused = false;

    // 攻击状态
    public bool isAttacking;           // 是否正在攻击
    private bool playerHitThisAttack;   // 本次攻击是否已命中（只命中一次）
    private float animDirection;        // 1=正向甩出，-1=反向收回

    // ============================================
    // 初始化
    // ============================================
    void Start()
    {
        // 获取玩家
        GameObject playerObj = FixedRoomManager.Instance.GetPlayer();
        if (playerObj != null)
            player = playerObj.transform;

        // 配置刚体
        rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.gravityScale = 0;
            rb.freezeRotation = true;
        }

        // 初始化巡逻
        patrolDirection = Random.insideUnitCircle.normalized;

        // ★ 如果没有拖入 bodyCenter，默认用自身 Transform
        if (bodyCenter == null)
            bodyCenter = transform;

        // 初始冻结在第一帧，隐藏手臂碰撞
        FreezeAtFirstFrame();
    }

    // ============================================
    // 每帧更新
    // ============================================
    void Update()
    {
        if (isPaused || isKnockedBack) return;
        if (player == null) return;

        // 玩家伪装 → 取消仇恨
        if (!player.CompareTag("Player"))
        {
            hasAggro = false;
            return;
        }

        // ★ 用 bodyCenter 计算距离
        float dist = Vector2.Distance(bodyCenter.position, player.position);

        // 首次发现玩家
        if (!hasAggro && dist <= chaseRange)
        {
            hasAggro = true;
            currentState = State.Chase;
        }

        // 未发现 → 巡逻
        if (!hasAggro)
        {
            UpdatePatrol();
            return;
        }

        // 冷却计时（攻击时不走冷却）
        if (!isAttacking)
            cooldownTimer -= Time.deltaTime;

        // 状态机
        switch (currentState)
        {
            case State.Chase:
                if (dist <= attackRange && cooldownTimer <= 0f)
                    EnterState(State.Attack);
                else
                    Chase();
                break;

            case State.Attack:
                rb.velocity = Vector2.zero;
                UpdateAttackAnimation();
                break;

            case State.Cooldown:
                rb.velocity = Vector2.zero;
                if (cooldownTimer <= 0f)
                    currentState = State.Chase;
                break;
        }
    }

    // ============================================
    // 巡逻
    // ============================================
    void UpdatePatrol()
    {
        patrolTimer -= Time.deltaTime;
        if (patrolTimer <= 0)
        {
            patrolDirection = Random.insideUnitCircle.normalized;
            patrolTimer = Random.Range(1f, 3f);
        }
        rb.velocity = patrolDirection * moveSpeed * 0.3f;
    }

    // ============================================
    // 追逐
    // ============================================
    void Chase()
    {
        // ★ 用 bodyCenter 计算方向
        Vector2 dir = (player.position - bodyCenter.position).normalized;
        rb.velocity = dir * moveSpeed;
    }

    // ============================================
    // 进入状态
    // ============================================
    void EnterState(State newState)
    {
        currentState = newState;
        if (newState == State.Attack)
            StartAttack();
    }

    // ============================================
    // 攻击逻辑
    // ============================================

    /// <summary>
    /// 开始攻击：旋转手臂朝向玩家 → 正向播放动画甩出
    /// </summary>
    void StartAttack()
    {
        isAttacking = true;
        playerHitThisAttack = false;
        animDirection = 1f;

        // 旋转手臂朝向玩家
        RotateArmsTowardsPlayer();

        // ★ 延迟一帧再播动画，确保旋转生效
        StartCoroutine(PlayAttackAnimNextFrame());
    }

    IEnumerator PlayAttackAnimNextFrame()
    {
        yield return null; // 等一帧

        if (skeletonAnimation != null)
        {
            var track = skeletonAnimation.AnimationState.SetAnimation(0, attackAnimName, false);
            track.TrackTime = 0f;
            track.TimeScale = 1f;
        }
    }

    /// <summary>
    /// 旋转手臂骨骼，使其朝向玩家方向
    /// 以 bodyCenter 为基准计算玩家方向
    /// </summary>
    void RotateArmsTowardsPlayer()
    {
        if (player == null) return;

        Vector2 dirToPlayer = (player.position - bodyCenter.position).normalized;
        if (dirToPlayer == Vector2.zero) return;

        float targetAngle = Mathf.Atan2(dirToPlayer.y, dirToPlayer.x) * Mathf.Rad2Deg;

        if (rightArmTransform != null)
        {
            float rightRotation =  targetAngle;
            rightArmTransform.rotation = Quaternion.Euler(0, 0, rightRotation);
        }

        if (leftArmTransform != null)
        {
            float leftRotation =  targetAngle-180f;
            leftArmTransform.rotation = Quaternion.Euler(0, 0, leftRotation);
        }
    }

    /// <summary>
    /// 每帧更新攻击动画
    /// 正向播完 → 自动反向播放收回
    /// 反向播完 → 攻击结束
    /// </summary>
    void UpdateAttackAnimation()
    {
        if (!isAttacking) return;

        var track = skeletonAnimation?.AnimationState?.GetCurrent(0);
        if (track == null) return;

        // 正向甩出播完 → 开始反向收回
        if (animDirection == 1f && track.IsComplete)
        {
            EndAttack();

        }
    }

    /// <summary>
    /// 攻击结束：冷却、冻结第一帧、关闭碰撞
    /// </summary>
    void EndAttack()
    {
        isAttacking = false;
        cooldownTimer = attackCooldown;
        currentState = State.Cooldown;

        FreezeAtFirstFrame();
    }

    /// <summary>
    /// 检测手臂碰撞框是否碰到玩家
    /// </summary>
    public void OnArmHitPlayer(GameObject playerObj)
    {
        if (playerHitThisAttack) return; // 一次攻击只命中一次
        if (!isAttacking) return;        // 非攻击状态不触发

        playerHitThisAttack = true;

        PlayerMove playerMovement = playerObj.GetComponent<PlayerMove>();
        if (playerMovement != null)
            playerMovement.Stun(stunDuration);
    }

    // ============================================
    // Spine 动画控制
    // ============================================

    /// <summary>
    /// 冻结动画在第一帧（巡逻/追逐/冷却时显示静止姿态）
    /// </summary>
    void FreezeAtFirstFrame()
    {
        if (skeletonAnimation == null) return;
        var track = skeletonAnimation.AnimationState.SetAnimation(0, attackAnimName, false);
        track.TrackTime = 0f;
        track.TimeScale = 0f;
    }

    // ============================================
    // 外部清理（死亡时调用）
    // ============================================
    public void Cleanup()
    {
        StopAllCoroutines();
    }

    // ============================================
    // IMovable 接口
    // ============================================
    public float GetMoveSpeed() => moveSpeed;
    public void SetMoveSpeed(float speed) { moveSpeed = speed; }
    public void StartKnockback() { isKnockedBack = true; }
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

    // ============================================
    // 编辑器可视化（用 bodyCenter 绘制）
    // ============================================
    void OnDrawGizmosSelected()
    {
        Vector3 center = bodyCenter != null ? bodyCenter.position : transform.position;

        Gizmos.color = new Color(1f, 0f, 0f, 0.2f);
        Gizmos.DrawWireSphere(center, chaseRange);
        Gizmos.color = new Color(1f, 1f, 0f, 0.2f);
        Gizmos.DrawWireSphere(center, attackRange);
    }
}