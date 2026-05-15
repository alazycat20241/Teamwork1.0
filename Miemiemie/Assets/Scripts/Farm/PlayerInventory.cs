using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    public static PlayerInventory Instance;

    public int seedCount = 10;   // 初始种子数
    [Header("UI组件")]
    [SerializeField] private TextMeshProUGUI SeedText;   //显示拥有种子数量

    void Awake()
    {
        Instance = this;
        UpdateUI();
    }
    

    public bool UseSeed()
    {
        if (seedCount > 0)
        {
            seedCount--;
            UpdateUI();
            return true;
        }
        return false;
    }

    public void AddSeed(int amount)
    {
        seedCount += amount;
        UpdateUI();
    }

    public void UpdateUI()
    {
            SeedText.text = "Seed:" + seedCount;
    }
}