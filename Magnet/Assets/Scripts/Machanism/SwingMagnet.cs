using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwingMagnet : MonoBehaviour
{
    [Header("摆动设置")]
    public float swingForce = 20f;           // 摆动力量
    public float maxSwingAngle = 90f;        // 最大摆动角度
    public float damping = 0.98f;            // 阻尼（越小停得越快）

    [Header("组件引用")]
    public Rigidbody2D pendulumRod;          // 杆子的 Rigidbody2D

    private PlayerController attachedPlayer;
    private bool isPlayerAttached = false;

    void Start()
    {
        // 获取杆子的 Rigidbody（向上找父物体）
        if (pendulumRod == null)
        {
            pendulumRod = GetComponentInParent<Rigidbody2D>();
        }
    }

    void Update()
    {
        if (!isPlayerAttached) return;

        // 获取玩家输入 (A/D 或 左右箭头)
        float horizontal = Input.GetAxisRaw("Horizontal");

        if (horizontal != 0 && pendulumRod != null)
        {
            ApplySwingForce(horizontal);
        }
    }

    void ApplySwingForce(float direction)
    {
        // 获取当前杆子的角度
        float currentAngle = pendulumRod.rotation;

        // 归一化到 -180 到 180
        if (currentAngle > 180) currentAngle -= 360;

        // 检查是否超过最大摆动角度
        if (Mathf.Abs(currentAngle) >= maxSwingAngle)
        {
            // 如果已经达到最大角度且还在往同方向加力，则停止
            bool isAtMaxAngle = (currentAngle >= maxSwingAngle && direction > 0) ||
                                (currentAngle <= -maxSwingAngle && direction < 0);
            if (isAtMaxAngle) return;
        }

        //直接施加扭矩
         float torque = swingForce * direction;
         pendulumRod.AddTorque(torque, ForceMode2D.Force);
    }

    public void AttachPlayer(PlayerController player)
    {
        attachedPlayer = player;
        isPlayerAttached = true;

        // 增加一点质量，让摆动更真实
        if (pendulumRod != null)
        {
            pendulumRod.mass += 0.5f;
        }
    }

    public void DetachPlayer()
    {
        isPlayerAttached = false;
        attachedPlayer = null;

        // 恢复质量
        if (pendulumRod != null)
        {
            pendulumRod.mass -= 0.5f;
        }
    }

    void FixedUpdate()
    {
        // 应用阻尼，让摆动慢慢停止
        if (pendulumRod != null && damping < 1f && !isPlayerAttached)
        {
            pendulumRod.velocity *= damping;
            pendulumRod.angularVelocity *= damping;
        }
    }
}
