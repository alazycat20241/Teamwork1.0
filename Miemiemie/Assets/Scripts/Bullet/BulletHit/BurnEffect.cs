using UnityEngine;

public class BurnEffect : MonoBehaviour
{
    [Header("灼烧伤害")]
    [SerializeField] private float tickInterval = 0.5f;  // 每0.5秒跳一次伤害

    private float timer;             // 剩余灼烧时间
    private float damagePerSecond;   // 每秒伤害
    private float tickTimer;
    private Health health;

    // ★ 火焰特效
    private GameObject fireEffect;   // 跟随的火焰特效实例

    void Awake()
    {
        health = GetComponent<Health>();
    }

    /// <summary>
    /// 开始灼烧
    /// </summary>
    public void StartBurn(float duration, float dps)
    {
        timer = duration;
        damagePerSecond = dps;
        tickTimer = 0f;

        // ★ 如果没有火焰特效，从对象池拿一个
        if (fireEffect == null && EffectPool.Instance != null)
        {
            fireEffect = EffectPool.Instance.Get("FireBurning");  // 用对象池的火焰特效
        }
    }

    void Update()
    {
        if (timer <= 0)
        {
            StopBurn();
            return;
        }

        timer -= Time.deltaTime;
        tickTimer -= Time.deltaTime;

        // ★ 火焰特效跟随自己
        if (fireEffect != null)
        {
            fireEffect.transform.position = transform.position;
        }

        // 每0.5秒造成一次灼烧伤害
        if (tickTimer <= 0)
        {
            tickTimer = tickInterval;
            health?.TakeDamage(damagePerSecond * tickInterval);
        }
    }

    /// <summary>
    /// 结束灼烧，回收火焰特效
    /// </summary>
    void StopBurn()
    {
        if (fireEffect != null && EffectPool.Instance != null)
        {
            EffectPool.Instance.Release("FireBurning", fireEffect);
            fireEffect = null;
        }
        Destroy(this);
    }

    /// <summary>
    /// 灼烧传递：碰到其他单位时传染
    /// </summary>
    void OnTriggerEnter2D(Collider2D other)
    {
        if (timer <= 0) return;

        // ★ 只有 Player 或 Enemy 层才传染
        if (!other.CompareTag("FireCol") && !other.CompareTag("Enemy")) return;

        BurnEffect otherBurn = other.GetComponent<BurnEffect>();
        if (otherBurn == null) otherBurn = other.gameObject.AddComponent<BurnEffect>();
        otherBurn.StartBurn(1f, damagePerSecond);
    }

    void OnDestroy()
    {
        // 销毁时也回收火焰特效
        if (fireEffect != null && EffectPool.Instance != null)
        {
            EffectPool.Instance.Release("FireBurning", fireEffect);
        }
    }

    public bool IsBurning => timer > 0;
}