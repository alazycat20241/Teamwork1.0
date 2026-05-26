using UnityEngine;

public class WindBullet : MonoBehaviour
{
    [Header("击退设置")]
    [SerializeField] private float knockbackDistance = 1f;  // 击退距离
    [SerializeField] private float knockbackSpeed = 10f;    // 击退速度

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

            // 击退
            StartCoroutine(Knockback(other.gameObject));

            // 特效
            EffectPool.Instance?.PlayAt("WindHit", transform.position);

            // 回收
            bulletBehav.ReleaseToPool();
        }
    }

    /// <summary>
    /// 击退协程：把目标推出一格距离
    /// </summary>
    System.Collections.IEnumerator Knockback(GameObject target)
    {
        Rigidbody2D targetRb = target.GetComponent<Rigidbody2D>();
        if (targetRb == null) yield break;

        // 计算击退方向（子弹飞行方向）
        Vector2 knockbackDir = transform.right;

        // 禁用目标的移动输入，让击退生效
        float elapsed = 0f;
        float duration = knockbackDistance / knockbackSpeed;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            targetRb.velocity = knockbackDir * knockbackSpeed;
            yield return null;
        }

        // 击退结束，停止
        targetRb.velocity = Vector2.zero;
    }
}