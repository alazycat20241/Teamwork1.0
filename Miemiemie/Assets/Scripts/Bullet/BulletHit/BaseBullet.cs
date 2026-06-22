using UnityEngine;

public class BaseBullet : MonoBehaviour
{
    [SerializeField] private float damage = 10f;             // 伤害

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

                // 计算伤害：基础伤害 * (1 + 百分比加成) + 固定加成
                float finalDamage = damage;
                if (PlayerStats.Instance != null)
                {
                    finalDamage = (damage + PlayerStats.Instance.attackBonus) * (1 + PlayerStats.Instance.attackPercentBonus) ;
                }

                // 先检查是否已石化，避免对石化敌人造成伤害和特效
                bool isAlreadyStoned = health.isStoned;
                
                // 石化中的敌人不造成伤害（由Health.TakeDamage内部处理），但仍需判定新效果
                if (!isAlreadyStoned)
                {
                    // 造成伤害
                    health.TakeDamage(finalDamage);
                }

                // 石化判定：10%概率触发石化2秒（不对已石化敌人重复触发）
                if (!isAlreadyStoned && other.CompareTag("Enemy") && PlayerStats.Instance != null && Random.value < PlayerStats.Instance.stoneChance)
                {
                    PropManager.Instance.ApplyStone(other.gameObject, PlayerStats.Instance.stoneDuration);
                }
                
                // 恐慌判定：20%概率触发恐慌
                if (other.CompareTag("Enemy") && PlayerStats.Instance != null && Random.value < PlayerStats.Instance.panicChance)
                {
                    PropManager.Instance.ApplyPanic(other.gameObject, PlayerStats.Instance.panicDuration);
                }

                // 只有非石化敌人才播放击中音效和特效
                if (!isAlreadyStoned)
                {
                    // ★ 播放音效
                    if (hitSound != null)
                    {
                        AudioManager.Instance.PlaySound(hitSound);
                    }

                    // 播放击中特效
                    EffectPool.Instance?.PlayAt("BulletHit", transform.position);
                }

                // 回收子弹
                bulletBehav.ReleaseToPool();
            }
        }
    }
}