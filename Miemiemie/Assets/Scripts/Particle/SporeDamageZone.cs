using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SporeDamageZone : MonoBehaviour
{
    [Header("伤害设置")]
    [SerializeField] private float damagePerSecond = 15f;
    [SerializeField] private float lingerDuration = 5f;
    [SerializeField] private LayerMask playerLayer;

    [Header("范围设置")]
    [SerializeField] private float damageRadius = 3f;

    private CircleCollider2D damageZone;
    private SporePool pool;

    void Start()
    {
        damageZone = gameObject.AddComponent<CircleCollider2D>();
        damageZone.radius = damageRadius;
        damageZone.isTrigger = true;

        pool = FindObjectOfType<SporePool>();
        Invoke(nameof(ReturnToPool), lingerDuration);
    }

    void ReturnToPool()
    {
        if (pool != null)
            pool.ReleaseSpore(gameObject);
        else
            Destroy(gameObject);
    }

    void OnTriggerStay2D(Collider2D other)
    {
        if (((1 << other.gameObject.layer) & playerLayer) != 0)
        {
            Debug.Log($"玩家在孢子中！每秒受伤: {damagePerSecond}");
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1, 0, 0, 0.3f);
        Gizmos.DrawWireSphere(transform.position, damageRadius);
    }
}
