using UnityEngine;

public class EnemyTurret : MonoBehaviour
{
    [Header("攻击参数")]
    [SerializeField] private float attackRange = 8f;        // 攻击范围（玩家进入才打）
    [SerializeField] private float attackCooldown = 2f;     // 攻击间隔（秒）
    [SerializeField] private GameObject sporeCloudPrefab;   // 孢子云预制体（范围伤害）

    [Header("死亡爆炸")]
    [SerializeField] private GameObject deathExplosionPrefab; // 死亡爆炸预制体
    [SerializeField] private float explosionRadius = 2f;      // 爆炸半径
    [SerializeField] private float explosionDamage = 10f;     // 爆炸伤害

    private Transform player;       // 玩家引用
    private float attackTimer;      // 攻击冷却计时器

    void Start()
    {
        // 找玩家
        GameObject playerObj = FixedRoomManager.Instance.GetPlayer();
        if (playerObj != null)
        {
            player = playerObj.transform;
        }
        attackTimer = attackCooldown;

        // 订阅死亡事件
        GetComponent<Health>().OnDeath += OnDeathExplosion;
    }

    void Update()
    {
        float dist = Vector2.Distance(transform.position, player.position);

        // 玩家不在攻击范围内就不打
        if (dist > attackRange)
            return;

        // 冷却计时
        attackTimer -= Time.deltaTime;

        // 冷却好了 → 朝玩家位置投掷孢子云
        if (attackTimer <= 0f)
        {
            attackTimer = attackCooldown;    // 重置冷却
            ThrowAttack();
        }
    }

    /// <summary>
    /// 在玩家当前位置生成孢子云
    /// </summary>
    void ThrowAttack()
    {
        Instantiate(sporeCloudPrefab, player.position, Quaternion.identity);
    }

    /// <summary>
    /// 死亡时调用（挂到Health组件的OnDeath事件上）
    /// </summary>
    public void OnDeathExplosion()
    {
        Instantiate(deathExplosionPrefab, transform.position, Quaternion.identity);
    }
}