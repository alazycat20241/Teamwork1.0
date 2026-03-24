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

        float moveInput = player.GetHorizontalMove();

        if (moveInput != 0)
        {
            // 给角速度，而不是线速度
            float torque = -moveInput * pushForce;  // 负号根据悬挂点位置调整
            rb.AddTorque(torque, ForceMode2D.Force);

            // 限制角速度
            rb.angularVelocity = Mathf.Clamp(rb.angularVelocity, -maxSwingSpeed, maxSwingSpeed);
        }
    }

    public void AttachPlayer(PlayerController playerController)
    {
        isPlayerAttached = true;
        player = playerController;
    }

    public void DetachPlayer()
    {
        isPlayerAttached = false;
        player = null;
    }
}
