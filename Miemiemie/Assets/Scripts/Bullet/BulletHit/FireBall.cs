using UnityEngine;

public class FireBall : MonoBehaviour
{
    [Header("灼烧设置")]
    [SerializeField] private float burnDuration = 1f;        // 灼烧持续1秒
    [SerializeField] private float burnDamagePerSecond = 5f; // 每秒伤害

    private BulletBehav bulletBehav;
    private bool hasHit = false;

    [SerializeField] private AudioClip hitSound;  // 在Inspector中拖入对应的音效
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

            // 施加灼烧（玩家和敌人都能被烧）
            BurnEffect burn = other.GetComponent<BurnEffect>();
            if (burn == null) burn = other.gameObject.AddComponent<BurnEffect>();
            burn.StartBurn(burnDuration, burnDamagePerSecond);

            // ★ 播放音效
            if (hitSound != null)
            {
                if (hitSound != null) AudioManager.Instance.PlaySound(hitSound);
            }

            // 特效
            EffectPool.Instance?.PlayAt("BulletHit", transform.position);

            // 回收
            bulletBehav.ReleaseToPool();
        }
    }
}