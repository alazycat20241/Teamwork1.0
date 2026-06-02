using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Boss : MonoBehaviour
{
    [Header("Boss参数")]
    [SerializeField] private float phase2HealthPercent = 0.33f; // 低于33%血进入二阶段

    [Header("一阶段：圆形弹幕")]
    [SerializeField] private BulletObject circleBulletConfig;   // 圆形弹幕配置
    [SerializeField] private float circleFireInterval = 1.5f;   // 发射间隔

    [Header("二阶段：投掷炸弹")]
    [SerializeField] private BulletObject bombConfig;           // 炸弹子弹配置
    [SerializeField] private float bombInterval = 2f;           // 投弹间隔

    [Header("二阶段：召唤小怪")]
    [SerializeField] private List<GameObject> minionPrefabs;    // 小怪预制体列表
    [SerializeField] private int initialMinionCount = 2;        // 初始召唤2-3个
    [SerializeField] private int maxInitialMinion = 3;
    [SerializeField] private float summonInterval = 30f;        // 每30秒召唤
    [SerializeField] private int summonCount = 1;               // 每次召唤1个
    [SerializeField] private float summonRadius = 3f;           // 召唤范围

    // ========== 内部状态 ==========
    private enum Phase { One, Two }
    private Phase currentPhase = Phase.One;

    private Transform player;
    private Health health;

    // 对象池
    private BulletPool circleBulletPool;
    private BulletPool bombPool;

    // 计时器
    private float circleFireTimer;
    private float bombTimer;
    private float summonTimer;

    void Start()
    {
        GameObject playerObj = FixedRoomManager.Instance.GetPlayer();
        if (playerObj != null) player = playerObj.transform;

        health = GetComponent<Health>();
        // 初始化对象池
        if (circleBulletConfig != null)
            circleBulletPool = PoolManager.Instance.GetPool(circleBulletConfig);
        if (bombConfig != null)
            bombPool = PoolManager.Instance.GetPool(bombConfig);

        circleFireTimer = circleFireInterval;
        bombTimer = bombInterval;
        summonTimer = summonInterval;
    }

    void Update()
    {
        if (player == null || health == null || health.IsDead) return;

        // 检查是否进入二阶段
        if (currentPhase == Phase.One && health.CurrentHealth <= health.MaxHealth * phase2HealthPercent)
        {
            EnterPhaseTwo();
        }

        switch (currentPhase)
        {
            case Phase.One:
                PhaseOneAttack();
                break;
            case Phase.Two:
                PhaseTwoAttack();
                break;
        }
    }

    /// <summary>
    /// 一阶段：圆形弹幕（用BulletObject发射器自动扩散）
    /// </summary>
    void PhaseOneAttack()
    {
        circleFireTimer -= Time.deltaTime;
        if (circleFireTimer <= 0f)
        {
            circleFireTimer = circleFireInterval;
            FireCircleBullets();
        }
    }

    /// <summary>
    /// 发射一圈子弹（用BulletObject的LineCount和LineAngle自动扩散）
    /// </summary>
    void FireCircleBullets()
    {
        if (circleBulletPool == null || circleBulletConfig == null) return;

        // 从对象池获取子弹，BulletObject的LineCount和LineAngle会自动扩散
        for (int i = 0; i < circleBulletConfig.LineCount; i++)
        {
            float angle = i * circleBulletConfig.LineAngle;
            Quaternion rotation = Quaternion.Euler(0, 0, angle);

            BulletBehav bullet = circleBulletPool.GetItem();
            if (bullet != null)
            {
                bullet.transform.position = transform.position;
                bullet.transform.rotation = rotation;
            }
        }
    }

    /// <summary>
    /// 二阶段：投炸弹 + 召唤小怪
    /// </summary>
    void PhaseTwoAttack()
    {
        // 投弹计时
        bombTimer -= Time.deltaTime;
        if (bombTimer <= 0f)
        {
            bombTimer = bombInterval;
            ThrowBomb();
        }

        // 召唤计时
        summonTimer -= Time.deltaTime;
        if (summonTimer <= 0f)
        {
            summonTimer = summonInterval;
            SummonMinions(summonCount);
        }
    }

    /// <summary>
    /// 朝玩家方向投掷炸弹
    /// </summary>
    void ThrowBomb()
    {
        if (bombPool == null || player == null) return;

        Vector2 dir = (player.position - transform.position).normalized;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

        BulletBehav bomb = bombPool.GetItem();
        if (bomb != null)
        {
            bomb.transform.position = transform.position;
            bomb.transform.rotation = Quaternion.Euler(0, 0, angle);
        }
    }

    /// <summary>
    /// 在周围随机召唤小怪
    /// </summary>
    void SummonMinions(int count)
    {
        if (minionPrefabs == null || minionPrefabs.Count == 0) return;

        for (int i = 0; i < count; i++)
        {
            GameObject prefab = minionPrefabs[Random.Range(0, minionPrefabs.Count)];
            Vector2 offset = Random.insideUnitCircle.normalized * summonRadius;
            Vector3 spawnPos = transform.position + (Vector3)offset;

            Instantiate(prefab, spawnPos, Quaternion.identity);
        }
    }

    /// <summary>
    /// 进入二阶段
    /// </summary>
    void EnterPhaseTwo()
    {
        currentPhase = Phase.Two;

        // 立刻召唤2-3个小怪
        int count = Random.Range(initialMinionCount, maxInitialMinion + 1);
        SummonMinions(count);

        bombTimer = bombInterval;
        summonTimer = summonInterval;
    }
}