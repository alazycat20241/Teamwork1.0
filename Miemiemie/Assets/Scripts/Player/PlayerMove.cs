using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMove : MonoBehaviour
{
    [Header("移动设置")]
    [SerializeField] private float moveSpeed = 5f;

    private Rigidbody2D rb;
    private Vector2 movement;

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
}
