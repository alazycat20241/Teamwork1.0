using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaserBeam : MonoBehaviour
{
    [Header("激光参数")]
    [SerializeField] private LayerMask targetLayer;         // 要伤害的层（敌人）
    [SerializeField] private LayerMask obstacleLayer;       // 阻挡层（墙壁等）
    [SerializeField] private float maxLength = 10f;         // 激光最大长度
    [SerializeField] private float damagePerSecond = 30f;   // 每秒伤害
    [SerializeField] private Transform firePoint;           // 发射点（玩家身上的空物体）

    [Header("视觉")]
    [SerializeField] private LineRenderer lineRenderer;     // 画线的组件
    [SerializeField] private float fadeOutDuration = 0.15f; // 松手后消失动画的时长

    private Vector2 currentEndPoint;        // 激光当前终点
    private bool isFiring = false;          // 是否正在发射
    private Coroutine fadeOutCoroutine;     // 消失动画协程引用
    private float originalWidth;            // 激光原始宽度（用于恢复）

    void Start()
    {
        // 没指定发射点就用自身位置
        if (firePoint == null) firePoint = transform;

        // 记录初始宽度，消失动画后恢复用
        originalWidth = lineRenderer.widthMultiplier;

        // 初始状态隐藏激光
        lineRenderer.enabled = false;
    }

    void Update()
    {
        // 按住鼠标右键 → 发射激光
        if (Input.GetMouseButton(1))
        {
            // 如果正在播放消失动画，中断它
            if (fadeOutCoroutine != null)
            {
                StopCoroutine(fadeOutCoroutine);
                fadeOutCoroutine = null;
                lineRenderer.widthMultiplier = originalWidth;  // 恢复宽度
            }

            isFiring = true;
            lineRenderer.enabled = true;    // 显示激光
            UpdateLaser();                  // 更新激光位置和长度
            ApplyDamage();                  // 对线上敌人造成伤害
        }
        // 松手 → 开始消失动画
        else if (isFiring)
        {
            isFiring = false;
            if (fadeOutCoroutine == null)
            {
                fadeOutCoroutine = StartCoroutine(FadeOut());
            }
        }
    }

    /// <summary>
    /// 更新激光的位置、方向和长度（碰到障碍物会截断）
    /// </summary>
    void UpdateLaser()
    {
        // 获取鼠标在世界空间的方向
        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 direction = (mousePos - (Vector2)firePoint.position).normalized;

        // 射线检测：看激光前方有没有障碍物
        RaycastHit2D hit = Physics2D.Raycast(firePoint.position, direction, maxLength, obstacleLayer);

        // 碰到障碍物就缩短到碰撞点，否则用最大长度
        currentEndPoint = hit.collider != null
            ? hit.point
            : (Vector2)firePoint.position + direction * maxLength;

        // 更新 LineRenderer 的起点和终点
        lineRenderer.SetPosition(0, firePoint.position);
        lineRenderer.SetPosition(1, currentEndPoint);
    }

    /// <summary>
    /// 对激光线段上的所有敌人造成持续伤害
    /// </summary>
    void ApplyDamage()
    {
        Vector2 direction = (currentEndPoint - (Vector2)firePoint.position).normalized;
        float length = Vector2.Distance(firePoint.position, currentEndPoint);

        // 检测光束扫过的所有碰撞体
        RaycastHit2D[] hits = Physics2D.RaycastAll(firePoint.position, direction, length, targetLayer);

        // 用HashSet记录本帧已经伤害过的Health组件，避免重复扣血
        HashSet<Health> damagedThisFrame = new HashSet<Health>();

        foreach (var hit in hits)
        {
            Health health = hit.collider.GetComponent<Health>();

            // 如果health存在且本帧还没被伤害过
            if (health != null && !damagedThisFrame.Contains(health))
            {
                health.TakeDamage(damagePerSecond * Time.deltaTime);
                Debug.Log($"伤害: {damagePerSecond * Time.deltaTime}, 敌人剩余血量: {health.CurrentHealth}");
                damagedThisFrame.Add(health);  // 标记已伤害
            }
        }
    }

    /// <summary>
    /// 消失动画：激光宽度逐渐减小到0，然后隐藏
    /// </summary>
    IEnumerator FadeOut()
    {
        float elapsed = 0f;
        float startWidth = lineRenderer.widthMultiplier;

        // 在 fadeOutDuration 秒内，宽度从当前值平滑过渡到 0
        while (elapsed < fadeOutDuration)
        {
            elapsed += Time.deltaTime;
            lineRenderer.widthMultiplier = Mathf.Lerp(startWidth, 0f, elapsed / fadeOutDuration);
            yield return null;  // 等待下一帧
        }

        // 恢复宽度、隐藏激光、清空协程引用
        lineRenderer.widthMultiplier = originalWidth;
        lineRenderer.enabled = false;
        fadeOutCoroutine = null;
    }
}
