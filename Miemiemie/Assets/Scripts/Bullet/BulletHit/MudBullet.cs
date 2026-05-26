using UnityEngine;

public class MudBullet : MonoBehaviour
{
    [Header("减速设置")]
    [SerializeField] private float slowAmount = 0.25f;   // 减速25%
    [SerializeField] private float slowDuration = 2f;    // 减速持续2秒

    private BulletBehav bulletBehav;
    private bool hasHit = false;

    void Awake()
    {
        bulletBehav = GetComponent<BulletBehav>();
    }

    void OnEnable()
    {
        hasHit = false;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (hasHit || bulletBehav == null) return;

        if (((1 << other.gameObject.layer) & bulletBehav.targetLayer) != 0)
        {
            hasHit = true;

            // 直接伤害
            Health health = other.GetComponent<Health>();
            if (health != null)
            {
                health.TakeDamage(bulletBehav.damage);
            }

            // 施加减速
            SlowEffect slow = other.GetComponent<SlowEffect>();
            if (slow == null) slow = other.gameObject.AddComponent<SlowEffect>();
            slow.ApplySlow(slowAmount, slowDuration);

            // 特效
            EffectPool.Instance?.PlayAt("MudHit", transform.position);

            // 回收
            bulletBehav.ReleaseToPool();
        }
    }
}