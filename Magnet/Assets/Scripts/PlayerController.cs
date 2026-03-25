using System.Collections;
using System.Collections.Generic;
using UnityEditor.Rendering.LookDev;
using UnityEngine;
using UnityEngine.U2D;

public class PlayerController : MonoBehaviour
{
    [Header("移动设置")]
    public float moveSpeed = 8f;                    // 左右移动速度
    public enum MagneticPole { North, South }       // 磁极枚举
    public MagneticPole currentPole = MagneticPole.North;  // 当前玩家磁极

    [Header("持续磁力设置")]
    public float attractForce = 15f;      // 吸附时的持续拉力
    public float repelForce = 8f;         // 排斥时的持续推力

    [Header("蓄力弹射设置")]
    public float chargeRange = 5f;                   // 蓄力触发范围
    public float maxChargeTime = 2f;                 // 最大蓄力时间（秒）
    public float maxRepelForce = 30f;                // 最大弹射力度
    public float minRepelForce = 8f;                 // 最小弹射力度

    [Header("吸附蓄力设置")]
    public float maxAttractForce = 25f;              // 最大吸附力度
    public float minAttractForce = 5f;               // 最小吸附力度

    private bool isAttracting;                       // 是否正在吸附中

    public bool isOnMagnet = false;        // 是否站在磁铁上
    private Magnet currentMagnetGround;     // 当前站立的磁铁
    private Vector2 attachOffset;           // 相对于磁铁的位置偏移

    [Header("相机效果")]
    public Pullaway cameraZoom;

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
    private bool isLaunching;                         // 是否正在弹射中

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        sprite = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        //动画
        UpdateAnimationParameters();
        UpdateFacingDirection();
        if (isLaunching) return;  // 弹射中不能操作

        // 处理移动输入
        HandleMovementInput();

        // 处理磁极切换
        if (Input.GetKeyDown(KeyCode.J))
        {
            animator.SetTrigger("J");
            SwitchPole();
            // 如果在磁铁上且切换后变排斥，就脱离
            if (isOnMagnet && currentMagnetGround != null)
            {
                bool isNowAttract = (currentPole == MagneticPole.North && currentMagnetGround.pole == MagneticPole.South) ||
                                    (currentPole == MagneticPole.South && currentMagnetGround.pole == MagneticPole.North);
                if (!isNowAttract)
                {
                    DetachFromMagnet();
                }
            }
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
    {
        if (isLaunching||isCharging) return;

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

    /// 进入磁铁范围
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Magnet"))
        {
            currentMagnet = other.GetComponent<Magnet>();
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

            // 只考虑在蓄力范围内的磁铁
            if (distance <= chargeRange && distance < closestDist)
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

        if ((canRepel || canAttract) && isHoldingSpace && !isLaunching && !isAttracting)
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
        // 停住！清除速度
        rb.velocity = Vector2.zero;
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
        animator.SetTrigger("Realse");
        // 根据蓄力时间计算弹射力度
        float chargePercent = currentChargeTime / maxChargeTime;
        float repelForce = Mathf.Lerp(minRepelForce, maxRepelForce, chargePercent);

        // 弹射方向：远离磁铁
        Vector2 direction = (transform.position - currentMagnet.transform.position).normalized;

        // 添加向上的分量，让弹射更自然
        direction = (direction + Vector2.up * 0.5f).normalized;

        StartCoroutine(LaunchCoroutine(direction * repelForce));
        //镜头恢复
        cameraZoom.ResetZoom();
        // 重置蓄力状态
        CancelCharging();
    }

    /// 弹射协程
    IEnumerator LaunchCoroutine(Vector2 force)
    {
        isLaunching = true;

        // 清除原有速度，应用弹射力
        rb.velocity = Vector2.zero;
        rb.AddForce(force, ForceMode2D.Impulse);

        // 等待弹射完成
        float waitTime = Mathf.Clamp(force.magnitude / 50f, 0.2f, 0.8f);
        yield return new WaitForSeconds(waitTime);

        isLaunching = false;
    }
    /// 释放吸附
    void ReleaseAttract()
    {
        if (!isCharging || nearestMagnet == null) return;
    
    // 根据蓄力时间计算吸附力度
    float chargePercent = currentChargeTime / maxChargeTime;
    float attractForce = Mathf.Lerp(minAttractForce, maxAttractForce, chargePercent);
    
    // 计算方向：指向磁铁
    Vector2 direction = (nearestMagnet.transform.position - transform.position).normalized;
    
    // 瞬间吸附！像弹射一样
    StartCoroutine(SmoothAttractCoroutine(nearestMagnet, attractForce));
    }


    /// 取消蓄力
    void CancelCharging()
    {
        isCharging = false;
        currentChargeTime = 0;
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
        if (!isOnMagnet)
        {
            Vector2 velocity = rb.velocity;
            velocity.x = horizontalMove * moveSpeed;
            rb.velocity = velocity;
        }
    }

    void AttachToMagnet(Magnet magnet)
    {
        isOnMagnet = true;
        currentMagnetGround = magnet;

        // 计算相对于磁铁的局部坐标（会随磁铁旋转/移动）
        attachOffset = magnet.transform.InverseTransformPoint(transform.position);

        // 禁用物理影响
        //rb.gravityScale = 0;
        //rb.velocity = Vector2.zero;
        //rb.isKinematic = true;  // 变成运动学刚体，完全由代码控制位置
        // 如果是荡绳磁铁，触发摆动
        SwingMagnet swing = magnet.GetComponent<SwingMagnet>();
        if (swing != null)
        {
            swing.AttachPlayer(this);
        }
    }

    void DetachFromMagnet()
    {
        if (!isOnMagnet) return;

        // 让荡绳磁铁停止接收玩家输入
        if (currentMagnetGround != null)
        {
            SwingMagnet swing = currentMagnetGround.GetComponent<SwingMagnet>();
            if (swing != null)
            {
                swing.DetachPlayer();
            }
        }

        isOnMagnet = false;
        //rb.isKinematic = false;
        //rb.gravityScale = 5;
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
    void ApplyMagnetMovement()
    {
        if (currentMagnetGround == null) return;

        // 获取磁铁半长
        float halfLength = GetSurfaceHalfLength();
        float localX = attachOffset.x;
        float edgeDistance = halfLength - Mathf.Abs(localX);

        // 计算速度系数（边缘减速）
        float speedMultiplier = 1f;
        float edgeStart = halfLength * 0.7f;

        if (edgeDistance < edgeStart && horizontalMove != 0)
        {
            float t = 1f - (edgeDistance / edgeStart);
            speedMultiplier = Mathf.Lerp(1f, 0.2f, t);
        }

        // 移动
        float actualSpeed = moveSpeed * speedMultiplier;
        attachOffset.x += horizontalMove * actualSpeed * Time.fixedDeltaTime;

        // 限制在磁铁范围内
        attachOffset.x = Mathf.Clamp(attachOffset.x, -halfLength, halfLength);
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

    // 其他脚本可以获取玩家的移动输入
    public int GetHorizontalMove()
    {
        return horizontalMove;
    }

    /// 切换磁极
    void SwitchPole()
    {
        currentPole = (currentPole == MagneticPole.North) ? MagneticPole.South : MagneticPole.North;
    }

    //动画
    void UpdateAnimationParameters()
    {
        // 水平移动速度（取绝对值）
        float horizontalSpeed = Mathf.Abs(rb.velocity.x);
        animator.SetFloat("Speed", horizontalSpeed);
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

    IEnumerator SmoothAttractCoroutine(Magnet targetMagnet, float force)
    {
        isLaunching = true;  // 禁用操作
        isAttracting = true;

        Vector2 startPos = transform.position;
        Vector2 targetPos = targetMagnet.transform.position;
        float distance = Vector2.Distance(startPos, targetPos);

        // 根据距离和力度计算吸附时间（力度越大越快）
        float attractTime = Mathf.Clamp(distance / force, 0.1f, 0.5f);
        float elapsedTime = 0f;

        while (elapsedTime < attractTime)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / attractTime;

            // 缓动：先快后慢，更自然
            t = 1 - Mathf.Pow(1 - t, 2);

            // 平滑移动
            rb.MovePosition(Vector2.Lerp(startPos, targetPos, t));
            rb.velocity = Vector2.zero;  // 保持速度为零，避免碰撞

            yield return null;
        }

        // 确保到达精确位置
        rb.MovePosition(targetPos);
        rb.velocity = Vector2.zero;

        // 吸附完成，附着到磁铁
        AttachToMagnet(targetMagnet);

        isLaunching = false;
        isAttracting = false;
    }
}