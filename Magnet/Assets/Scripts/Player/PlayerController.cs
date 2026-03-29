using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("移动设置")]
    public float moveSpeed = 8f;                    // 左右移动速度
    public enum MagneticPole { North, South }       // 磁极枚举
    public MagneticPole currentPole = MagneticPole.North;  // 当前玩家磁极
    [Header("墙壁检测")]
    public Transform wallCheck;
    public Vector2 wallCheckSize = new Vector2(0.8f, 1.2f);
    public LayerMask wallLayer;

    private bool touchingLeftWall;
    private bool touchingRightWall;

    [Header("持续磁力设置")]
    public float attractForce = 15f;      // 吸附时的持续拉力
    public float repelForce = 8f;         // 排斥时的持续推力

    [Header("蓄力弹射设置")]
    public float chargeRange = 5f;                   // 蓄力触发范围
    public float maxChargeTime = 2f;                 // 最大蓄力时间（秒）

    [Header("水平弹射力度")]
    public float minHorizontalRepelForce = 10f;      // 水平最小弹射力度
    public float maxHorizontalRepelForce = 50f;      // 水平最大弹射力度

    [Header("垂直弹射力度")]
    public float minVerticalRepelForce = 8f;         // 垂直最小弹射力度
    public float maxVerticalRepelForce = 30f;        // 垂直最大弹射力度

    [Header("吸附蓄力设置")]
    public float maxAttractForce = 25f;              // 最大吸附力度
    public float minAttractForce = 5f;               // 最小吸附力度
    public float minChargeTimeToAttract = 0.2f;  // 最小蓄力时间才能吸附

    private bool isAttracting;                       // 是否正在吸附中

    public bool isOnMagnet = false;        // 是否站在磁铁上
    private Magnet currentMagnetGround;     // 当前站立的磁铁
    private Vector2 attachOffset;           // 相对于磁铁的位置偏移

    [Header("落地设置")]
    public float groundCheckDistance = 0.3f;     // 地面检测距离
    public LayerMask groundLayer;                // 地面层（需要在Inspector中设置）
    private bool wasGrounded;                    // 上一帧是否在地面
    [Header("动画设置")]
    private MagneticPole pendingPole;  // 待切换的磁极
    private bool isSwitchingPole = false;

    private bool isRising = false;      // 是否正在上升
    private bool isFalling = false;     // 是否正在下落

    [Header("相机效果")]
    public Pullaway cameraZoom;


    [Header("死亡重生设置")]
    public Transform startPlace;      //重生点

    [Header("抖动设置")]
    public float force;
    private CinemachineImpulseSource ImpulseSource;


    //角色动画切换
    private Animator animator;
    private SpriteRenderer sprite;

    // 组件引用
    private Rigidbody2D rb;
    private Magnet currentMagnet;                     // 当前在Collider范围内的磁铁
    private Magnet nearestMagnet;                     // 最近且在蓄力范围内的磁铁
    private float distanceToNearest;                  // 到最近磁铁的距离

    // 移动状态
    private int horizontalMove;                       // 移动方向：-1左，0不动，1右
    private int lastDirection;                        // 上一个方向

    // 蓄力相关
    private float currentChargeTime;                  // 当前蓄力时间
    private bool isCharging;                          // 是否正在蓄力

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        sprite = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
        ImpulseSource = GetComponent<CinemachineImpulseSource>();
        // 配置抖动参数
        ImpulseSource.m_ImpulseDefinition.m_AmplitudeGain = force;//强度
    }

    void Update()
    {
        // 落地检测
        bool isGrounded = IsGrounded();

        // 检测落地事件（从空中到地面）
        if (!wasGrounded && isGrounded && !isOnMagnet&&!isCharging)
        {

            // 重置上升/下落状态
            isRising = false;
            isFalling = false;
        }

        // 更新空中动画状态（未在地面且不在磁铁上时）
        if (!isGrounded && !isOnMagnet)
        {
            UpdateAirAnimation();
        }
        // 更新上一帧状态
        wasGrounded = isGrounded;

        // 动画参数更新
        UpdateAnimationParameters(isGrounded);  
        UpdateFacingDirection();

        // 处理移动输入
        HandleMovementInput();

        // 处理磁极切换
        if (Input.GetKeyDown(KeyCode.LeftShift)|| Input.GetKeyDown(KeyCode.RightShift))
        {
            isSwitchingPole = true;

            // 计算要切换到的目标磁极
            pendingPole = (currentPole == MagneticPole.North) ? MagneticPole.South : MagneticPole.North;

            // 触发动画
            animator.SetTrigger("J");

            // 注 不立即调用 SwitchPole()
        }

        if (!isOnMagnet)
        {
            // 更新最近的磁铁（用于蓄力）
            UpdateNearestMagnet();
            // 处理蓄力弹射系统
            HandleChargeSystem();
        }
    }

    void FixedUpdate()
    {// 检测碰撞
        Collider2D[] hits = Physics2D.OverlapBoxAll(wallCheck.position, wallCheckSize, 0, wallLayer);

        touchingLeftWall = false;
        touchingRightWall = false;

        foreach (var hit in hits)
        {
            // 计算碰撞点相对于玩家的位置
            Vector2 hitPoint = hit.ClosestPoint(transform.position);
            float xDiff = hitPoint.x - transform.position.x;

            if (xDiff < -0.1f)  // 在左边
                touchingLeftWall = true;
            else if (xDiff > 0.1f)  // 在右边
                touchingRightWall = true;
        }
        if (isCharging) return;

        // 如在磁铁上，只维持位置，不应用移动和磁力
        if (isOnMagnet)
        {
            ApplyMagnetMovement();
            MaintainAttachPosition();
            return;
        }

        // 应用移动
        ApplyMovement();
        // 应用持续的磁力
        ApplyContinuousMagneticForce();

        //吸附过程中的拉力
        if (isAttracting && nearestMagnet != null)
        {
            Vector2 direction = (nearestMagnet.transform.position - transform.position).normalized;
            rb.AddForce(direction * maxAttractForce, ForceMode2D.Force);
        }
    }

    /// 检测是否在地面
    bool IsGrounded()
    {
        // 从角色脚下向下发射
        return Physics2D.Raycast(transform.position, Vector2.down, groundCheckDistance, groundLayer); ;
    }

    /// 进入磁铁范围
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Magnet"))
        {
            currentMagnet = other.GetComponent<Magnet>();
        }
        if (other.CompareTag("JIANCI"))
        { 
            isOnMagnet = false;
            ImpulseSource.GenerateImpulse();
            animator.SetTrigger("ReStart");
            
        }
    }

    /// 离开磁铁范围
    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Magnet"))
        {
            currentMagnet = null;
        }
    }

    /// 更新最近的磁铁（用于蓄力判断，范围比Collider大）
    void UpdateNearestMagnet()
    {
        // 查找所有磁铁
        Magnet[] magnets = FindObjectsOfType<Magnet>();
        Magnet closest = null;
        float closestDist = float.MaxValue;

        foreach (Magnet magnet in magnets)
        {
            float distance = Vector2.Distance(transform.position, magnet.transform.position);

            // 检查磁极是否相吸（只有异极才能吸附）
            bool isAttract = (currentPole == MagneticPole.North && magnet.pole == MagneticPole.South) ||
                             (currentPole == MagneticPole.South && magnet.pole == MagneticPole.North);

            // 只考虑在蓄力范围内且磁极相吸的磁铁
            if (distance <= chargeRange && isAttract && distance < closestDist)
            {
                closestDist = distance;
                closest = magnet;
            }
        }

        nearestMagnet = closest;
        distanceToNearest = closestDist;
    }

    /// 应用磁力
    void ApplyContinuousMagneticForce()
    {
        if (currentMagnet == null) return;

        // 判断相吸相斥
        bool isAttract = (currentPole == MagneticPole.North && currentMagnet.pole == MagneticPole.South) ||
                         (currentPole == MagneticPole.South && currentMagnet.pole == MagneticPole.North);

        // 如排斥，不进入附着状态
        if (!isAttract)
        {
            if (isOnMagnet) DetachFromMagnet();

            // 排斥逻辑
            Vector2 direction = (currentMagnet.transform.position - transform.position).normalized;
            float distance = Vector2.Distance(transform.position, currentMagnet.transform.position);
            float distanceFactor = 1 - Mathf.Clamp01(distance / 3f);
            distanceFactor = Mathf.Lerp(0.3f, 1f, distanceFactor);
            rb.AddForce(-direction * repelForce * distanceFactor, ForceMode2D.Force);
            return;
        }

        // 相吸逻辑
        float distanceToMagnet = Vector2.Distance(transform.position, currentMagnet.transform.position);
        float attachDistance = 0.5f;

        if (!isOnMagnet && distanceToMagnet <= attachDistance)
        {
            // 足够近，附着到磁铁
            AttachToMagnet(currentMagnet);
        }
        else
        {
            // 还没到附着距离，施加拉力
            Vector2 direction = (currentMagnet.transform.position - transform.position).normalized;
            float distanceFactor = 1 - Mathf.Clamp01(distanceToMagnet / 3f);
            distanceFactor = Mathf.Lerp(0.3f, 1f, distanceFactor);
            rb.AddForce(direction * attractForce * distanceFactor, ForceMode2D.Force);
        }
    }

    /// 处理蓄力弹射系统
    void HandleChargeSystem()
    {
        // 弹射：在Collider范围内且互斥（优先）
        bool canRepel = false;
        if (currentMagnet != null)
        {
            canRepel = (currentPole == MagneticPole.North && currentMagnet.pole == MagneticPole.North) ||
                       (currentPole == MagneticPole.South && currentMagnet.pole == MagneticPole.South);
        }

        // 吸附蓄力：只有在不能弹射的情况下，才检查远距离吸附
        bool canAttract = false;
        if (!canRepel && nearestMagnet != null)  // 只有不能弹射时才检查吸附
        {
            canAttract = (currentPole == MagneticPole.North && nearestMagnet.pole == MagneticPole.South) ||
                         (currentPole == MagneticPole.South && nearestMagnet.pole == MagneticPole.North);
        }

        bool isHoldingSpace = Input.GetKey(KeyCode.Space);

        if ((canRepel || canAttract) && isHoldingSpace && !isAttracting)
        {
            if (!isCharging) StartCharging();
            ContinueCharging();
        }
        else if (isCharging && !isHoldingSpace)
        {
            if (canRepel) ReleaseRepel();      // 弹射优先
            else if (canAttract) ReleaseAttract();  // 吸附
            CancelCharging();
        }
        else if (isCharging && !canRepel && !canAttract)
        {
            CancelCharging();
        }
    }

    /// 开始蓄力
    void StartCharging()
    {
        isCharging = true;
        currentChargeTime = 0;
        // 开始持续抖动
        if (currentMagnet != null)
        {
            Magnet shake = currentMagnet.GetComponentInChildren<Magnet>();
            if (shake != null) shake.StartContinuousShake(0.04f);
        }
    }

    /// 持续蓄力
    void ContinueCharging()
    {
        currentChargeTime += Time.deltaTime;
        if (currentChargeTime > maxChargeTime)
        {
            currentChargeTime = maxChargeTime;
        }
        //镜头拉远
        //先计算蓄力百分比
        float chargePercent = currentChargeTime / maxChargeTime;
        cameraZoom.UpdateCharge(chargePercent);
        // 蓄力期间持续停住，不让移动
        rb.velocity = Vector2.zero;
    }

    /// 释放弹射
    void ReleaseRepel()
    {
        if (!isCharging || currentMagnet == null) return;
        // 停止持续抖动
        if (currentMagnet != null)
        {
            Magnet shake = currentMagnet.GetComponentInChildren<Magnet>();
            if (shake != null) shake.StopContinuousShake();
        }

        // 根据蓄力时间计算弹射力度
        float chargePercent = currentChargeTime / maxChargeTime;
        // 计算方向：远离磁铁
        Vector2 direction = (transform.position - currentMagnet.transform.position).normalized;

        // 根据方向分别计算力度
        float repelForce;
        if (Mathf.Abs(direction.x) > Mathf.Abs(direction.y))
        {
            // 水平弹射 - 直接改坐标
            float distance = Mathf.Lerp(minHorizontalRepelForce, maxHorizontalRepelForce, chargePercent);
            transform.position += new Vector3(direction.x * distance, 0.5f, 0);
            //rb.velocity = new Vector2(0, rb.velocity.y);
        }
        else
        {
            animator.SetTrigger("Realse");
            // 垂直方向为主（上下弹射）
            repelForce = Mathf.Lerp(minVerticalRepelForce, maxVerticalRepelForce, chargePercent);
        // 弹射方向：远离磁铁
        Vector2 finalForce = direction * repelForce;

        StartCoroutine(LaunchCoroutine(finalForce));
        }

        //镜头恢复
        cameraZoom.ResetZoom();
        // 重置蓄力状态
        CancelCharging();
    }

    /// 弹射协程
    IEnumerator LaunchCoroutine(Vector2 force)
    {

        // 清除原有速度，应用弹射力
        rb.AddForce(force, ForceMode2D.Impulse);

        yield return null;

    }
    /// 释放吸附
    void ReleaseAttract()
    {
        if (!isCharging || nearestMagnet == null) return;

        // 计算到磁铁的距离
        float distanceToMagnet = Vector2.Distance(transform.position, nearestMagnet.transform.position);

        // 根据距离计算所需的最小蓄力时间（距离越远，需要时间越长）

        float requiredChargeTime = Mathf.Lerp(
            minChargeTimeToAttract,                    // 距离=0时需要的时间
            maxChargeTime,                              // 距离=chargeRange时需要的时间
            Mathf.Clamp01(distanceToNearest / chargeRange)
        );

        // 检查是否达到所需的最小蓄力时间
        if (currentChargeTime < requiredChargeTime)
        {
            CancelCharging();
            return;
        }

        // 根据蓄力时间计算吸附力度
        float chargePercent = currentChargeTime / maxChargeTime;
        float attractForce = Mathf.Lerp(minAttractForce, maxAttractForce, chargePercent);
    
    
        // 吸附 像弹射
        StartCoroutine(SmoothAttractCoroutine(nearestMagnet, attractForce));
    }


    /// 取消蓄力
    void CancelCharging()
    {
        isCharging = false;
        currentChargeTime = 0;

        // 停止持续抖动
        if (currentMagnet != null)
        {
            Magnet shake = currentMagnet.GetComponentInChildren<Magnet>();
            if (shake != null) shake.StopContinuousShake();
        }

        // 恢复相机视角
        if (cameraZoom != null)
        {
            cameraZoom.ResetZoom();
        }
    }


    /// 处理移动输入
    void HandleMovementInput()
    {
        bool aPressed = Input.GetKey(KeyCode.A);
        bool dPressed = Input.GetKey(KeyCode.D);

        if (aPressed && dPressed)
        {
            horizontalMove = lastDirection;
        }
        else if (aPressed)
        {
            horizontalMove = -1;
            lastDirection = -1;
        }
        else if (dPressed)
        {
            horizontalMove = 1;
            lastDirection = 1;
        }
        else
        {
            horizontalMove = 0;
        }
    }

    /// 应用水平移动
    void ApplyMovement()
    {
        if (!isOnMagnet && !isCharging)
        {
            Vector2 velocity = rb.velocity;
            // 根据左右墙壁限制移动
            if ((horizontalMove == -1 && touchingLeftWall) ||
                (horizontalMove == 1 && touchingRightWall))
            {
                velocity.x = 0;
            }
            else
            {
                velocity.x = horizontalMove * moveSpeed;
            }
            rb.velocity = velocity;
        }
    }

    void AttachToMagnet(Magnet magnet)
    {
        isOnMagnet = true;
        currentMagnetGround = magnet;

        // 计算相对于磁铁的局部坐标（会随磁铁旋转/移动）
        attachOffset = magnet.transform.InverseTransformPoint(transform.position);
        
        // 如果是荡绳磁铁，触发摆动
        SwingMagnet swing = magnet.GetComponent<SwingMagnet>();
        if (swing != null)
        {
            swing.AttachPlayer(this);
        }
    }

    //松开吸附
    void DetachFromMagnet()
    {
        if (!isOnMagnet) return;

        //// 让荡绳磁铁停止接收玩家输入
        if (currentMagnetGround != null)
        {
            SwingMagnet swing = currentMagnetGround.GetComponent<SwingMagnet>();
            if (swing != null)
            {
                swing.DetachPlayer();
            }
        }

        isOnMagnet = false;
        currentMagnetGround = null;
    }



    void MaintainAttachPosition()
    {
        if (currentMagnetGround == null) return;

        // 使用局部坐标转世界坐标，自动跟随磁铁的移动和旋转
        Vector2 targetPosition = currentMagnetGround.transform.TransformPoint(attachOffset);
        rb.MovePosition(targetPosition);

        // 保持速度为0
        rb.velocity = Vector2.zero;
    }
    //控制吸附时候状态
    void ApplyMagnetMovement()
    {
        if (currentMagnetGround == null) return;

        if (horizontalMove != 0)
        {
            // 1. 获取磁铁当前的右方向（局部X轴在世界中的方向）
            Vector2 magnetRight = currentMagnetGround.transform.right;

            // 2. 获取屏幕向右的方向（世界X轴正方向）
            Vector2 worldRight = Vector2.right;

            // 3. 计算玩家输入方向与磁铁右方向的点积，判断是否需要反转
            //    如果磁铁右方向指向屏幕左边（点积为负），则需要反转输入
            float alignment = Vector2.Dot(magnetRight, worldRight);

            // 4. 确定实际移动方向
            int effectiveMove = horizontalMove;

            // 如果磁铁方向与屏幕方向相反，反转控制
            if (alignment < 0)
            {
                effectiveMove = -horizontalMove;
            }

            // 5. 计算移动距离（局部坐标系下）
            float halfLength = GetSurfaceHalfLength();
            float localX = attachOffset.x;
            float edgeDistance = halfLength - Mathf.Abs(localX);

            // 边缘减速
            float speedMultiplier = 1f;
            float edgeStart = halfLength * 0.7f;
            if (edgeDistance < edgeStart)
            {
                float t = 1f - (edgeDistance / edgeStart);
                speedMultiplier = Mathf.Lerp(1f, 0.2f, t);
            }

            float actualSpeed = moveSpeed * speedMultiplier;
            float deltaMove = effectiveMove * actualSpeed * Time.fixedDeltaTime;

            // 6. 更新局部偏移
            attachOffset.x += deltaMove;
            attachOffset.x = Mathf.Clamp(attachOffset.x, -halfLength, halfLength);
        }
    }

    float GetSurfaceHalfLength()
    {
        Collider2D collider = currentMagnetGround.GetComponent<Collider2D>();

        if (collider is BoxCollider2D box)
            return box.size.x / 2f;
        if (collider is CircleCollider2D circle)
            return circle.radius;

        return 1f;  // 默认
    }

    /// 切换磁极
    void SwitchPole()
    {
        currentPole = (currentPole == MagneticPole.North) ? MagneticPole.South : MagneticPole.North;
    }

    //动画
    void UpdateAnimationParameters(bool isGrounded)
    {
        // 水平移动速度（取绝对值）
        float horizontalSpeed = Mathf.Abs(rb.velocity.x);
        animator.SetFloat("Speed", horizontalSpeed);

        // 是否在地面。
        animator.SetBool("IsGrounded", isGrounded);
    }

    /// 更新空中动画（根据垂直速度判断上升或下落）
    void UpdateAirAnimation()
    {
        // 获取垂直速度
        float verticalVelocity = rb.velocity.y;

        // 判断是上升还是下落（加一个小阈值避免抖动）
        if (verticalVelocity > 0.5f)  // 上升
        {
            if (!isRising)
            {
                isRising = true;
                isFalling = false;
            }
        }
        else if (verticalVelocity < -0.5f)  // 下落
        {
            if (!isFalling)
            {
                isFalling = true;
                isRising = false;

                // 播放下落动画
                animator.SetTrigger("Fall");
            }
        }
    }
    //转向
    void UpdateFacingDirection()
    {
        if (horizontalMove != 0)
        {
            // horizontalMove = -1 向左，1 向右
            sprite.flipX = horizontalMove < 0;
        }
    }

    //吸附时候飞过去操作
    IEnumerator SmoothAttractCoroutine(Magnet targetMagnet, float force)
    {
        isAttracting = true;

        Vector2 startPos = transform.position;
        Vector2 targetPos = targetMagnet.transform.position;
        float distance = Vector2.Distance(startPos, targetPos);

        // 根据距离和力度计算吸附时间（力度越大越快）
        float attractTime = Mathf.Clamp(distance / force, 0.1f, 0.5f);
        float elapsedTime = 0f;

        // 记录上一帧位置，用于检测是否被卡住
        Vector2 lastPos = startPos;
        float stuckTime = 0f;

        while (elapsedTime < attractTime)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / attractTime;

            // 缓动：先快后慢，更自然
            t = 1 - Mathf.Pow(1 - t, 2);

            Vector2 currentPos = Vector2.Lerp(startPos, targetPos, t);
            // 平滑移动
            rb.MovePosition(currentPos);
            rb.velocity = Vector2.zero;  // 保持速度为零，避免碰撞

            // 检测是否被卡住（移动距离过小）
            float movedDistance = Vector2.Distance(currentPos, lastPos);
            if (movedDistance < 0.05f)
            {
                stuckTime += Time.deltaTime;
                if (stuckTime > 0.1f)  // 被卡住超过0.1秒
                {
                    // 被障碍物挡住，取消吸附
                    Debug.Log("吸附被障碍物阻挡");
                    isAttracting = false;
                    yield break;  // 退出协程，不吸附
                }
            }
            else
            {
                stuckTime = 0f;
            }
            lastPos = currentPos;
            yield return null;
        }

        // 最终检查是否真的到达
        float finalDistance = Vector2.Distance(transform.position, targetMagnet.transform.position);
        if (finalDistance <= 0.5f)
        {
            rb.MovePosition(targetPos);
            rb.velocity = Vector2.zero;
            AttachToMagnet(targetMagnet);
        }

        isAttracting = false;
    }

    

    public void ReStart()
    {
        transform.position = startPlace.position;
    }

    public void OnPoleSwitchAnimationEvent()
    {
        // 实际切换磁极
        currentPole = pendingPole;

        // 处理磁铁上的脱离逻辑
        if (isOnMagnet && currentMagnetGround != null)
        {
            bool isNowAttract = (currentPole == MagneticPole.North && currentMagnetGround.pole == MagneticPole.South) ||
                                (currentPole == MagneticPole.South && currentMagnetGround.pole == MagneticPole.North);
            if (!isNowAttract)
            {
                DetachFromMagnet();
            }
        }

        isSwitchingPole = false;
    }
}