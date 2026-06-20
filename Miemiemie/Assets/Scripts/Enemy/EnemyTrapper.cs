using UnityEngine;
using Spine;
using Spine.Unity;
using System.Collections;

/// <summary>
/// 陷阱型敌人
/// 远离玩家，定期在脚下放置陷阱
/// Spine 动画：walk（循环），attack（放陷阱时播一次）
/// </summary>
public class EnemyTrapper : MonoBehaviour, IMovable
{
    [Header("索敌参数")]
    [SerializeField] private float detectRange = 8f;
    [SerializeField] private float fleeDistance = 5f;
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float bufferZone = 0.5f;

    [Header("陷阱")]
    [SerializeField] private GameObject trapPrefab;
    [SerializeField] private float trapInterval = 7f;

    [Header("Spine 动画")]
    [SerializeField] private SkeletonAnimation skeletonAnimation;
    [SpineAnimation]
    [SerializeField] private string walkAnimation = "walk";    // 走路动画
    [SpineAnimation]
    [SerializeField] private string attackAnimation = "attack"; // 放陷阱动画

    private Transform player;
    private Rigidbody2D rb;
    private float trapTimer;

    private bool hasAggro = false;
    private float patrolTimer;
    private Vector2 patrolDirection;

    private bool isKnockedBack = false;
    private bool isPaused = false;

    private string currentAnim;  // 当前动画名，避免重复播放

    private bool isPlacingTrap;//正在放陷阱

    void Start()
    {
        GameObject playerObj = FixedRoomManager.Instance.GetPlayer();
        if (playerObj != null)
            player = playerObj.transform;

        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0;
        rb.freezeRotation = true;
        trapTimer = trapInterval;

        patrolTimer = Random.Range(1f, 3f);
        patrolDirection = Random.insideUnitCircle.normalized;

        // 初始播放走路动画
        PlayAnimation(walkAnimation, true);
    }

    void Update()
    {
        if (isPaused || isKnockedBack) return;
        if (player == null) return;

        // 定时种陷阱
        trapTimer -= Time.deltaTime;
        if (trapTimer <= 0f)
        {
            trapTimer = trapInterval;
            PlaceTrapWithAnim();  // ★ 带动画的放陷阱
        }
        if (isPlacingTrap)
        {
            rb.velocity = Vector2.zero;
            return;
        }

        // 玩家伪装中 → 巡逻
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
            FlipByVelocity(patrolDirection);
            return;
        }

        float dist = Vector2.Distance(transform.position, player.position);


        // 首次发现玩家
        if (!hasAggro && dist <= detectRange)
            hasAggro = true;

        float safeDistance = fleeDistance + bufferZone;

        // 未发现 → 巡逻
        if (!hasAggro)
        {
            patrolTimer -= Time.deltaTime;
            if (patrolTimer <= 0)
            {
                patrolDirection = Random.insideUnitCircle.normalized;
                patrolTimer = Random.Range(1f, 3f);
            }
            rb.velocity = patrolDirection * moveSpeed * 0.3f;
            FlipByVelocity(patrolDirection);
            return;
        }

        // 发现玩家后
        if (dist < fleeDistance)
        {
            // 太近 → 远离
            Vector2 fleeDir = (transform.position - player.position).normalized;
            rb.velocity = fleeDir * moveSpeed;
            FlipByVelocity(fleeDir);
            PlayAnimation(walkAnimation, true);
        }
        else if (dist < safeDistance)
        {
            // 缓冲带内 → 停住
            rb.velocity = Vector2.zero;
        }
        else
        {
            // 安全距离外 → 巡逻
            patrolTimer -= Time.deltaTime;
            if (patrolTimer <= 0)
            {
                patrolDirection = Random.insideUnitCircle.normalized;
                patrolTimer = Random.Range(1f, 3f);
            }
            rb.velocity = patrolDirection * moveSpeed * 0.5f;
            FlipByVelocity(patrolDirection);
            PlayAnimation(walkAnimation, true);
        }
    }

    // ============================================
    // 陷阱 + 动画
    // ============================================

    /// <summary>
    /// 播放 attack 动画，播完生成陷阱并回到 walk
    /// </summary>
    void PlaceTrapWithAnim()
    {
        isPlacingTrap = true;
        PlayAnimation(attackAnimation, false);
        Instantiate(trapPrefab, transform.position, Quaternion.identity);

        // 动画播完自动回 walk（用协程等待动画时长）
        StartCoroutine(ReturnToWalkAfterDelay());
    }

    IEnumerator ReturnToWalkAfterDelay()
    {
        yield return new WaitForSeconds(1f); // 动画时长
        isPlacingTrap = false;
        PlayAnimation(walkAnimation, true);
    }

    // ============================================
    // Spine 动画
    // ============================================

    void PlayAnimation(string animName, bool loop)
    {
        if (skeletonAnimation == null) return;
        if (animName == currentAnim) return;
        currentAnim = animName;
        skeletonAnimation.AnimationState.SetAnimation(0, animName, loop);
    }

    // ============================================
    // 左右翻转
    // ============================================

    void FlipByVelocity(Vector2 velocity)
    {
        if (skeletonAnimation == null) return;
        if (velocity.x > 0.1f)
            skeletonAnimation.Skeleton.ScaleX = 1f;
        else if (velocity.x < -0.1f)
            skeletonAnimation.Skeleton.ScaleX = -1f;
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

    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 1f, 0f, 0.15f);
        Gizmos.DrawWireSphere(transform.position, detectRange);
        Gizmos.color = new Color(0f, 1f, 0f, 0.2f);
        Gizmos.DrawWireSphere(transform.position, fleeDistance);
    }

    void OnDrawGizmos()
    {
        Gizmos.color = new Color(1f, 1f, 0f, 0.15f);
        Gizmos.DrawSphere(transform.position, detectRange);
        Gizmos.color = new Color(0f, 1f, 0f, 0.2f);
        Gizmos.DrawSphere(transform.position, fleeDistance);
    }
}