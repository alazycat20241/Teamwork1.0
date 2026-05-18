using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletDamage : MonoBehaviour
{
    [SerializeField] private float damage = 10f;
    [SerializeField] private LayerMask targetLayer; // 敌人或玩家层
    private bool isReleased = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (((1 << other.gameObject.layer) & targetLayer) != 0)
        {
            if (isReleased) return;  // ← 已回收，跳过
            isReleased = true;       // ← 标记已回收
            Health health = other.GetComponent<Health>();
            if (health != null)
            {
                health.TakeDamage(damage);
                // 命中后回收子弹
                if (TryGetComponent<BulletBehav>(out var bullet))
                {
                    bullet.pool.RealseItem(bullet);
                }
            }
        }
    }
}
