using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    public static PlayerInventory Instance;

    public int seedCount = 10;   // 初始种子数
    public int playerGold = 100;
    public int HarvestCount = 0; //作物数量
    

    void Awake()
    {
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
    

    public bool UseSeed()
    {
        if (seedCount > 0)
        {
            seedCount--;
            return true;
        }
        return false;
    }

    public void AddSeed(int amount)
    {
        seedCount += amount;
    }

   
}