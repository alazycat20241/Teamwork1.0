using UnityEngine;
using System.Collections;

public class WindBullet : MonoBehaviour
{
    [Header("击退设置")]
    [SerializeField] private float knockbackDistance = 1f;    // 击退距离（1格）
    [SerializeField] private float knockbackDuration = 0.15f; // 击退持续时间（越短越有力）

    private BulletBehav bulletBehav;    // 子弹行为组件
    private bool hasHit = false;        // 是否已命中（防止重复触发）

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
        if (hasHit || bulletBehav == null) return;

        if (((1 << other.gameObject.layer) & bulletBehav.targetLayer) != 0)
        {
            hasHit = true;

            // ★ 先存方向，再处理
            Vector2 knockbackDir = (other.transform.position - transform.position).normalized;

            Health health = other.GetComponent<Health>();
            if (health != null)
            {
                health.TakeDamage(bulletBehav.damage);
            }

            // ★ 协程挂到敌人身上
            KnockbackOnTarget(other.gameObject, knockbackDir);

            // ★ 播放音效
            if (hitSound != null)
            {
                if (hitSound != null) AudioManager.Instance.PlaySound(hitSound);
            }

            EffectPool.Instance?.PlayAt("WindHit", transform.position);

            bulletBehav.ReleaseToPool();  // 回收子弹
        }
    }

    void KnockbackOnTarget(GameObject target, Vector2 direction)
    {
        // ★ 如果目标已死亡/未激活，不击退
        if (target == null || !target.activeInHierarchy) return;

        Rigidbody2D targetRb = target.GetComponent<Rigidbody2D>();
        if (targetRb == null) return;

        IMovable movable = target.GetComponent<IMovable>();
        if (movable != null)
        {
            // 在敌人身上跑协程
            MonoBehaviour mb = movable as MonoBehaviour;

            if (mb == null || !mb.isActiveAndEnabled) return;  // ★ 确保脚本活跃

            mb.StartCoroutine(KnockbackRoutine(targetRb, direction, movable));
        }
    }

    IEnumerator KnockbackRoutine(Rigidbody2D targetRb, Vector2 direction, IMovable movable)
    {
        movable.StartKnockback();

        float elapsed = 0f;
        float startSpeed = knockbackDistance / knockbackDuration;  // ★ 用配置计算速度

        while (elapsed < knockbackDuration)
        {
            elapsed += Time.deltaTime;
            float speed = Mathf.Lerp(startSpeed, 0f, elapsed / 0.15f);
            targetRb.velocity = direction * speed;
            yield return null;
        }

        targetRb.velocity = Vector2.zero;
        movable.EndKnockback();
    }
}