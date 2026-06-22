using UnityEngine;

public class BaseBullet : MonoBehaviour
{
    [SerializeField] private float damage = 10f;             // 伤害
    [SerializeField] private LayerMask targetLayer;          // 目标层

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

        // 检查是否在目标层
        if (((1 << other.gameObject.layer) & targetLayer) != 0)
        {
            Health health = other.GetComponent<Health>();
            if (health != null)
            {
                hasHit = true;

                // 计算伤害：基础伤害 * (1 + 百分比加成) + 固定加成
                float finalDamage = damage;
                if (PlayerStats.Instance != null)
                {
                    finalDamage = (damage + PlayerStats.Instance.attackBonus) * (1 + PlayerStats.Instance.attackPercentBonus) ;
                }

                // 造成伤害
                health.TakeDamage(finalDamage);

                // 石化和恐慌判定：10%概率触发石化2秒(目前只加了基础子弹，其他子弹看看日后
                if (other.CompareTag("Enemy") && PlayerStats.Instance != null && Random.value < PlayerStats.Instance.stoneChance)
                {
                    PropManager.Instance.ApplyStone(other.gameObject, PlayerStats.Instance.stoneDuration);
                }
                if (other.CompareTag("Enemy") && PlayerStats.Instance != null && Random.value < PlayerStats.Instance.panicChance)
                {
                    PropManager.Instance.ApplyPanic(other.gameObject, PlayerStats.Instance.panicDuration);
                }

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