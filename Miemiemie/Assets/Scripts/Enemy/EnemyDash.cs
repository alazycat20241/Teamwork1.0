using UnityEngine;
using System.Collections;
using Spine;
using Spine.Unity;

/// <summary>
/// 冲刺型敌人
/// 行为流程：
///   巡逻(左右翻转) → 发现玩家 → 追逐(左右翻转) → 蓄力(面朝玩家左右翻转)
///   → 冲刺(360度旋转) → 硬直(Idle呼吸)
/// 
/// 使用两个独立的 SkeletonDataAsset（蓄力和冲刺骨骼/贴图不同，挂在子物体上）
/// 空闲/硬直时显示蓄力动画第一帧 + 呼吸缩放效果
/// 实现 IMovable 接口，支持外部击退和暂停控制
/// </summary>
public class EnemyDash : MonoBehaviour, IMovable
{
    // ============================================
    // 状态参数（Inspector 可调）
    // ============================================
    [Header("状态参数")]
    [SerializeField] private float chaseRange = 8f;        // 发现玩家的圆形半径
    [SerializeField] private float attackRange = 5f;        // 触发蓄力的圆形半径
    [SerializeField] private float pauseDuration = 0.5f;    // 蓄力停顿时间（秒），期间播放蓄力动画
    [SerializeField] private float dashDistance = 4f;       // 冲刺距离（格数），用于计算冲刺持续时长
    [SerializeField] private float dashSpeed = 15f;         // 冲刺移动速度
    [SerializeField] private float stunDuration = 1f;       // 冲刺后硬直时间（秒），期间无法行动
    [SerializeField] private float contactDamage = 10f;     // 冲刺碰撞伤害（半颗心 = 10）

    [Header("移动")]
    [SerializeField] private float moveSpeed = 3f;          // 追逐/巡逻移动速度
    [SerializeField] private float triggerBuffer = 1f;      // 缓冲带距离：硬直结束后玩家必须走出 attackRange+buffer 才重新追逐

    // ============================================
    // Spine 动画配置
    // 两个 SkeletonDataAsset 骨骼/贴图不同，无法合并，分别挂在子物体上
    // ============================================
    [Header("Spine - 蓄力动画（也用于空闲展示）")]
    [SerializeField] private SkeletonAnimation chargeSkeleton;   // 蓄力子物体上的 SkeletonAnimation 组件
    [SpineAnimation]                                              // Inspector 显示为下拉菜单
    [SerializeField] private string chargeAnimation = "animation"; // 蓄力动画名（SkeletonData 里的实际名称）

    [Header("Spine - 冲刺动画")]
    [SerializeField] private SkeletonAnimation dashSkeleton;      // 冲刺子物体上的 SkeletonAnimation 组件
    [SpineAnimation]
    [SerializeField] private string dashAnimation = "animation";  // 冲刺动画名

    [Header("Spine - 空闲呼吸效果")]
    [SerializeField] private float breatheSpeed = 2f;      // 呼吸缩放频率（值越大越快）
    [SerializeField] private float breatheAmount = 0.05f;   // 呼吸缩放幅度（0.05 = 5% 的大小变化）

    // ============================================
    // 状态枚举
    // ============================================
    /// <summary>敌人行为状态</summary>
    private enum State
    {
        Patrol,   // 巡逻：随机方向移动，左右翻转
        Chase,    // 追逐：朝玩家移动，左右翻转
        Pause,    // 蓄力：停顿，面朝玩家左右翻转，播放完整蓄力动画
        Dash,     // 冲刺：快速冲向玩家，360度旋转，碰撞伤害
        Stun      // 硬直：无法行动，显示 Idle 呼吸效果
    }

    /// <summary>当前显示哪个 Spine 动画</summary>
    private enum ActiveSpine
    {
        None,    // 不显示任何 Spine
        Idle,    // 蓄力动画冻结在第一帧 + Update 里做呼吸缩放
        Charge,  // 播放完整蓄力动画
        Dash     // 播放完整冲刺动画
    }

    // ============================================
    // 内部变量
    // ============================================
    private State currentState = State.Patrol;                   // 当前行为状态
    private ActiveSpine currentActiveSpine = ActiveSpine.None;   // 当前显示的 Spine 类型

    private Transform player;          // 玩家 Transform 缓存引用
    private Rigidbody2D rb;            // 刚体组件，用于物理移动
    private float stateTimer;          // 通用状态计时器（蓄力/冲刺/硬直共用）

    private bool hasAggro = false;     // 是否已激活仇恨（一旦发现玩家，永不脱战）
    private float patrolTimer;         // 巡逻方向切换倒计时
    private Vector2 patrolDirection;   // 当前巡逻移动方向

    private bool isKnockedBack = false;  // 外部击退标记（true 时暂停自身移动逻辑）
    private bool isPaused = false;       // 全局暂停标记（对话、过场等）

    // ============================================
    // Unity 生命周期：初始化
    // ============================================
    void Start()
    {
        // --- 获取玩家 Transform ---
        // FixedRoomManager：房间管理器单例，负责管理房间内玩家引用
        GameObject playerObj = FixedRoomManager.Instance.GetPlayer();
        if (playerObj != null)
            player = playerObj.transform;

        // --- 配置 Rigidbody2D ---
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0;        // 无重力（2D 俯视角游戏）
        rb.freezeRotation = true;   // 冻结旋转，防止碰撞导致意外旋转

        // --- 初始化巡逻参数 ---
        patrolTimer = Random.Range(1f, 3f);                   // 1~3 秒后首次切换方向
        patrolDirection = Random.insideUnitCircle.normalized;  // 随机初始方向

        // --- 初始显示 Idle 状态（蓄力第一帧冻结 + 呼吸效果） ---
        SwitchSpine(ActiveSpine.Idle);
    }

    // ============================================
    // Unity 生命周期：每帧更新
    // ============================================
    void Update()
    {
        // === 前置检查：全局暂停 或 外部击退中 → 跳过所有自身逻辑 ===
        if (isPaused || isKnockedBack) return;

        // ============================================
        // 情况1：玩家不存在 或 Tag 不是 "Player" → 强制巡逻
        // （玩家死亡/伪装后 Tag 会改变）
        // ============================================
        if (player == null || !player.CompareTag("Player"))
        {
            hasAggro = false;
            UpdatePatrol();
            return;
        }

        // --- 计算到玩家的距离（用于各种范围判断） ---
        float dist = Vector2.Distance(transform.position, player.position);

        // ============================================
        // 情况2：首次发现玩家进入 chaseRange → 激活仇恨，开始追逐
        // ============================================
        if (!hasAggro && dist <= chaseRange)
        {
            hasAggro = true;
            currentState = State.Chase;
        }

        // ============================================
        // 情况3：还没发现玩家 → 继续巡逻
        // ============================================
        if (!hasAggro)
        {
            UpdatePatrol();
            return;
        }

        // ============================================
        // 情况4：已激活仇恨 → 状态机逻辑
        // ============================================

        // --- 通用计时器递减（蓄力/冲刺/硬直共用） ---
        stateTimer -= Time.deltaTime;

        // --- 状态切换判断 ---
        switch (currentState)
        {
            case State.Chase:
                // 追逐中进入攻击范围 → 开始蓄力
                if (dist <= attackRange)
                    EnterState(State.Pause);
                break;

            case State.Pause:
                // 蓄力计时结束 → 开始冲刺
                if (stateTimer <= 0)
                    EnterState(State.Dash);
                break;

            case State.Dash:
                // 冲刺计时结束 → 进入硬直
                if (stateTimer <= 0)
                    EnterState(State.Stun);
                break;

            case State.Stun:
                // 硬直结束 + 玩家走出攻击范围+缓冲带 → 重新追逐
                if (stateTimer <= 0 && dist > attackRange + triggerBuffer)
                    currentState = State.Chase;
                else if (stateTimer <= 0)
                    rb.velocity = Vector2.zero; // 硬直结束但玩家还在范围内 → 原地等待
                break;
        }

        // --- 执行当前状态的行为 ---
        switch (currentState)
        {
            case State.Chase:
                Chase();
                break;

            case State.Pause:
                // 蓄力期间：原地不动，持续面朝玩家左右翻转
                rb.velocity = Vector2.zero;
                if (player != null)
                {
                    Vector2 dirToPlayer = (player.position - transform.position).normalized;
                    FlipAllSkeletons(dirToPlayer);
                }
                break;

            case State.Dash:
                Dash();
                break;

            case State.Stun:
                // 硬直期间：原地不动
                rb.velocity = Vector2.zero;
                break;
        }

        // ============================================
        // 每帧固定处理
        // ============================================

        // --- 强制子物体位置归零 ---
        // 抵消 Spine 动画可能自带的骨骼位移，确保敌人位置完全由 Rigidbody2D 控制
        if (chargeSkeleton != null)
            chargeSkeleton.transform.localPosition = Vector3.zero;
        if (dashSkeleton != null)
            dashSkeleton.transform.localPosition = Vector3.zero;
    }

    // ============================================
    // 巡逻逻辑
    // ============================================
    /// <summary>
    /// 巡逻：随机方向移动，定时切换方向，左右翻转，显示 Idle 呼吸效果
    /// </summary>
    void UpdatePatrol()
    {
        patrolTimer -= Time.deltaTime;
        if (patrolTimer <= 0)
        {
            patrolDirection = Random.insideUnitCircle.normalized;  // 随机新方向
            patrolTimer = Random.Range(1f, 3f);                    // 重置倒计时
        }
        rb.velocity = patrolDirection * moveSpeed;
        SwitchSpine(ActiveSpine.Idle);        // 显示
        FlipAllSkeletons(patrolDirection);    // 左右翻转（巡逻只要左右）

        // ★ 呼吸缩放
        float breathe = 0.7f + Mathf.Sin(Time.time * breatheSpeed) * breatheAmount;
        transform.localScale = new Vector3(breathe, 0.7f, 0.7f);
    }

    // ============================================
    // 状态进入方法
    // ============================================
    /// <summary>
    /// 进入新状态：切换当前状态 + 设置计时器 + 切换对应 Spine 动画
    /// </summary>
    /// <param name="newState">目标状态</param>
    void EnterState(State newState)
    {
        currentState = newState;

        switch (newState)
        {
            case State.Pause:
                stateTimer = pauseDuration;              // 蓄力时长
                SwitchSpine(ActiveSpine.Charge);          // 播放完整蓄力动画（翻转在 SwitchSpine 里处理）
                break;

            case State.Dash:
                stateTimer = dashDistance / dashSpeed;    // 冲刺时长 = 距离 ÷ 速度
                SwitchSpine(ActiveSpine.Dash);            // 播放完整冲刺动画
                break;

            case State.Stun:
                stateTimer = stunDuration;               // 硬直时长
                SwitchSpine(ActiveSpine.Idle);            // 硬直时显示 Idle 呼吸效果
                break;
        }
    }

    // ============================================
    // 状态行为：追逐
    // ============================================
    /// <summary>
    /// 追逐：朝玩家移动，左右翻转，显示 Idle 呼吸效果
    /// </summary>
    void Chase()
    {
        Vector2 dir = (player.position - transform.position).normalized;
        rb.velocity = dir * moveSpeed;
        SwitchSpine(ActiveSpine.Idle);        // 追逐时显示
        FlipAllSkeletons(dir);                // 左右翻转（追逐只要左右）

        // ★ 呼吸缩放
        float breathe = 0.7f + Mathf.Sin(Time.time * breatheSpeed) * breatheAmount;
        transform.localScale = new Vector3(breathe, 0.7f, 0.7f);
    }

    // ============================================
    // 状态行为：冲刺
    // ============================================
    /// <summary>
    /// 冲刺：快速冲向玩家，360度旋转面朝冲刺方向
    /// </summary>
    void Dash()
    {
        Vector2 dir = (player.position - transform.position).normalized;
        rb.velocity = dir * dashSpeed;

        // ★ 强制重置
        if (dashSkeleton != null)
        {
            dashSkeleton.transform.localScale = Vector3.one;
            dashSkeleton.transform.rotation = Quaternion.identity;
        }

        RotateAllSkeletons(dir);              // 360度旋转（冲刺要任意方向）

        // 安全检查：如果动画被意外清除了，重新显示
        if (currentActiveSpine != ActiveSpine.Dash)
            SwitchSpine(ActiveSpine.Dash);
    }

    // ============================================
    // Spine 动画控制
    // ============================================

    /// <summary>
    /// 切换当前显示的 Spine 并播放对应动画
    /// - Idle：蓄力动画冻结在第一帧 + Update 里做呼吸缩放
    /// - Charge：播放完整蓄力动画，激活时自动面朝玩家翻转
    /// - Dash：播放完整冲刺动画
    /// - None：全部隐藏
    /// </summary>
    /// <param name="target">目标 Spine 类型</param>
    void SwitchSpine(ActiveSpine target)
    {
        // 避免重复切换到同一状态
        if (currentActiveSpine == target) return;
        currentActiveSpine = target;

        // --- 先隐藏所有子物体的 Spine ---
        if (chargeSkeleton != null)
            chargeSkeleton.gameObject.SetActive(false);
        if (dashSkeleton != null)
            dashSkeleton.gameObject.SetActive(false);

        // --- 根据目标显示对应的 Spine ---
        switch (target)
        {
            case ActiveSpine.Idle:
                // 空闲：显示蓄力 Spine，冻结在第一帧，配合 Update 呼吸缩放
                if (chargeSkeleton != null)
                {
                    chargeSkeleton.gameObject.SetActive(true);
                    chargeSkeleton.transform.rotation = Quaternion.identity; // 重置旋转（冲刺可能残留）

                    var track = chargeSkeleton.AnimationState.SetAnimation(0, chargeAnimation, false);
                    track.TrackTime = 0f;       // 跳到第 0 秒（第一帧）
                    track.TimeScale = 0f;        // 冻结时间，动画不播放
                }
                break;

            case ActiveSpine.Charge:
                // 蓄力：播放完整蓄力动画（不循环），激活后立刻面朝玩家翻转
                if (chargeSkeleton != null)
                {
                    chargeSkeleton.gameObject.SetActive(true);
                    chargeSkeleton.transform.rotation = Quaternion.identity; // 重置旋转

                    chargeSkeleton.AnimationState.SetAnimation(0, chargeAnimation, false);
                    // ★ 激活后立刻面朝玩家翻转（用角度判断，覆盖上下左右所有方向）
                    if (player != null)
                    {
                        Vector2 dirToPlayer = (player.position - transform.position).normalized;
                        float angle = Mathf.Atan2(dirToPlayer.y, dirToPlayer.x) * Mathf.Rad2Deg;
                        // 角度在 -90°~90° 之间 = 右半圆（朝右），否则 = 左半圆（朝左）
                        float scaleX = (angle > -90f && angle < 90f) ? 1f : -1f;
                        chargeSkeleton.transform.localScale = new Vector3(scaleX, 1f, 1f);
                    }

                }
                break;

            case ActiveSpine.Dash:
                // 冲刺：播放完整冲刺动画（不循环）
                if (dashSkeleton != null)
                {
                    dashSkeleton.gameObject.SetActive(true);
                    dashSkeleton.AnimationState.SetAnimation(0, dashAnimation, false);
                }
                break;

            case ActiveSpine.None:
                // 不显示任何 Spine
                break;
        }
    }

    /// <summary>
    /// 左右翻转所有 Spine 子物体（巡逻/追逐/蓄力用）
    /// 使用角度判断，覆盖所有方向（包括正上正下）：
    ///   角度在 -90°~90°  → 右半圆 → ScaleX = 1（默认朝右）
    ///   角度在 90°~270° → 左半圆 → ScaleX = -1（镜像朝左）
    /// </summary>
    /// <param name="direction">移动方向向量</param>
    void FlipAllSkeletons(Vector2 direction)
    {
        // 用 Atan2 计算方向角度，-180°~180°
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        // 右半圆（包括正上、正下、右上、右下）→ 正 Scale
        // 左半圆（包括左上、左下）→ 负 Scale（镜像翻转）
        float scaleX = (angle > -90f && angle < 90f) ? 1f : -1f;

        if (chargeSkeleton != null)
            chargeSkeleton.transform.localScale = new Vector3(scaleX, 1f, 1f);
        if (dashSkeleton != null)
            dashSkeleton.transform.localScale = new Vector3(scaleX, 1f, 1f);
    }

    /// <summary>
    /// 360度旋转所有 Spine 子物体（冲刺用）
    /// 使用 Atan2 计算方向角度，让动画面朝任意方向
    /// </summary>
    /// <param name="direction">移动方向向量</param>
    void RotateAllSkeletons(Vector2 direction)
    {
        if (direction == Vector2.zero) return;

        // Atan2(y, x) 返回弧度，乘以 Rad2Deg 转为角度（-180°~180°）
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        // 旋转子物体 Transform（Spine 动画本身不旋转，旋转的是挂载它的容器）
        if (chargeSkeleton != null)
            chargeSkeleton.transform.rotation = Quaternion.Euler(0, 0, angle);
        if (dashSkeleton != null)
            dashSkeleton.transform.rotation = Quaternion.Euler(0, 0, angle);
    }

    // ============================================
    // 碰撞检测：冲刺时碰到玩家造成伤害
    // ============================================
    /// <summary>
    /// 触发器碰撞检测（冲刺碰到玩家受击框）
    /// </summary>
    void OnTriggerEnter2D(Collider2D collision)
    {
        // 条件1：碰撞体标签是 "FireCol"（玩家受击框）
        // 条件2：当前状态必须是冲刺
        if (collision.gameObject.CompareTag("FireCol") && currentState == State.Dash)
        {
            // 从碰撞体父物体获取 Health 组件（FireCol 通常是玩家子物体）
            Health health = collision.transform.parent.GetComponent<Health>();
            if (health != null)
                health.TakeDamage(contactDamage);
        }
    }

    // ============================================
    // IMovable 接口实现（供外部系统控制敌人移动）
    // ============================================

    /// <summary>获取当前移动速度</summary>
    public float GetMoveSpeed() => moveSpeed;

    /// <summary>设置移动速度（如加速/减速 Buff）</summary>
    public void SetMoveSpeed(float speed) { moveSpeed = speed; }

    /// <summary>开始击退：暂停自身移动逻辑，由击退系统接管位移</summary>
    public void StartKnockback() { isKnockedBack = true; }

    /// <summary>结束击退：恢复自身移动逻辑，清除残留速度</summary>
    public void EndKnockback()
    {
        isKnockedBack = false;
        rb.velocity = Vector2.zero;
    }

    /// <summary>全局暂停（对话、过场等）：停止移动</summary>
    public void PauseMovement()
    {
        isPaused = true;
        if (rb == null) rb = GetComponent<Rigidbody2D>();
        if (rb != null) rb.velocity = Vector2.zero;
    }

    /// <summary>恢复移动</summary>
    public void ResumeMovement() { isPaused = false; }

    // ============================================
    // 编辑器可视化：在 Scene 视图绘制范围和朝向
    // ============================================

    /// <summary>选中 GameObject 时绘制线框圆（方便调试范围）</summary>
    void OnDrawGizmosSelected()
    {
        // 追逐范围（发现玩家）- 红色线框
        Gizmos.color = new Color(1f, 0f, 0f, 0.3f);
        Gizmos.DrawWireSphere(transform.position, chaseRange);

        // 攻击范围（触发蓄力）- 黄色线框
        Gizmos.color = new Color(1f, 1f, 0f, 0.3f);
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }

    /// <summary>始终绘制半透明实心圆（方便观察范围）</summary>
    void OnDrawGizmos()
    {
        // 追逐范围 - 黄色半透明
        Gizmos.color = new Color(1f, 1f, 0f, 0.15f);
        Gizmos.DrawSphere(transform.position, chaseRange);

        // 攻击范围 - 红色半透明
        Gizmos.color = new Color(1f, 0f, 0f, 0.15f);
        Gizmos.DrawSphere(transform.position, attackRange);
    }
}