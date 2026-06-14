using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    public static PlayerInventory Instance;

    public int seedCount = 10;   // 初始种子数
    //金币数
    public int playerGold = 100;
    public int soulStones = 0;//灵魂石数量
    public int DollCount = 1;


    [SerializeField] private TextMeshProUGUI StoneText;  //UI显示
    [SerializeField] private TextMeshProUGUI SeedText;  //UI显示
    [SerializeField] private TextMeshProUGUI GoldText;  //UI显示

    void Awake()
    {
        Instance = this;
        DontDestroyOnLoad(gameObject);
        UpdateGold();
        UpdateStone();
    }
    public void UpdateStone()
    {
        StoneText.text = "" + soulStones;
        SeedText.text = "" + seedCount;
    }
    void UpdateGold()
    {
        SeedText.text = "" + seedCount;
        GoldText.text = "" + playerGold;
    }

    public bool UseSeed()
    {
        if (seedCount > 0)
        {
            seedCount--;
            UpdateGold();
            return true;
        }
        return false;
    }

    public void AddSeed(int amount)
    {
        seedCount += amount;
        UpdateGold();
    }

    /// <summary>
    /// 加金币
    /// </summary>
    public void AddGold(int amount)
    {
        playerGold += amount;
        Debug.Log($"金币+{amount}，当前金币: {playerGold}");
    }

    /// <summary>
    /// 获取当前金币
    /// </summary>
    public int GetGold() => playerGold;

    /// <summary>
    /// 消耗灵魂石
    /// </summary>
    public void SpendSoulStones(int amount)
    {
        soulStones = Mathf.Max(0, soulStones - amount);
        Debug.Log($"消耗灵魂石: -{amount}, 剩余: {soulStones}");
    }

    /// <summary>
    /// 获取灵魂石数量
    /// </summary>
    public int GetSoulStones() => soulStones;

    /// <summary>
    /// 消耗金币
    /// </summary>
    public void SpendGold(int amount)
    {
        playerGold = Mathf.Max(0, playerGold - amount);
        Debug.Log($"消耗金币: -{amount}, 剩余: {playerGold}");
    }


    ///新开始游戏时重置一切
    public void ResetData()
    {
        seedCount = 10;
        playerGold = 0;
        soulStones = 0;
    }
}