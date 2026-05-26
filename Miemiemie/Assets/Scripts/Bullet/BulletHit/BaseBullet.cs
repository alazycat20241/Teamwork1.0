using UnityEngine;

public class BaseBullet : MonoBehaviour
{
    [SerializeField] private float damage = 10f;             // 伤害
    [SerializeField] private LayerMask targetLayer;          // 目标层（敌人或玩家）

    private float extraDamage = 0f;

    private BulletBehav bulletBehav;
    private bool hasHit = false;

    void Awake()
    {
        bulletBehav = GetComponent<BulletBehav>();
    }

    void OnEnable()
    {
        hasHit = false;  // 每次激活重置
        extraDamage = 0f;
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

                // 造成伤害
                health.TakeDamage(damage + extraDamage);

                // 播放击中特效
                EffectPool.Instance?.PlayAt("BulletHit", transform.position);

                // 回收子弹
                bulletBehav.ReleaseToPool();
            }
        }
    }

    // 设置伤害（替换，不累加）
    public void SetExtraDamage(float amount)
    {
        extraDamage = amount;
    }
}