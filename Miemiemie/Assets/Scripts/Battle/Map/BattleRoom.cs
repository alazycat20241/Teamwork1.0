using UnityEngine;
using System.Collections.Generic;

public class BattleRoom : RoomBase
{
    [Header("容器")]
    [SerializeField] private Transform enemiesContainer;

    private List<GameObject> aliveEnemies = new List<GameObject>();
    private bool battleStarted = false;

    public override void SetupRoom(RoomConfig config)
    {
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

            GameObject enemy = Instantiate(enemyInfo.enemyPrefab,
                                          enemyInfo.spawnPosition,
                                          Quaternion.identity,
                                          enemiesContainer);

            Health health = enemy.GetComponent<Health>();
            if (health != null)
            {
                health.OnDeath += () => OnEnemyDeath(enemy);
                aliveEnemies.Add(enemy);
            }
        }

        battleStarted = true;

        if (aliveEnemies.Count == 0)
        {
            OnBattleWon();
        }
    }

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


        OnRoomCompleted();
    }
}