using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

/// <summary>
/// 事件房间
/// 加载 EventData，显示文字和选项，掷骰执行效果
/// </summary>
public class EventRoom : RoomBase
{
    public static EventRoom Current { get; private set; }

    [Header("事件UI")]
    [SerializeField] private SlidePanel eventPanel;
    [SerializeField] private TextMeshProUGUI descriptionText; // 事件描述
    [SerializeField] private Button[] choiceButtons;          // 选项按钮数组
    [SerializeField] private TextMeshProUGUI[] choiceTexts;   // 按钮文字
    [SerializeField] private TextMeshProUGUI resultText;      // 结果文字

    [Header("道具展示")]
    [SerializeField] private GameObject propObject;        // 场景预放的道具物体，默认隐藏
    [SerializeField] private Image propImage;              // 道具图标

    [Header("强制战斗")]
    [SerializeField] private List<EnemySpawnInfo> commonEnemies;  // 通用小怪配置

    [Header("面板延迟")]
    public float waitTime = 1.5f;   // 面板打开延迟

    [Header("随机事件池")]
    [SerializeField] private List<EventData> eventPool;  // 所有可能的事件，拖进去

    // ==================== 玩家属性 ====================
    private float currentHP;
    private float maxHP;
    private float attackBonus;
    private float speedBonus;
    private float rangeBonus;
    private bool hasFireBuff;

    // ==================== 事件数据 ====================
    private EventData eventData;
    private int currentPropID = -1;  // 当前展示的道具ID

    // ==================== 生命周期 ====================

    void Awake()
    {
        Current = this;
    }

    void OnDestroy()
    {
        if (Current == this) Current = null;
        // 所有回调已移至 EventEffectExecutor，这里不再需要清理
    }

    public override void SetupRoom(RoomConfig config)
    {
        roomConfig = config;
        SetupExitData();

        // 已通关过则直接激活出口
        if (FixedRoomManager.Instance.IsRoomCleared(config.roomId))
        {
            ActivateExits();
            return;
        }

        // 优先用 RoomConfig 指定的，否则从池里随机
        eventData = config.customEventData;
        if (eventData == null && eventPool != null && eventPool.Count > 0)
        {
            eventData = eventPool[Random.Range(0, eventPool.Count)];
        }

        if (eventData == null)
        {
            OnRoomCompleted();
            return;
        }

        // 读取玩家属性
        ReadPlayerStats();

        // 延迟显示事件UI
        StartCoroutine(OpenPanelDelayed());
    }

    IEnumerator OpenPanelDelayed()
    {
        eventPanel.gameObject.SetActive(true);
        yield return new WaitForSeconds(waitTime);
        eventPanel.Open(OnPanelOpened);
    }

    // ==================== 读取玩家属性 ====================

    /// <summary>从玩家系统读取当前属性，用于成功率计算</summary>
    void ReadPlayerStats()
    {
        var player = FixedRoomManager.Instance.GetPlayer();
        if (player != null)
        {
            var health = player.GetComponent<Health>();
            if (health != null)
            {
                currentHP = health.CurrentHealth;
                maxHP = health.MaxHealth;
            }
        }

        if (PlayerStats.Instance != null)
        {
            attackBonus = PlayerStats.Instance.attackBonus;
            speedBonus = PlayerStats.Instance.speedBonus;
            rangeBonus = PlayerStats.Instance.rangeBonus;
        }

        hasFireBuff = HasBuff("FireMode");
    }

    // ==================== 显示事件UI ====================

    void OnPanelOpened()
    {
        // 把事件数据里的描述文字显示到面板上
        descriptionText.text = eventData.description;

        for (int i = 0; i < choiceButtons.Length; i++)
        {
            // ----- 有选项的情况 -----
            if (i < eventData.choices.Count)
            {
                var choice = eventData.choices[i];           // 取出第 i 个选项数据

                // 显示这个按钮
                choiceButtons[i].gameObject.SetActive(true);

                // 计算成功率，显示选项文字
                float rate = CalculateSuccessRate(choice);
                choiceTexts[i].text = choice.choiceText;

                // 检查是否满足选项要求，不满足按钮变灰，不可点击
                choiceButtons[i].interactable = CheckRequirements(choice);

                // 绑定点击事件（用局部变量 index 防止闭包陷阱）
                int index = i;  // 存一个局部变量，不能用 i 直接放进 Lambda
                choiceButtons[i].onClick.RemoveAllListeners();        // 先清掉旧的监听
                choiceButtons[i].onClick.AddListener(() =>            // 绑定新的
                    OnChoiceSelected(eventData.choices[index])        // 点击时调用
                );
            }
            // ----- 没有选项的情况（多余的按钮）-----
            else
            {
                // 隐藏多余的按钮
                choiceButtons[i].gameObject.SetActive(false);
            }
        }

        resultText.text = "";
    }

    // ==================== 条件检查 ====================

    /// <summary>检查玩家是否满足选项条件</summary>
    bool CheckRequirements(EventChoice choice)
    {
        // 需要消耗道具（事件04B、事件08B）
        if (choice.requiresItem && !HasAnyProp())
            return false;

        // 需要灵魂石（事件03B）
        if (choice.requiredSoulStones > 0 && PlayerInventory.Instance.GetSoulStones() < choice.requiredSoulStones)
            return false;

        // 需要金币
        if (choice.requiredGold > 0 && PlayerInventory.Instance.GetGold() < choice.requiredGold)
            return false;

        // 需要特定Buff（事件02A需灼烧模式）
        if (!string.IsNullOrEmpty(choice.requiredBuff) && !HasBuff(choice.requiredBuff))
            return false;

        return true;
    }

    // ==================== 成功率计算 ====================

    /// <summary>根据玩家属性和选项配置计算成功率</summary>
    float CalculateSuccessRate(EventChoice choice)
    {
        float rate = choice.baseRate;

        // 当前血量百分比加成（满血+100%，半血+50%）
        // 用于：事件01A、事件06A、事件07A、事件09A
        if (choice.useHPPercent && maxHP > 0)
            rate += (currentHP / maxHP) * 100f;

        // 损失血量百分比加成（半血+20%，残血+30%）
        // 用于：事件05B
        if (choice.useMissingHPPercent && maxHP > 0)
            rate += ((maxHP - currentHP) / maxHP) * 40f;

        // 攻击力加成（每点+5%）
        // 用于：事件04A、事件09B
        if (choice.useAttackPercent)
            rate += attackBonus * 5f;

        // 射速加成（每点+5%）
        // 用于：事件05A、事件08A
        if (choice.useSpeedPercent)
            rate += speedBonus * 5f;

        // 灼烧模式 → 100%
        // 用于：事件02A
        if (choice.useFireMode && hasFireBuff)
            rate = 100f;

        return Mathf.Clamp(rate, 0f, 100f);
    }

    // ==================== 选项选择 ====================

    /// <summary>玩家选择选项后执行</summary>
    void OnChoiceSelected(EventChoice choice)
    {
        // 先消耗条件资源
        if (choice.requiresItem) RemoveRandomProp();
        if (choice.requiredSoulStones > 0)
            PlayerInventory.Instance?.SpendSoulStones(choice.requiredSoulStones);
        if (choice.requiredGold > 0)
            PlayerInventory.Instance?.SpendGold(choice.requiredGold);

        // 掷骰判定
        float roll = Random.Range(0f, 100f);
        float rate = CalculateSuccessRate(choice);
        bool success = roll < rate;

        resultText.text = success ? choice.successText : choice.failText;

        // 隐藏描述和选项，只显示结果文字
        descriptionText.gameObject.SetActive(false);
        foreach (var btn in choiceButtons)
            btn.gameObject.SetActive(false);

        // 执行成功或失败的效果列表（委托给持久化执行器）
        var effects = success ? choice.successEffects : choice.failEffects;
        EventRoomEffect.Instance?.ExecuteEffects(effects);

        // 延迟关闭面板
        StartCoroutine(CloseAfterDelay(1f));
    }

    IEnumerator CloseAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        eventPanel.Close();
        OnRoomCompleted();
    }

    // ================================================================
    // 场景相关方法（需要场景物体，由 EventEffectExecutor 回调）
    // ================================================================

    /// <summary>给予道具（extraId指定则给指定道具，否则随机）</summary>
    public void SpawnPropByEffect(EventEffect effect)
    {
        if (!string.IsNullOrEmpty(effect.extraId) && int.TryParse(effect.extraId, out int itemId))
        {
            // 指定道具ID（如事件07C：灵魂花种子）
            SpawnProp(itemId);
        }
        else
        {
            // 随机给一个道具
            var allProps = PropManager.Instance?.GetAllProps();
            if (allProps != null && allProps.Count > 0)
                SpawnProp(allProps[Random.Range(0, allProps.Count)].propID);
        }
    }

    /// <summary>强制战斗，在玩家周围生成小怪</summary>
    public void StartForcedBattle(EventEffect effect)
    {
        if (commonEnemies == null || commonEnemies.Count == 0) return;

        int enemyCount = (int)effect.value;
        for (int i = 0; i < enemyCount; i++)
        {
            var info = commonEnemies[Random.Range(0, commonEnemies.Count)];
            Instantiate(info.enemyPrefab, GetRandomSpawnPos(), Quaternion.identity);
        }
    }

    /// <summary>玩家周围随机位置（距离2-4格）</summary>
    Vector3 GetRandomSpawnPos()
    {
        Vector3 playerPos = FixedRoomManager.Instance.GetPlayer().transform.position;
        float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
        float dist = Random.Range(2f, 4f);
        return playerPos + new Vector3(Mathf.Cos(angle) * dist, Mathf.Sin(angle) * dist, 0);
    }

    // ================================================================
    // 道具相关
    // ================================================================

    /// <summary>在场景中展示道具物体，玩家可拖拽拾取</summary>
    void SpawnProp(int propID)
    {
        PropData propData = PropManager.Instance.GetAllProps().Find(p => p.propID == propID);
        if (propData == null) return;

        currentPropID = propID;
        propObject.SetActive(true);
        propImage.sprite = propData.icon;
        propObject.GetComponent<DragHandler>().propData = propData;
    }

    /// <summary>道具被拖到槽位时调用（由DropHandler触发）</summary>
    public void OnPropDropped(GameObject draggedObj, int slotIndex)
    {
        if (currentPropID >= 0)
        {
            PropManager.Instance.ApplyPropEffect(currentPropID);
            currentPropID = -1;
        }
    }

    /// <summary>获取被拖拽道具的ID（由DropHandler调用）</summary>
    public int GetDraggedPropID(GameObject draggedObj)
    {
        return currentPropID;
    }

    /// <summary>移除随机一个已拥有的道具（事件04B、事件08B消耗）</summary>
    void RemoveRandomProp()
    {
        DropHandler[] slots = FindObjectsOfType<DropHandler>();
        List<DropHandler> occupiedSlots = new List<DropHandler>();

        foreach (var slot in slots)
        {
            if (slot.propID != -1)
                occupiedSlots.Add(slot);
        }

        if (occupiedSlots.Count == 0) return;

        DropHandler target = occupiedSlots[Random.Range(0, occupiedSlots.Count)];
        int removedID = target.propID;
        target.GrayOut();
        target.propID = -1;
    }

    /// <summary>检查玩家是否有任意道具</summary>
    bool HasAnyProp()
    {
        DropHandler[] slots = FindObjectsOfType<DropHandler>();
        foreach (var slot in slots)
        {
            if (slot.propID != -1)
                return true;
        }
        return false;
    }

    /// <summary>检查是否拥有指定Buff</summary>
    bool HasBuff(string buffId)
    {
        //没做到，先不管
        return false;
    }
}