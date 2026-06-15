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
        Current = this;  // ★ 设置当前房间
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
            if (Random.value <= item.dropChance)
            {
                for (int i = 0; i < item.Amount; i++)
                {
                    Instantiate(item.prefab, GetRandomDropPosition(), Quaternion.identity);
                }
            }
        }
    }

    private Vector3 GetRandomDropPosition()
    {
        float range = 2f;
        return transform.position + new Vector3(Random.Range(-range, range), Random.Range(-range, range), 0);
    }
}