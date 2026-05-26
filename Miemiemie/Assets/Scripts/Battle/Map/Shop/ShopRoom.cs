using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 商店房间
/// 进入后展示两个随机道具，玩家二选一拾取放入道具栏
/// </summary>
public class ShopRoom : RoomBase
{
    [Header("商店UI")]
    [SerializeField] private SlidePanel shopPanel;              // 商店面板（SlidePanel组件）
    [SerializeField] private GameObject[] propSlots;            // 两个道具槽物体
    [SerializeField] private Image[] propIcons;                 // 道具图标
    [SerializeField] private TextMeshProUGUI[] propNames;       // 道具名
    [SerializeField] private TextMeshProUGUI[] propDescs;       // 道具描述

    [Header("道具栏")]
    [SerializeField] private Transform[] inventorySlots;        // 右侧6个道具栏槽位
    [SerializeField] private Image[] inventoryIcons;            // 槽位里的图标

    private List<PropData> inventory = new List<PropData>();    // 当前拥有的道具
    private PropData[] shopProps = new PropData[2];             // 本次商店展示的两个道具

    // ==================== 初始化 ====================

    public override void SetupRoom(RoomConfig config)
    {
        roomConfig = config;
        SetupExitData();  // 设置出口数据

        // 已通关则直接激活出口
        if (FixedRoomManager.Instance.IsRoomCleared(config.roomId))
        {
            ActivateExits();
            return;
        }

        // 随机选出两个道具
        GenerateShopProps();

        // 延迟一帧打开面板（等物体激活）
        StartCoroutine(OpenPanelDelayed());
    }

    /// <summary>
    /// 从道具池中随机选两个
    /// </summary>
    void GenerateShopProps()
    {
        List<PropData> allProps = PropManager.Instance.GetAllProps();
        List<PropData> pool = new List<PropData>(allProps);

        for (int i = 0; i < 2 && pool.Count > 0; i++)
        {
            int idx = Random.Range(0, pool.Count);
            shopProps[i] = pool[idx];
            pool.RemoveAt(idx);  // 不重复
        }
    }

    /// <summary>
    /// 延迟一帧打开面板，避免协程未激活报错
    /// </summary>
    IEnumerator OpenPanelDelayed()
    {
        shopPanel.gameObject.SetActive(true);
        yield return null;
        shopPanel.Open(ShowProps);
    }

    // ==================== 显示道具 ====================

    /// <summary>
    /// 面板打开后，显示两个道具
    /// </summary>
    void ShowProps()
    {
        for (int i = 0; i < 2; i++)
        {
            if (shopProps[i] != null)
            {
                propSlots[i].SetActive(true);
                propIcons[i].sprite = shopProps[i].icon;
                propNames[i].text = shopProps[i].propName;
                propDescs[i].text = shopProps[i].description;

                // 绑定点击事件
                int index = i;
                propSlots[i].GetComponent<Button>().onClick.RemoveAllListeners();
                propSlots[i].GetComponent<Button>().onClick.AddListener(() => OnPropSelected(shopProps[index]));
            }
            else
            {
                propSlots[i].SetActive(false);
            }
        }
    }

    // ==================== 选择道具 ====================

    /// <summary>
    /// 玩家点击某个道具
    /// </summary>
    void OnPropSelected(PropData prop)
    {
        // 检查道具栏是否已满
        if (inventory.Count >= 6)
        {
            Debug.Log("道具栏已满，无法拾取");
            return;
        }

        // 添加到道具栏
        AddToInventory(prop);

        // 应用道具效果
        PropManager.Instance.ApplyPropEffect(prop.propID);

        // 关闭商店面板
        shopPanel.Close();

        // 标记房间完成，激活出口
        OnRoomCompleted();
    }

    // ==================== 道具栏 ====================

    /// <summary>
    /// 添加道具到道具栏
    /// </summary>
    void AddToInventory(PropData prop)
    {
        inventory.Add(prop);
        RefreshInventoryUI();
    }

    /// <summary>
    /// 刷新道具栏UI
    /// </summary>
    void RefreshInventoryUI()
    {
        for (int i = 0; i < inventorySlots.Length; i++)
        {
            if (i < inventory.Count)
            {
                inventoryIcons[i].sprite = inventory[i].icon;
                inventorySlots[i].gameObject.SetActive(true);
            }
            else
            {
                inventorySlots[i].gameObject.SetActive(false);
            }
        }
    }
}