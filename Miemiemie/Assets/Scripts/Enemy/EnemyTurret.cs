using UnityEngine;

public class EnemyTurret : MonoBehaviour
{
    [Header("攻击参数")]
    [SerializeField] private float attackRange = 8f;        // 攻击范围
    [SerializeField] private float attackCooldown = 2f;     // 攻击间隔
    [SerializeField] private GameObject sporeCloudPrefab;   // 小型孢子云预制体（复用你的孢子系统）
    [SerializeField] private float throwDamage = 10f;       // 伤害（半颗心）

    [Header("死亡爆炸")]
    [SerializeField] private GameObject deathExplosionPrefab; // 死亡爆炸预制体
    [SerializeField] private float explosionRadius = 2f;
    [SerializeField] private float explosionDamage = 10f;

    private Transform player;
    private float attackTimer;

    private bool hasAggro = false;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        attackTimer = attackCooldown;
    }

    void Update()
    {
        float dist = Vector2.Distance(transform.position, player.position);

        // 首次进入攻击范围，激活仇恨
        if (!hasAggro && dist <= attackRange)
        {
            hasAggro = true;
        }

        // 没激活就不攻击
        if (!hasAggro)
        {
            return;
        }

        // 攻击冷却计时
        attackTimer -= Time.deltaTime;

        // 冷却好了就朝玩家位置投掷孢子
        if (attackTimer <= 0f)
        {
            attackTimer = attackCooldown;    // 重置冷却
            ThrowAttack();                   // 在玩家位置生成孢子云
        }
    }

    /// <summary>
    /// 朝玩家位置投掷圆形伤害
    /// </summary>
    void ThrowAttack()
    {
        // 在玩家当前位置生成孢子云（小范围、短持续）
        Instantiate(sporeCloudPrefab, player.position, Quaternion.identity);
        // 注：这里应该用对象池，简化起见用Instantiate
    }

    /// <summary>
    /// 死亡时调用（由Health组件的OnDeath事件触发）
    /// </summary>
    public void OnDeathExplosion()
    {
        // 在自身位置生成爆炸
        GameObject explosion = Instantiate(deathExplosionPrefab, transform.position, Quaternion.identity);
        // 爆炸有一个CircleCollider2D，OnTriggerEnter2D对玩家造成伤害
    }
}