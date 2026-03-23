using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class ShakeCollision : MonoBehaviour
{
    [Header("抖动设置")]
    public float force;
    private CinemachineImpulseSource ImpulseSource;
    private Magnet parentMagnet;  // 获取父物体的 Magnet 脚本

    private void Start()
    {
        parentMagnet = GetComponentInParent<Magnet>();
        ImpulseSource = GetComponent<CinemachineImpulseSource>();
        if (ImpulseSource == null)
        {
            ImpulseSource = gameObject.AddComponent<CinemachineImpulseSource>();
        }

        // 配置抖动参数
        ImpulseSource.m_ImpulseDefinition.m_AmplitudeGain = force;//强度
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.gameObject.CompareTag("Player")) return;

        // 获取玩家的 PlayerController
        PlayerController player = other.gameObject.GetComponent<PlayerController>();
        if (player == null || parentMagnet == null) return;

        // 如果玩家已经站在磁铁上，不触发抖动
        if (player.isOnMagnet) return;

        // 检查是否互相吸引（不同磁极）
        bool isAttracting = player.currentPole != parentMagnet.pole;

        // 只有互相吸引且未附着时才触发抖动
        if (isAttracting)
        {
            ImpulseSource.GenerateImpulse();
        }
    }
}
