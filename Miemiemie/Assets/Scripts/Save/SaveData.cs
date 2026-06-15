using System;
using System.Collections.Generic;

[Serializable]
public class SaveData
{
    // 背包数据
    public int seedCount;
    public int playerGold;
    public int soulStones;
    public int dollCount;

    // 行动点数数据
    public int currentActionPoints;
    public int maxActionPoints;
    public int currentDay;

    public string saveTime;  // 存档时间，方便显示

    // ======= 所有田块的数据 =======
    public List<GrowBlockData> growBlockDataList = new List<GrowBlockData>();

    // 单个田块的存档数据
    [Serializable]
    public class GrowBlockData
    {
        public string blockID;              // 用于匹配是哪一块田
        public int growthStage;             // (int)GrowthStage
        public int plantDay;                // 种植时的天数
    }
}