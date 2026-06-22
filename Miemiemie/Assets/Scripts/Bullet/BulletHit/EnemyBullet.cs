using UnityEngine;

public class EnemyBullet : MonoBehaviour
{
    [SerializeField] private float damage = 5f;             // 伤害

    private BulletBehav bulletBehav;
    private bool hasHit = false;

    [SerializeField] private AudioClip hitSound;  // 在Inspector中拖入对应的音效
    void Awake()
    {
        bulletBehav = GetComponent<BulletBehav>();
    }

    void OnEnable()
    {
        hasHit = false;  // 每次激活重置
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // 已命中或无组件则跳过
        if (hasHit || bulletBehav == null) return;

        // 检查是否在目标层（使用bulletBehav的配置，与其他子弹类型保持一致）
        if (((1 << other.gameObject.layer) & bulletBehav.targetLayer) != 0)
        {
            Health health = other.GetComponent<Health>();
            if (health != null)
            {
                hasHit = true;

                health.TakeDamage(damage);
                // ★ 播放音效
                if (hitSound != null)
                {
                    AudioManager.Instance.PlaySound(hitSound);
                }

                // 播放击中特效
                EffectPool.Instance?.PlayAt("BulletHit", transform.position);
                
                // 回收子弹
                bulletBehav.ReleaseToPool();
            }
        }
    }
}