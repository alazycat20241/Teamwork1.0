using System.Collections;
using System.Collections.Generic;
using UnityEditor.Rendering.LookDev;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("移动设置")]
    public float moveSpeed = 8f;                    // 左右移动速度
    public enum MagneticPole { North, South }       // 磁极枚举
    public MagneticPole currentPole = MagneticPole.North;  // 当前玩家磁极

    public bool canMove = true;

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
    }

    void Update()
    {
        if (isLaunching) return;  // 弹射中不能操作

        // 处理移动输入
        HandleMovementInput();

        // 处理磁极切换
        if (Input.GetKeyDown(KeyCode.K))
        {
            SwitchPole();
        }

        // 更新最近的磁铁（用于蓄力）
        UpdateNearestMagnet();

        // 处理蓄力弹射系统
        HandleChargeSystem();
    }

    void FixedUpdate()
    {
        if (isLaunching) return;
        if (isCharging)return;
        // 应用移动
        if (canMove)
        {
            ApplyMovement();
        }

        // 应用持续的磁力（在Collider范围内时）
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

    /// 应用持续的磁力（只在Collider范围内生效）
    void ApplyContinuousMagneticForce()
    {
        if (!canMove) return;
        if (currentMagnet == null) return;

        // 判断是相吸还是相斥
        bool isAttract = (currentPole == MagneticPole.North && currentMagnet.pole == MagneticPole.South) ||
                         (currentPole == MagneticPole.South && currentMagnet.pole == MagneticPole.North);

        // 计算方向（从玩家指向磁铁）
        Vector2 direction = (currentMagnet.transform.position - transform.position).normalized;

        if (isAttract)
        {
            // 相吸：持续被拉向磁铁
            rb.AddForce(direction * attractForce, ForceMode2D.Force);
        }
        else
        {
            // 相斥：持续被推开
            rb.AddForce(-direction * repelForce, ForceMode2D.Force);
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

        // 蓄力期间持续停住，不让移动
        rb.velocity = Vector2.zero;
    }

    /// 释放弹射
    void ReleaseRepel()
    {
        if (!isCharging || currentMagnet == null) return;  

        // 根据蓄力时间计算弹射力度
        float chargePercent = currentChargeTime / maxChargeTime;
        float repelForce = Mathf.Lerp(minRepelForce, maxRepelForce, chargePercent);

        // 弹射方向：远离磁铁
        Vector2 direction = (transform.position - currentMagnet.transform.position).normalized;

        // 添加向上的分量，让弹射更自然
        direction = (direction + Vector2.up * 0.5f).normalized;

        StartCoroutine(LaunchCoroutine(direction * repelForce));

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
    StartCoroutine(LaunchCoroutine(direction * attractForce));
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
        Vector2 velocity = rb.velocity;
        velocity.x = horizontalMove * moveSpeed;
        rb.velocity = velocity;
    }


    /// 可动
    public void SetCanMove(bool canMove)
    {
        this.canMove = canMove;
    }
    /// 切换磁极
    void SwitchPole()
    {
        currentPole = (currentPole == MagneticPole.North) ? MagneticPole.South : MagneticPole.North;
    }
}
