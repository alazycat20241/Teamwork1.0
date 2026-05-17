using UnityEngine;
using System.Collections.Generic;

public class BossRoom : RoomBase
{
    [Header("Boss容器")]
    [SerializeField] private Transform enemiesContainer;

    private GameObject bossInstance;
    private bool bossDefeated = false;

    public override void SetupRoom(RoomConfig config)
    {
        roomConfig = config;
        // 如果已通关，不用再生成
        if (FixedRoomManager.Instance.IsRoomCleared(config.roomId))
            return;

        // 生成Boss
        SpawnBoss();
    }

    private void SpawnBoss()
    {
        if (roomConfig.battleSetting?.enemies == null) return;
        if (roomConfig.battleSetting.enemies.Count == 0) return;

        // Boss房间通常只有一个敌人
        EnemySpawnInfo bossInfo = roomConfig.battleSetting.enemies[0];

        if (bossInfo.enemyPrefab == null) return;

        // 生成Boss
        bossInstance = Instantiate(bossInfo.enemyPrefab,
                                   bossInfo.spawnPosition,
                                   Quaternion.identity,
                                   enemiesContainer);

        // 获取Health组件，监听死亡事件
        Health bossHealth = bossInstance.GetComponent<Health>();
        if (bossHealth != null)
        {
            bossHealth.OnDeath += OnBossDefeated;  // ← 就是这里
        }
    }

    private void OnBossDefeated()
    {
        if (bossDefeated) return;  // 防止重复触发
        bossDefeated = true;

        Debug.Log("Boss被击败！返回家园");

        // 给奖励（可选）
        //int reward = roomConfig.battleSetting.rewardGold;
        //if (PlayerInventory.Instance != null)
        //{
        //    PlayerInventory.Instance.AddGold(reward);
        //}

        // 延迟一下再返回，让玩家看到Boss死亡效果
        StartCoroutine(DelayedReturn());
    }

    private System.Collections.IEnumerator DelayedReturn()
    {
        yield return new WaitForSeconds(1.5f);  // 1.5秒后返回
        FixedRoomManager.Instance.ReturnToHome(true);  // 胜利返回
    }
}