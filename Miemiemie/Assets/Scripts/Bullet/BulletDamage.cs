using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletDamage : MonoBehaviour
{
    [SerializeField] private float damage = 10f;
    [SerializeField] private LayerMask targetLayer; // 敌人或玩家层

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (((1 << other.gameObject.layer) & targetLayer) != 0)
        {
            Health health = other.GetComponent<Health>();
            if (health != null)
            {
                health.TakeDamage(damage);
                EffectPool.Instance?.PlayAt("BulletHit", transform.position);
                // 命中后回收子弹
                if (TryGetComponent<BulletBehav>(out var bullet))
                {
                    bullet.ReleaseToPool();  //改用封装方法，不用 pool.RealseItem
                }
            }
        }
    }
}
