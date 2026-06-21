using UnityEngine;

public class DeathExplosion : MonoBehaviour
{
    [SerializeField] private float damage = 10f;      // 爆炸伤害
    [SerializeField] private float lifetime = 0.5f;   // 爆炸持续时间

    private bool hasExploded = false;

    void Start()
    {
        // 到时间自动销毁
        Destroy(gameObject, lifetime);
    }

    /// <summary>
    /// 爆炸范围内对玩家造成伤害
    /// </summary>
    void OnTriggerEnter2D(Collider2D other)
    {
        // 已经炸过了 → 不再扣血
        if (hasExploded) return;

        if (other.CompareTag("Player"))
        {
            Health health = other.GetComponent<Health>();
            if (health != null)
            {
                health.TakeDamage(damage);
                hasExploded = true;
            }
        }
    }
}