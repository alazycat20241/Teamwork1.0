using UnityEngine;

public class Trap : MonoBehaviour
{
    [SerializeField] private float damage = 10f;            // 对玩家伤害
    [SerializeField] private float lifetime = 30f;          // 最长存活时间

    void Start()
    {
        Destroy(gameObject, lifetime);  // 超时自动消失
    }

    /// <summary>
    /// 玩家踩到陷阱
    /// </summary>
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Health health = other.GetComponent<Health>();
            if (health != null)
            {
                health.TakeDamage(damage);
            }
            // 踩到后陷阱消失
            Destroy(gameObject);
        }
    }

    // 陷阱有Health组件，可以被子弹打掉（在Health里设maxHealth=1即可）
}