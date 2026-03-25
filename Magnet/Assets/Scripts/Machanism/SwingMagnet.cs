using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwingMagnet : MonoBehaviour
{
    [Header("摆动设置")]
    public float pushForce = 10f;      // 玩家移动时给磁铁的推力
    public float maxSwingSpeed = 5f;   // 最大摆动速度

    [Header("摆动控制")]
    public float swingForce = 10f;      // A/D键摆动推力（备用控制方式）
    public bool useKeyControl = false;  // 是否使用A/D键控制摆动（默认使用玩家移动控制）

    private Rigidbody2D rb;
    private bool isPlayerAttached = false;
    private PlayerController player;
    private Magnet magnet;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
        }

        magnet = GetComponent<Magnet>();
    }

    void FixedUpdate()
    {
        if (!isPlayerAttached || player == null) return;

        if (useKeyControl)
        {
            // 方式1：直接用A/D键控制摆动（从SwingRope迁移过来）
            HandleKeySwingControl();
        }
        else
        {
            // 方式2：通过玩家在磁铁上移动来施加力（原有逻辑）
            HandlePlayerMovementSwing();
        }
    }

    /// 方式1：通过A/D键直接控制磁铁摆动（从SwingRope迁移）
    void HandleKeySwingControl()
    {
        if (Input.GetKey(KeyCode.A))
        {
            rb.AddForce(Vector2.left * swingForce, ForceMode2D.Force);
        }
        if (Input.GetKey(KeyCode.D))
        {
            rb.AddForce(Vector2.right * swingForce, ForceMode2D.Force);
        }

        // 可选：限制最大速度
        rb.velocity = Vector2.ClampMagnitude(rb.velocity, maxSwingSpeed);
    }

    /// 方式2：通过玩家在磁铁上移动来施加力
    void HandlePlayerMovementSwing()
    {
        float moveInput = player.GetHorizontalMove();

        if (moveInput != 0)
        {
            // 给角速度，而不是线速度
            float torque = -moveInput * pushForce;
            rb.AddTorque(torque, ForceMode2D.Force);

            // 限制角速度
            rb.angularVelocity = Mathf.Clamp(rb.angularVelocity, -maxSwingSpeed, maxSwingSpeed);
        }
    }

    public void AttachPlayer(PlayerController playerController)
    {
        isPlayerAttached = true;
        player = playerController;

        // 可选：附着时重置速度，避免突然的力
        // rb.velocity = Vector2.zero;
        // rb.angularVelocity = 0;
    }

    public void DetachPlayer()
    {
        isPlayerAttached = false;
        player = null;
    }
}
