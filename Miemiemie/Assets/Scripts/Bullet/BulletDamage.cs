using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletDamage : MonoBehaviour
{
    private BulletBehav bulletBehav;    // 子弹行为组件引用
    private bool hasHit = false;        // 是否已命中（防止重复触发）

    /// <summary>
    /// 获取组件引用
    /// </summary>
    void Awake()
    {
        bulletBehav = GetComponent<BulletBehav>();
    }

    /// <summary>
    /// 每次从池中激活时重置命中状态
    /// </summary>
    private void OnEnable()
    {
        hasHit = false;
    }

    /// <summary>
    /// 碰撞检测：命中目标时造成伤害、播放特效、回收子弹
    /// </summary>
    private void OnTriggerEnter2D(Collider2D other)
    {
        // 已命中或无子弹组件则跳过
        if (hasHit || bulletBehav == null) return;

        // 检查碰撞对象是否在目标层中
        if (((1 << other.gameObject.layer) & bulletBehav.targetLayer) != 0)
        {
            Health health = other.GetComponent<Health>();
            if (health != null)
            {
                // 标记已命中，防止同一颗子弹重复触发
                hasHit = true;

                // 造成伤害（使用BulletBehav上配置的伤害值）
                health.TakeDamage(bulletBehav.damage);

                // 播放击中特效（使用BulletBehav上配置的特效名）
                if (!string.IsNullOrEmpty(bulletBehav.hitEffectKey))
                {
                    EffectPool.Instance?.PlayAt(bulletBehav.hitEffectKey, transform.position);
                }

                // 回收子弹到对象池
                bulletBehav.ReleaseToPool();
            }
        }
    }
}