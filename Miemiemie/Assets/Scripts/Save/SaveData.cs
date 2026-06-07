using System;
using System.Collections.Generic;

[Serializable]
public class SaveData
{
    // 背包数据
    public int seedCount;
    public int playerGold;
    public int soulStones;

    // 行动点数数据
    public int currentActionPoints;
    public int maxActionPoints;
    public int currentDay;

    public string saveTime;  // 存档时间，方便显示
    // 未来可扩展的数据（示例）
    // public List<InventoryItem> items;
    // public bool[] unlockedAchievements;
}