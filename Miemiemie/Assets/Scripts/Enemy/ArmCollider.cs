using UnityEngine;

/// <summary>
/// 手臂碰撞检测
/// 碰到玩家 → 通知 EnemyTentacle
/// </summary>
public class ArmCollider : MonoBehaviour
{
    private EnemyTentacle owner;

    void Start()
    {
        owner = GetComponentInParent<EnemyTentacle>();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            owner.OnArmHitPlayer(other.gameObject);
        }
    }
}