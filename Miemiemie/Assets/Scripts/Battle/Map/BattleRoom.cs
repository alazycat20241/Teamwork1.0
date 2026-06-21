using UnityEngine;
using System.Collections.Generic;

public class BattleRoom : RoomBase
{
    [Header("容器")]
    [SerializeField] private Transform enemiesContainer;

    private List<GameObject> aliveEnemies = new List<GameObject>();
    private bool battleStarted = false;

    // 当前战斗房间实例（供道具访问）
    public static BattleRoom Current { get; private set; }

    // 战斗结束事件（用于道具判断）
    public static event System.Action OnBattleEnd;

    //战斗开始
    public static event System.Action OnBattleStart;

    public override void SetupRoom(RoomConfig config)
    {
        Current = this;  // 设置当前房间
        base.SetupRoom(config);

        if (FixedRoomManager.Instance.IsRoomCleared(config.roomId))
            return;

        SpawnEnemies();
    }

    private void SpawnEnemies()
    {
        if (roomConfig.battleSetting?.enemies == null) return;

        foreach (var enemyInfo in roomConfig.battleSetting.enemies)
        {
            if (enemyInfo.enemyPrefab == null) continue;

            GameObject enemy = Instantiate(enemyInfo.enemyPrefab, enemiesContainer);
            enemy.transform.localPosition = enemyInfo.spawnPosition;

            Health health = enemy.GetComponent<Health>();
            if (health != null)
            {
                health.OnDeath += () => OnEnemyDeath(enemy);
                aliveEnemies.Add(enemy);
            }
        }

        battleStarted = true;
        OnBattleStart?.Invoke();

        if (aliveEnemies.Count == 0)
        {
            OnBattleWon();
        }
    }

    /// <summary>
    /// 额外生成敌人（道具调用）
    /// </summary>
    public void SpawnExtraEnemy(EnemySpawnInfo info)
    {
        if (info.enemyPrefab == null) return;

        GameObject enemy = Instantiate(info.enemyPrefab, enemiesContainer);
        enemy.transform.localPosition = info.spawnPosition;

        Health health = enemy.GetComponent<Health>();
        if (health != null)
        {
            health.OnDeath += () => OnEnemyDeath(enemy);
            aliveEnemies.Add(enemy);
        }
    }

    /// <summary>
    /// 获取当前房间配置（道具调用）
    /// </summary>
    public RoomConfig GetRoomConfig() => roomConfig;


    private void OnEnemyDeath(GameObject enemy)
    {
        aliveEnemies.Remove(enemy);

        if (aliveEnemies.Count == 0 && battleStarted)
        {
            OnBattleWon();
        }
    }

    private void OnBattleWon()
    {
        battleStarted = false;

        // 掉落
        DropLoot();

        OnBattleEnd?.Invoke();
        OnRoomCompleted();
    }

    /// <summary>
    /// 外部触发战斗结束和开始（BossRoom用）
    /// </summary>
    public static void TriggerBattleEnd()
    {
        OnBattleEnd?.Invoke();
    }

    public static void TriggerBattleStart()
    {
        OnBattleStart?.Invoke();
    }

    private void DropLoot()
    {
        if (roomConfig.dropItems == null) return;

        foreach (DropItem item in roomConfig.dropItems)
        {
            // ★ 按概率随机：Random.value 返回 0~1 的随机小数
            // 比如 dropChance=0.5 → 50% 概率掉落
            if (Random.value <= item.dropChance)
            {
                // 掉几个就生成几个（比如金币掉5个）
                for (int i = 0; i < item.Amount; i++)
                {
                    // 在随机位置生成掉落物
                    Instantiate(
                        item.prefab,                 // 掉落物的预制体
                        GetRandomDropPosition(),     // 随机位置（在房间中心附近散开）
                        Quaternion.identity          // 不旋转
                    );
                }
            }
        }
    }

    private Vector3 GetRandomDropPosition()
    {
        float range = 2f;  // 在 ±2 米范围内随机

        return transform.position + new Vector3(
            Random.Range(-range, range),  // X：随机 -2 到 2
            Random.Range(-range, range),  // Y：随机 -2 到 2
            0                              // Z：0
        );
    }
}