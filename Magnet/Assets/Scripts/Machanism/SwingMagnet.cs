using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwingMagnet : MonoBehaviour
{
    [Header("摆动设置")]
    public float pushForce = 10f;      // 玩家移动时给磁铁的推力
    public float maxSwingSpeed = 5f;   // 最大摆动速度

    private Rigidbody2D rb;
    private bool isPlayerAttached = false;
    private PlayerController player;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
        }
    }

    void FixedUpdate()
    {
        if (!isPlayerAttached || player == null) return;

        // 获取玩家的移动方向
        float moveInput = player.GetHorizontalMove();

        if (moveInput != 0)
        {
            // 给磁铁一个水平方向的力
            Vector2 force = new Vector2(moveInput * pushForce, 0);
            rb.AddForce(force, ForceMode2D.Force);

            // 限制最大速度，避免飞得太远
            rb.velocity = new Vector2(
                Mathf.Clamp(rb.velocity.x, -maxSwingSpeed, maxSwingSpeed),
                rb.velocity.y
            );
        }
    }

    public void AttachPlayer(PlayerController playerController)
    {
        isPlayerAttached = true;
        player = playerController;

        // 可选：让磁铁稍微变重一点
        rb.mass = 1f;
    }

    public void DetachPlayer()
    {
        isPlayerAttached = false;
        player = null;
    }
}
