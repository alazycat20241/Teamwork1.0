using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwingMagnet : MonoBehaviour
{
    [Header("摆动设置")]
    public float pushForce;      // 玩家移动时给磁铁的推力
    public float maxSwingSpeed;

    private Rigidbody2D rb;
    private bool isPlayerAttached = false;
    private PlayerController player;
    private Magnet magnet;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        magnet = GetComponent<Magnet>();
    }

    void FixedUpdate()
    {
        if (!isPlayerAttached || player == null) return;
        //HandleKeySwingControl();
        rb.AddForce(Vector2.down * pushForce, ForceMode2D.Force);

    }
    void HandleKeySwingControl()
    {
        if (Input.GetKey(KeyCode.A))
        {
            rb.AddForce(Vector2.left * pushForce, ForceMode2D.Force);

        }
        if (Input.GetKey(KeyCode.D))
        {
            rb.AddForce(Vector2.right * pushForce, ForceMode2D.Force);
        }

        // 可选：限制最大速度
        rb.velocity = Vector2.ClampMagnitude(rb.velocity, maxSwingSpeed);
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
