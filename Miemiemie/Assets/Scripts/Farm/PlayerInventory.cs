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


    // 动态查找，不加 [SerializeField]
    private TextMeshProUGUI StoneText;
    private TextMeshProUGUI SeedText;
    private TextMeshProUGUI GoldText;

    [Header("音效")]
    [SerializeField] private AudioClip dropSound;
    private AudioSource audioSource;

    void Awake()
    {
        // ===== 保持单例但允许重赋值 =====
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
            return;
        }
    }

    private void Start()
    {
        FindUIElements();
        UpdateAllUI();

        audioSource = GetComponent<AudioSource>();
    }

    /// <summary>
    /// 更新灵魂石和种子显示
    /// </summary>
    public void UpdateStone()
    {
        if (StoneText != null)
            StoneText.text = soulStones.ToString();
        if (SeedText != null)
            SeedText.text = seedCount.ToString();
    }

    /// <summary>
    /// 更新金币和种子显示
    /// </summary>
    public void UpdateGold()
    {
        if (SeedText != null)
            SeedText.text = seedCount.ToString();
        if (GoldText != null)
            GoldText.text = playerGold.ToString();
    }

    /// <summary>
    /// 动态查找 UI 引用，场景切换后调用
    /// </summary>
    public void FindUIElements()
    {
        StoneText = GameObject.Find("StoneText")?.GetComponent<TextMeshProUGUI>();
        SeedText = GameObject.Find("SeedText")?.GetComponent<TextMeshProUGUI>();
        GoldText = GameObject.Find("GoldText")?.GetComponent<TextMeshProUGUI>();
    }

    /// <summary>
    /// 刷新所有UI（场景切换后调用）
    /// </summary>
    public void RefreshAllUI()
    {
        UpdateGold();
        UpdateStone();
    }

    public void UpdateAllUI()
    {
        UpdateGold();
        UpdateStone();
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
    }

    public void AddStone(int amount)
    {
        soulStones += amount;
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
        if (audioSource != null && dropSound != null)
        {
            audioSource.PlayOneShot(dropSound);
        }
    }


    // ===== 存档/读档辅助方法 =====
    public int GetDollCount() => DollCount;

    public void SetDollCount(int count)
    {
        DollCount = count;
    }

    ///新开始游戏时重置一切
    public void ResetData()
    {
        seedCount = 10;
        playerGold = 0;
        soulStones = 0;
        DollCount = 0;
        RefreshAllUI();
    }
}