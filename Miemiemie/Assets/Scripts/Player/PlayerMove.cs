using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMove : MonoBehaviour
{
    [Header("移动设置")]
    [SerializeField] private float moveSpeed = 5f;

    private Rigidbody2D rb;
    private Vector2 movement;

    // 用于定身
    private bool isStunned = false;

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
    }

    void Update()
    {
        if (isStunned)
        {
            movement = Vector2.zero;  // 不能移动
            return;
        }

        // 获取输入
        movement.x = Input.GetAxisRaw("Horizontal"); // A/D 或 左右箭头
        movement.y = Input.GetAxisRaw("Vertical");   // W/S 或 上下箭头

        // 归一化防止斜向移动过快
        movement = movement.normalized;
    }

    void FixedUpdate()
    {
        rb.velocity = movement * moveSpeed;
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
