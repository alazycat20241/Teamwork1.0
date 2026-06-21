using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 商店房间
/// 场景中展示两个道具，玩家拖拽一个到右侧道具栏拾取
/// </summary>
public class ShopRoom : RoomBase
{
    public static ShopRoom Instance { get; private set; }

    [Header("道具展示（场景中的两个道具物体）")]
    [SerializeField] private GameObject[] propObjects;          // 道具物体（挂DragHandler）
    [SerializeField] private Image[] propImages;                // 道具图标

    [Header("道具栏（右侧6个槽位）")]
    [SerializeField] private Transform[] inventorySlots;        // 槽位（挂DropHandler）

    private List<PropData> inventory = new List<PropData>();    // 当前拥有的道具
    private PropData[] shopProps = new PropData[2];             // 本次商店的两个道具
    private bool picked = false;                                // 是否已拾取


    public override void SetupRoom(RoomConfig config)
    {
        Instance = this;
        roomConfig = config;
        SetupExitData();

        // 随机选出两个道具并展示
        GenerateShopProps();

        ShowProps();

        ActivateExits();
    }

    /// <summary>
    /// 从道具池中随机选两个不重复的道具
    /// </summary>
    void GenerateShopProps()
    {
        List<PropData> allProps = PropManager.Instance.GetAllProps();
        List<PropData> pool = new List<PropData>(allProps);//复制一份道具列表，防止后续操作改到原始数据

        for (int i = 0; i < 2 && pool.Count > 0; i++)
        {
            int idx = Random.Range(0, pool.Count);
            shopProps[i] = pool[idx];
            pool.RemoveAt(idx);//从复制列表里移除
        }
    }

    /// <summary>
    /// 展示两个道具的图标和名字
    /// </summary>
    void ShowProps()
    {
        for (int i = 0; i < 2; i++)
        {
            if (shopProps[i] != null)
            {
                propObjects[i].SetActive(true);
                propImages[i].sprite = shopProps[i].icon;
                propObjects[i].GetComponent<DragHandler>().propData = shopProps[i];
            }
            else
            {
                propObjects[i].SetActive(false);
            }
        }

        // ===== 额外生成一个掉落物 =====
        if (roomConfig.dropItems != null && roomConfig.dropItems.Length > 0)
        {
            float totalWeight = 0f;
            foreach (var drop in roomConfig.dropItems)
                totalWeight += drop.dropChance;

            float roll = Random.Range(0f, totalWeight);
            float cumulative = 0f;

            foreach (var drop in roomConfig.dropItems)
            {
                cumulative += drop.dropChance;
                if (roll <= cumulative)
                {
                    Instantiate(drop.prefab, new Vector3(0, 1.2f, 0), Quaternion.identity, transform);
                    break;
                }
            }
        }
    }


    /// <summary>
    /// 道具被拖到道具栏槽位时调用（由DropHandler触发）
    /// </summary>
    /// <param name="draggedObj">被拖拽的道具物体</param>
    /// <param name="slotIndex">放入的槽位编号（0-5）</param>
    public void OnPropDropped(GameObject draggedObj, int slotIndex)
    {
        // 只能拾取一次
        if (picked) return;
        picked = true;

        // 判断拖的是哪个道具（0或1）
        int propIndex = draggedObj == propObjects[0] ? 0 : 1;
        PropData prop = shopProps[propIndex];

        // 记录到道具背包
        inventory.Add(prop);

        // 隐藏另一个道具
        propObjects[1 - propIndex].SetActive(false);

        // 应用道具效果
        PropManager.Instance.ApplyPropEffect(prop.propID);

        // 标记房间完成，激活出口
        OnRoomCompleted();
    }


    /// <summary>
    /// 测试用：固定出现指定道具
    /// 在 SetupRoom 里调用 TestSetProps(道具ID1, 道具ID2) 即可
    /// </summary>
    void TestSetProps(int id1, int id2)
    {
        List<PropData> allProps = PropManager.Instance.GetAllProps();
        foreach (var p in allProps)
        {
            if (p.propID == id1) shopProps[0] = p;
            if (p.propID == id2) shopProps[1] = p;
        }
        ShowProps();
    }

    /// 根据拖拽的物体判断是哪个道具，返回道具ID
    /// </summary>
    /// <param name="draggedObj">被拖拽的道具物体</param>
    /// <returns>道具ID（1-12）</returns>
    public int GetDraggedPropID(GameObject draggedObj)
    {
        // 判断拖的是左边还是右边的道具
        int propIndex = draggedObj == propObjects[0] ? 0 : 1;
        // 返回对应道具的ID
        return shopProps[propIndex].propID;
    }
}