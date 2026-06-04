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

    [Header("事件UI（挂在这个房间预制体上）")]
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

    // ==================== 订阅清理 ====================
    private List<Action> battleEndCleanups = new List<Action>();         // 战斗结束时的还原回调
    private List<Action> roomEnteredCleanups = new List<Action>();       // 进入房间时的还原回调

    // ==================== 生命周期 ====================

    void Awake()
    {
        Current = this;
    }

    void OnDestroy()
    {
        if (Current == this) Current = null;

        // 清理本场战斗的订阅
        foreach (var action in battleEndCleanups)
            BattleRoom.OnBattleEnd -= action;
        battleEndCleanups.Clear();

        // 清理跨房间的订阅
        foreach (var action in roomEnteredCleanups)
            FixedRoomManager.OnRoomEntered -= action;
        roomEnteredCleanups.Clear();
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

        eventData = config.customEventData;
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
        descriptionText.text = eventData.description;

        for (int i = 0; i < choiceButtons.Length; i++)
        {
            if (i < eventData.choices.Count)
            {
                var choice = eventData.choices[i];
                choiceButtons[i].gameObject.SetActive(true);
                float rate = CalculateSuccessRate(choice);
                choiceTexts[i].text = $"{choice.choiceText}\n( {rate:F0}%)";
                choiceButtons[i].interactable = CheckRequirements(choice);

                int index = i;
                choiceButtons[i].onClick.RemoveAllListeners();
                choiceButtons[i].onClick.AddListener(() => OnChoiceSelected(eventData.choices[index]));
            }
            else
            {
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
        Debug.Log($"事件掷骰 - 成功率: {rate}%, 掷骰: {roll}, 结果: {(success ? "成功" : "失败")}");

        // 隐藏描述和选项，只显示结果文字
        descriptionText.gameObject.SetActive(false);
        foreach (var btn in choiceButtons)
            btn.gameObject.SetActive(false);

        // 执行成功或失败的效果列表
        var effects = success ? choice.successEffects : choice.failEffects;
        ExecuteEffects(effects);

        // 延迟关闭面板
        StartCoroutine(CloseAfterDelay(2f));
    }

    IEnumerator CloseAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        eventPanel.Close();
        OnRoomCompleted();
    }

    // ================================================================
    // 效果分发
    // ================================================================

    /// <summary>遍历效果列表，根据类型分发到对应方法</summary>
    void ExecuteEffects(List<EventEffect> effects)
    {
        foreach (var effect in effects)
        {
            switch (effect.effectType)
            {
                // ========== 即时效果 ==========
                case EffectType.Heal:
                    HealPlayer(effect.value);
                    break;

                case EffectType.Damage:
                    DamagePlayer(effect.value);
                    break;

                case EffectType.AddGold:
                    PlayerInventory.Instance?.AddGold((int)effect.value);
                    break;

                case EffectType.LoseGold:
                    PlayerInventory.Instance?.SpendGold((int)effect.value);
                    break;

                case EffectType.AddItem:
                    GiveProp(effect);
                    break;

                case EffectType.RemoveRandomItem:
                    RemoveRandomProp();
                    break;

                case EffectType.NextDamageImmune:
                    SetNextDamageImmune();
                    break;

                case EffectType.StartBattle:
                    StartForcedBattle(effect);
                    break;

                // ========== 血量上限 ==========
                case EffectType.MaxHPUp:
                    ChangeMaxHP((int)effect.value);
                    break;

                case EffectType.MaxHPDown:
                    ChangeMaxHP(-(int)effect.value);
                    break;

                // ========== 攻击力（本场战斗，战斗结束后自动还原） ==========
                case EffectType.AttackUp:
                    AddAttackThisBattle(effect.value);
                    break;

                case EffectType.AttackDown:
                    AddAttackThisBattle(-effect.value);
                    break;

                // ========== 射程（本场战斗，战斗结束后自动还原） ==========
                case EffectType.RangeUp:
                    AddRangeThisBattle(effect.value);
                    break;

                case EffectType.RangeDown:
                    AddRangeThisBattle(-effect.value);
                    break;

                // ========== 射速（本场战斗，战斗结束后自动还原） ==========
                case EffectType.SpeedUp:
                    AddSpeedThisBattle(effect.value);
                    break;

                case EffectType.SpeedDown:
                    AddSpeedThisBattle(-effect.value);
                    break;

                // ========== 失误率（本场战斗，战斗结束后清除） ==========
                case EffectType.MissRate:
                    SetMissRate(effect.value);
                    break;

                // ========== 跨房间诅咒（事件03A失败，进下2个房间扣血） ==========
                case EffectType.CurseNextRoom:
                    SetCurseForNextRoom(effect.value);
                    break;

                // ========== Buff ==========
                case EffectType.AddBuff:
                    AddBuff(effect.extraId);
                    break;
            }
        }
    }

    // ================================================================
    // 即时效果
    // ================================================================

    /// <summary>回复血量</summary>
    void HealPlayer(float amount)
    {
        var health = FixedRoomManager.Instance.GetPlayer()?.GetComponent<Health>();
        if (health != null)
        {
            health.currentHealth = Mathf.Min(health.currentHealth + amount, health.maxHealth);
            Debug.Log($"事件回血: +{amount}");
        }
    }

    /// <summary>扣除血量（1心=10HP）</summary>
    void DamagePlayer(float amount)
    {
        FixedRoomManager.Instance.GetPlayer()?.GetComponent<Health>()?.TakeDamage(amount);
        Debug.Log($"事件扣血: -{amount}");
    }

    /// <summary>给予道具（extraId指定则给指定道具，否则随机）</summary>
    void GiveProp(EventEffect effect)
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

    /// <summary>设置下次受伤免疫（事件04B）</summary>
    void SetNextDamageImmune()
    {
        var health = FixedRoomManager.Instance.GetPlayer()?.GetComponent<Health>();
        if (health != null)
        {
            health.isNextDamageImmune = true;
            Debug.Log("下次受伤免疫已激活");
        }
    }

    /// <summary>强制战斗，在玩家周围生成小怪</summary>
    void StartForcedBattle(EventEffect effect)
    {
        if (commonEnemies == null || commonEnemies.Count == 0) return;

        int enemyCount = (int)effect.value;
        for (int i = 0; i < enemyCount; i++)
        {
            var info = commonEnemies[Random.Range(0, commonEnemies.Count)];
            Instantiate(info.enemyPrefab, GetRandomSpawnPos(), Quaternion.identity);
        }
        Debug.Log($"强制战斗：生成 {enemyCount} 只敌人");
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
    // 血量上限（永久改变，不还原）
    // ================================================================

    /// <summary>血量上限永久变化（事件02A失败：-0.5心）</summary>
    void ChangeMaxHP(int amount)
    {
        var health = FixedRoomManager.Instance.GetPlayer()?.GetComponent<Health>();
        if (health == null) return;
        health.maxHealth = Mathf.Max(1, health.maxHealth + amount);
        health.currentHealth = Mathf.Clamp(health.currentHealth, 0, health.maxHealth);
        Debug.Log($"血量上限永久变化: {amount}");
    }

    // ================================================================
    // 攻击力（本场战斗，战斗结束后自动还原）
    // ================================================================

    /// <summary>攻击力本场战斗变化（事件03A成功+20%，事件06A成功+50%，事件06A失败-50%，事件10B+20%）</summary>
    void AddAttackThisBattle(float amount)
    {
        if (PlayerStats.Instance == null) return;
        PlayerStats.Instance.attackBonus += amount;

        // 注册战斗结束还原回调
        Action cleanup = () =>
        {
            PlayerStats.Instance.attackBonus -= amount;
            Debug.Log($"攻击力战斗加成已还原: {-amount}");
        };
        BattleRoom.OnBattleEnd += cleanup;
        battleEndCleanups.Add(cleanup);

        Debug.Log($"攻击力本场战斗变化: {amount}");
    }

    // ================================================================
    // 射程（本场战斗，战斗结束后自动还原）
    // ================================================================

    /// <summary>射程本场战斗变化（事件05B成功+1，事件07A失败-0.5，事件08A失败-0.5，事件09A失败-1，事件09B成功-1）</summary>
    void AddRangeThisBattle(float amount)
    {
        if (PlayerShoot.Instance == null) return;
        PlayerShoot.Instance.AddRange(amount);

        // 注册战斗结束还原回调
        Action cleanup = () =>
        {
            PlayerShoot.Instance.AddRange(-amount);
            Debug.Log($"射程战斗加成已还原: {-amount}");
        };
        BattleRoom.OnBattleEnd += cleanup;
        battleEndCleanups.Add(cleanup);

        Debug.Log($"射程本场战斗变化: {amount}");
    }

    // ================================================================
    // 射速（本场战斗，战斗结束后自动还原）
    // ================================================================

    /// <summary>射速本场战斗变化（事件05B成功+0.5，事件06C-0.5，事件10A失败-0.5）</summary>
    void AddSpeedThisBattle(float amount)
    {
        if (PlayerStats.Instance == null) return;
        PlayerStats.Instance.speedBonus += amount;

        // 注册战斗结束还原回调
        Action cleanup = () =>
        {
            PlayerStats.Instance.speedBonus -= amount;
            Debug.Log($"射速战斗加成已还原: {-amount}");
        };
        BattleRoom.OnBattleEnd += cleanup;
        battleEndCleanups.Add(cleanup);

        Debug.Log($"射速本场战斗变化: {amount}");
    }

    // ================================================================
    // 失误率（本场战斗，战斗结束后清除）
    // ================================================================

    /// <summary>设置失误率，本场战斗有效（事件05A失败：10%）</summary>
    void SetMissRate(float rate)
    {
        if (PlayerStats.Instance == null) return;
        PlayerStats.Instance.missChance = rate;

        // 注册战斗结束清除回调
        Action cleanup = () =>
        {
            PlayerStats.Instance.missChance = 0f;
            Debug.Log("失误率已清除");
        };
        BattleRoom.OnBattleEnd += cleanup;
        battleEndCleanups.Add(cleanup);

        Debug.Log($"设置失误率: {rate * 100}%");
    }

    // ================================================================
    // 跨房间诅咒（事件03A失败，进入下2个房间时各扣0.5心）
    // ================================================================

    /// <summary>进入下2个房间时触发扣血</summary>
    /// <param name="damageAmount">每次扣血量（5=0.5心）</param>
    void SetCurseForNextRoom(float damageAmount)
    {
        int remaining = 2;  // 固定持续2个房间

        FixedRoomManager.OnRoomEntered += OnRoomEntered;
        void OnRoomEntered()
        {
            remaining--;
            if (remaining <= 0)
            {
                FixedRoomManager.OnRoomEntered -= OnRoomEntered;
                roomEnteredCleanups.Remove(OnRoomEntered);
                return;
            }
            DamagePlayer(damageAmount);
            Debug.Log($"诅咒触发：扣血 {damageAmount}，剩余 {remaining} 次");
        }
        // 注册清理回调（房间销毁时兜底取消订阅）
        roomEnteredCleanups.Add(OnRoomEntered);
    }

    // ================================================================
    // Buff系统（预留接口）
    // ================================================================

    /// <summary>添加Buff</summary>
    void AddBuff(string buffId)
    {
        Debug.Log($"添加Buff: {buffId}");
    }

    /// <summary>检查是否拥有指定Buff</summary>
    bool HasBuff(string buffId)
    {
        // TODO: 对接Buff系统
        return false;
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
        Debug.Log($"事件生成道具: {propData.propName}");
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
        Debug.Log($"事件移除道具ID: {removedID}");
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
}