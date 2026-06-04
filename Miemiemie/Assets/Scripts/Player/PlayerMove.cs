using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMove : MonoBehaviour
{
    [Header("移动设置")]
    [SerializeField] private float moveSpeed = 5f;
    
    [Header("翻转设置")]
    [SerializeField] private bool flipByScale = true;  // 是否通过缩发放翻转（带动所有子物体）
    [SerializeField] private Transform visualRoot;     // 视觉根节点（骨骼/精灵的父级），如果为空则翻转自身
    
    private Rigidbody2D rb;
    private Vector2 movement;
    private bool isStunned = false;
    
    // 动画
    private Animator anim;                    
    private SpriteRenderer spriteRenderer;
    
    // 翻转状态
    private bool isFacingRight = false;  // 记录当前朝向
    private Transform flipTarget;       // 实际翻转的目标Transform
    
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
        }
        
        // 俯视角2D设置
        rb.gravityScale = 0f;
        rb.freezeRotation = true;
        
        anim = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        
        // 确定翻转目标：如果有指定视觉根节点则用它，否则用自身
        flipTarget = visualRoot != null ? visualRoot : transform;
    }
    
    void Update()
    {
        if (isStunned)
        {
            movement = Vector2.zero;
            return;
        }
        
        // 获取输入
        movement.x = Input.GetAxisRaw("Horizontal");
        movement.y = Input.GetAxisRaw("Vertical");
        
        // 归一化
        movement = movement.normalized;
        
        // ===== 动画 =====
        bool isMoving = movement.magnitude > 0.01f;
        anim?.SetBool("IsMoving", isMoving);
        
        // ===== 翻转逻辑（使用缩放，带动所有子物体） =====
        if (movement.x > 0.01f && !isFacingRight)
        {
            FlipToRight();
        }
        else if (movement.x < -0.01f && isFacingRight)
        {
            FlipToLeft();
        }
        // 上下移动时不改变朝向，保持上次左右朝向
        
        // ===== 射击检测 =====
        bool isShooting = Input.GetMouseButton(1);
        if (PropManager.Instance != null) 
            PropManager.Instance.UpdateDouPeng(isMoving, isShooting);
    }
    
    void FixedUpdate()
    {
        rb.velocity = movement * moveSpeed;
    }
    
    /// <summary>
    /// 翻转向右
    /// </summary>
    private void FlipToRight()
    {
        isFacingRight = true;
        
        if (flipByScale)
        {
            // 方案1：缩发放翻转（推荐，带动所有子物体包括骨骼、魔杖、跟宠）
            Vector3 scale = flipTarget.localScale;
            scale.x = -Mathf.Abs(scale.x);  // 确保x为正
            flipTarget.localScale = scale;
        }
        else
        {
            // 方案2：仅翻转SpriteRenderer（旧方案，不会带动子物体）
            if (spriteRenderer != null)
                spriteRenderer.flipX = true;
        }
    }
    
    /// <summary>
    /// 翻转向左
    /// </summary>
    private void FlipToLeft()
    {
        isFacingRight = false;
        
        if (flipByScale)
        {
            // 方案1：缩发放翻转
            Vector3 scale = flipTarget.localScale;
            scale.x = Mathf.Abs(scale.x);  // 确保x为负
            flipTarget.localScale = scale;
        }
        else
        {
            // 方案2：仅翻转SpriteRenderer
            if (spriteRenderer != null)
                spriteRenderer.flipX = false;
        }
    }
    
    /// <summary>
    /// 被定身（外部调用）
    /// </summary>
    public void Stun(float duration)
    {
        if (!isStunned)
            StartCoroutine(StunCoroutine(duration));
    }
    
    IEnumerator StunCoroutine(float duration)
    {
        isStunned = true;
        yield return new WaitForSeconds(duration);
        isStunned = false;
    }
    
    /// <summary>
    /// 定身（直到调用 Resume 解除）
    /// </summary>
    public void Freeze()
    {
        isStunned = true;
    }
    
    /// <summary>
    /// 解除定身
    /// </summary>
    public void Resume()
    {
        isStunned = false;
    }
}