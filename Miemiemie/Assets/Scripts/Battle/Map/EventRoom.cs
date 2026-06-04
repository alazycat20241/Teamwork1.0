using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

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

    // 玩家属性（从战斗系统获取）
    private float currentHP;
    private float maxHP;
    private float attackBonus;
    private float speedBonus;
    private float rangeBonus;
    private bool hasFireBuff;

    private EventData eventData;
    private EventChoice selectedChoice; // 记录当前选择的选项，用于延迟执行效果

    void Awake()
    {
        Current = this;
    }

    void OnDestroy()
    {
        if (Current == this) Current = null;
    }

    public override void SetupRoom(RoomConfig config)
    {
        roomConfig = config;
        SetupExitData();

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

        // 显示事件UI
        StartCoroutine(OpenPanelDelayed());
    }

    public float waitTime = 1.5f;

    IEnumerator OpenPanelDelayed()
    {
        eventPanel.gameObject.SetActive(true);
        yield return new WaitForSeconds(waitTime);
        eventPanel.Open(OnPanelOpened);
    }

    // ==================== 读取玩家属性 ====================

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

        // 从玩家属性系统读取
        if (PlayerStats.Instance != null)
        {
            attackBonus = PlayerStats.Instance.attackBonus;
            speedBonus = PlayerStats.Instance.speedBonus;
            rangeBonus = PlayerStats.Instance.rangeBonus;
        }

        // 检查是否携带灼烧模式
        hasFireBuff = HasBuff("FireMode");
    }

    // ==================== 显示事件 ====================

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

    // ==================== 检查条件 ====================

    bool CheckRequirements(EventChoice choice)
    {
        // 检查是否需要消耗道具（事件04选项B、事件08选项B）
        if (choice.requiresItem && !HasAnyProp())
            return false;

        // 检查灵魂石数量（事件03选项B）
        if (choice.requiredSoulStones > 0 && PlayerInventory.Instance.GetSoulStones() < choice.requiredSoulStones)
            return false;

        // 检查金币
        if (choice.requiredGold > 0 && PlayerInventory.Instance.GetGold() < choice.requiredGold)
            return false;

        // 检查特定Buff
        if (!string.IsNullOrEmpty(choice.requiredBuff) && !HasBuff(choice.requiredBuff))
            return false;

        return true;
    }

    // ==================== 计算成功率 ====================

    float CalculateSuccessRate(EventChoice choice)
    {
        float rate = choice.baseRate;

        // 当前血量百分比加成
        if (choice.useHPPercent && maxHP > 0)
            rate += (currentHP / maxHP) * 100f;

        // 损失血量百分比加成
        if (choice.useMissingHPPercent && maxHP > 0)
            rate += ((maxHP - currentHP) / maxHP) * 40f;

        // 攻击力加成（每点攻击+5%）
        if (choice.useAttackPercent)
            rate += attackBonus * 5f;

        // 射速加成（每点射速+5%）
        if (choice.useSpeedPercent)
            rate += speedBonus * 5f;

        // 灼烧模式 → 100%
        if (choice.useFireMode && hasFireBuff)
            rate = 100f;

        return Mathf.Clamp(rate, 0f, 100f);
    }

    // ==================== 选择后 ====================

    void OnChoiceSelected(EventChoice choice)
    {
        // 消耗条件
        if (choice.requiresItem) RemoveRandomProp();
        if (choice.requiredSoulStones > 0)
            PlayerInventory.Instance?.SpendSoulStones(choice.requiredSoulStones);
        if (choice.requiredGold > 0)
            PlayerInventory.Instance?.SpendGold(choice.requiredGold);

        // 掷骰
        float roll = Random.Range(0f, 100f);
        float rate = CalculateSuccessRate(choice);
        bool success = roll < rate;

        resultText.text = success ? choice.successText : choice.failText;
        Debug.Log($"事件掷骰 - 成功率: {rate}%, 掷骰: {roll}, 结果: {(success ? "成功" : "失败")}");

        // 隐藏描述文字和选项按钮
        descriptionText.gameObject.SetActive(false);
        foreach (var btn in choiceButtons)
        {
            btn.gameObject.SetActive(false);
        }

        // 执行效果
        var effects = success ? choice.successEffects : choice.failEffects;
        Debug.Log($"执行效果数量: {effects.Count}");
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

    // ==================== 执行效果 ====================

    void ExecuteEffects(List<EventEffect> effects)
    {
        foreach (var effect in effects)
        {
            switch (effect.effectType)
            {
                case EffectType.Heal:
                    HealPlayer(effect.value);
                    break;

                case EffectType.Damage:
                    DamagePlayer(effect.value);
                    break;

                case EffectType.MaxHPUp:
                    ChangeMaxHP(effect.value);
                    break;

                case EffectType.MaxHPDown:
                    ChangeMaxHP(-effect.value);
                    break;

                case EffectType.AttackUp:
                    AddAttackBonus(effect.value, effect.durationRooms);
                    break;

                case EffectType.AttackDown:
                    AddAttackBonus(-effect.value, effect.durationRooms);
                    break;

                case EffectType.RangeUp:
                    AddRangeBonus(effect.value, effect.durationRooms);
                    break;

                case EffectType.RangeDown:
                    AddRangeBonus(-effect.value, effect.durationRooms);
                    break;

                case EffectType.SpeedUp:
                    AddSpeedBonus(effect.value, effect.durationRooms);
                    break;

                case EffectType.SpeedDown:
                    AddSpeedBonus(-effect.value, effect.durationRooms);
                    break;

                case EffectType.AddGold:
                    PlayerInventory.Instance?.AddGold(effect.value);
                    break;

                case EffectType.LoseGold:
                    PlayerInventory.Instance?.SpendGold(effect.value);
                    break;

                case EffectType.AddItem:
                    // 如果指定了道具ID，给指定道具；否则随机给一个
                    if (!string.IsNullOrEmpty(effect.extraId) && int.TryParse(effect.extraId, out int itemId))
                    {
                        SpawnProp(itemId);
                    }
                    else
                    {
                        // 随机给一个道具
                        List<PropData> allProps = PropManager.Instance?.GetAllProps();
                        if (allProps != null && allProps.Count > 0)
                        {
                            int randomID = allProps[Random.Range(0, allProps.Count)].propID;
                            SpawnProp(randomID);
                        }

                    }
                    break;

                case EffectType.RemoveRandomItem:
                    RemoveRandomProp();
                    break;

                case EffectType.NextDamageImmune:
                    SetNextDamageImmune();
                    break;

                case EffectType.StartBattle:
                    // 强制战斗：extraId为战斗配置ID或随机小怪数量
                    StartForcedBattle(effect);
                    break;

                case EffectType.CurseNextRoom:
                    SetCurseForNextRoom(effect);
                    break;

                case EffectType.LastXRooms:
                    AddTimedBuff(effect);
                    break;

                case EffectType.MissRate:
                    SetMissRate(effect.value, effect.durationRooms);
                    break;

                case EffectType.AddBuff:
                    AddBuff(effect.extraId);
                    break;
            }
        }
    }

    // ==================== 具体效果实现 ====================

    /// <summary>
    /// 回复血量
    /// </summary>
    void HealPlayer(float amount)
    {
        var player = FixedRoomManager.Instance.GetPlayer();
        var health = player?.GetComponent<Health>();
        if (health != null)
        {
            health.currentHealth = Mathf.Min(health.currentHealth + amount, health.maxHealth);
            Debug.Log($"事件回血: +{amount}, 当前血量: {health.currentHealth}");
        }
    }

    /// <summary>
    /// 扣除血量
    /// </summary>
    void DamagePlayer(float amount)
    {
        var player = FixedRoomManager.Instance.GetPlayer();
        player?.GetComponent<Health>()?.TakeDamage(amount);
        Debug.Log($"事件扣血: -{amount}");
    }

    /// <summary>
    /// 修改血量上限（正值增加，负值减少）
    /// 参考道具12月蚀碎片的实现
    /// </summary>
    void ChangeMaxHP(int amount)
    {
        var player = FixedRoomManager.Instance.GetPlayer();
        var health = player?.GetComponent<Health>();
        if (health != null)
        {
            health.maxHealth = Mathf.Max(1, health.maxHealth + amount);
            health.currentHealth = Mathf.Clamp(health.currentHealth, 0, health.maxHealth);
            Debug.Log($"血量上限变化: {amount}, 当前上限: {health.maxHealth}");
        }
    }

    /// <summary>
    /// 修改攻击力加成
    /// 参考道具09蜂后蜜和道具11狼人指尖的实现
    /// durationRooms: -1=永久, 0=本场战斗, >0=持续N个房间
    /// </summary>
    void AddAttackBonus(float amount, int durationRooms)
    {
        if (PlayerStats.Instance == null) return;

        if (durationRooms == -1)
        {
            // 永久加成
            PlayerStats.Instance.attackBonus += amount;
            Debug.Log($"攻击力永久变化: {amount}, 当前攻击加成: {PlayerStats.Instance.attackBonus}");
        }
        else if (durationRooms == 0)
        {
            // 本场战斗加成
            PlayerStats.Instance.attackBonus += amount;
            BattleRoom.OnBattleEnd += () =>
            {
                PlayerStats.Instance.attackBonus -= amount;
                Debug.Log($"攻击力战斗加成已移除: {amount}");
            };
            Debug.Log($"攻击力本场战斗变化: {amount}");
        }
        else
        {
            // 持续N个房间加成
            PlayerStats.Instance.attackBonus += amount;
            int remaining = durationRooms;
            FixedRoomManager.OnRoomEntered += OnRoomEntered_RemoveAttackBuff;

            void OnRoomEntered_RemoveAttackBuff()
            {
                remaining--;
                if (remaining <= 0)
                {
                    PlayerStats.Instance.attackBonus -= amount;
                    FixedRoomManager.OnRoomEntered -= OnRoomEntered_RemoveAttackBuff;
                    Debug.Log($"攻击力持续加成已移除: {amount}");
                }
            }
        }
    }

    /// <summary>
    /// 修改射程加成
    /// 参考道具04萤火虫囊和道具11狼人指尖的实现
    /// </summary>
    void AddRangeBonus(float amount, int durationRooms)
    {
        if (PlayerShoot.Instance == null) return;

        if (durationRooms == -1)
        {
            // 永久加成
            PlayerShoot.Instance.AddRange(amount);
            Debug.Log($"射程永久变化: {amount}");
        }
        else if (durationRooms == 0)
        {
            // 本场战斗加成
            PlayerShoot.Instance.AddRange(amount);
            BattleRoom.OnBattleEnd += () =>
            {
                PlayerShoot.Instance.AddRange(-amount);
                Debug.Log($"射程战斗加成已移除: {amount}");
            };
            Debug.Log($"射程本场战斗变化: {amount}");
        }
        else
        {
            // 持续N个房间加成
            PlayerShoot.Instance.AddRange(amount);
            int remaining = durationRooms;
            FixedRoomManager.OnRoomEntered += OnRoomEntered_RemoveRangeBuff;

            void OnRoomEntered_RemoveRangeBuff()
            {
                remaining--;
                if (remaining <= 0)
                {
                    PlayerShoot.Instance.AddRange(-amount);
                    FixedRoomManager.OnRoomEntered -= OnRoomEntered_RemoveRangeBuff;
                    Debug.Log($"射程持续加成已移除: {amount}");
                }
            }
        }
    }

    /// <summary>
    /// 修改射速加成
    /// </summary>
    void AddSpeedBonus(float amount, int durationRooms)
    {
        if (PlayerStats.Instance == null) return;

        if (durationRooms == -1)
        {
            PlayerStats.Instance.speedBonus += amount;
            Debug.Log($"射速永久变化: {amount}");
        }
        else if (durationRooms == 0)
        {
            PlayerStats.Instance.speedBonus += amount;
            BattleRoom.OnBattleEnd += () =>
            {
                PlayerStats.Instance.speedBonus -= amount;
                Debug.Log($"射速战斗加成已移除: {amount}");
            };
        }
        else
        {
            PlayerStats.Instance.speedBonus += amount;
            int remaining = durationRooms;
            FixedRoomManager.OnRoomEntered += OnRoomEntered_RemoveSpeedBuff;

            void OnRoomEntered_RemoveSpeedBuff()
            {
                remaining--;
                if (remaining <= 0)
                {
                    PlayerStats.Instance.speedBonus -= amount;
                    FixedRoomManager.OnRoomEntered -= OnRoomEntered_RemoveSpeedBuff;
                }
            }
        }
    }

    /// <summary>
    /// 设置失误率
    /// 参考道具10石化种子的概率机制
    /// </summary>
    void SetMissRate(float rate, int durationRooms)
    {
        if (PlayerStats.Instance == null) return;

        PlayerStats.Instance.missChance = rate;

        if (durationRooms == 0)
        {
            // 本场战斗
            BattleRoom.OnBattleEnd += () =>
            {
                PlayerStats.Instance.missChance = 0f;
                Debug.Log("失误率已清除");
            };
        }
        else if (durationRooms > 0)
        {
            int remaining = durationRooms;
            FixedRoomManager.OnRoomEntered += OnRoomEntered_RemoveMiss;

            void OnRoomEntered_RemoveMiss()
            {
                remaining--;
                if (remaining <= 0)
                {
                    PlayerStats.Instance.missChance = 0f;
                    FixedRoomManager.OnRoomEntered -= OnRoomEntered_RemoveMiss;
                }
            }
        }

        Debug.Log($"设置失误率: {rate * 100}%");
    }

    /// <summary>
    /// 设置下次受伤免疫
    /// 参考道具02碎裂的护身符（但这里是免疫一次伤害，不是保留1血）
    /// </summary>
    void SetNextDamageImmune()
    {
        var player = FixedRoomManager.Instance.GetPlayer();
        var health = player?.GetComponent<Health>();
        if (health != null)
        {
            health.isNextDamageImmune = true;
            Debug.Log("下次受伤免疫已激活");
        }
    }

    /// <summary>
    /// 强制战斗
    /// 事件02选项B失败、事件04选项A失败、事件09选项B失败
    /// extraId为空则随机3只小怪
    /// </summary>
    void StartForcedBattle(EventEffect effect)
    {
        // 如果指定了战斗配置ID，直接跳转
        if (!string.IsNullOrEmpty(effect.extraId))
        {
            FixedRoomManager.Instance.MoveToRoom(effect.extraId);
            return;
        }

        // 否则在当前房间生成随机小怪
        BattleRoom room = BattleRoom.Current;
        if (room == null) return;

        // 获取可用的小怪列表
        var config = room.GetRoomConfig();
        if (config?.battleSetting?.enemies == null || config.battleSetting.enemies.Count == 0) return;

        int enemyCount = effect.value > 0 ? effect.value : 3; // 默认3只
        for (int i = 0; i < enemyCount; i++)
        {
            var info = config.battleSetting.enemies[Random.Range(0, config.battleSetting.enemies.Count)];
            room.SpawnExtraEnemy(info);
        }
        Debug.Log($"强制战斗：生成 {enemyCount} 只敌人");
    }

    /// <summary>
    /// 进入下个房间时触发诅咒效果
    /// 事件03选项A失败：进入新房间时损失0.5心，持续2轮
    /// </summary>
    void SetCurseForNextRoom(EventEffect effect)
    {
        int remaining = effect.value > 0 ? effect.value : 1; // 持续轮数
        float damageAmount = effect.durationRooms > 0 ? effect.durationRooms : 5f; // 伤害值

        FixedRoomManager.OnRoomEntered += OnRoomEntered_Curse;

        void OnRoomEntered_Curse()
        {
            remaining--;
            if (remaining <= 0)
            {
                FixedRoomManager.OnRoomEntered -= OnRoomEntered_Curse;
                return;
            }

            // 触发诅咒：扣血
            DamagePlayer(damageAmount);
            Debug.Log($"诅咒触发：进入新房间扣血 {damageAmount}，剩余 {remaining} 次");
        }
    }

    /// <summary>
    /// 持续X个房间的临时Buff
    /// 事件03选项B失败、事件06选项A失败、事件05选项A成功等
    /// </summary>
    void AddTimedBuff(EventEffect effect)
    {
        int remaining = effect.value > 0 ? effect.value : 1;

        switch (effect.durationRooms)
        {
            case 0: // 本场战斗
                ApplyTempBattleBuff(effect.extraId, effect.value);
                break;
            default: // 持续N个房间
                FixedRoomManager.OnRoomEntered += OnRoomEntered_TimedBuff;
                break;
        }

        void OnRoomEntered_TimedBuff()
        {
            remaining--;
            if (remaining <= 0)
            {
                FixedRoomManager.OnRoomEntered -= OnRoomEntered_TimedBuff;
                Debug.Log($"临时Buff '{effect.extraId}' 已过期");
            }
        }
    }

    /// <summary>
    /// 应用本场战斗临时Buff
    /// </summary>
    void ApplyTempBattleBuff(string buffType, float value)
    {
        switch (buffType)
        {
            case "MaxHP":
                ChangeMaxHP((int)value);
                BattleRoom.OnBattleEnd += () => ChangeMaxHP(-(int)value);
                break;
            case "Attack":
                PlayerStats.Instance.attackBonus += value;
                BattleRoom.OnBattleEnd += () => PlayerStats.Instance.attackBonus -= value;
                break;
            case "Range":
                PlayerShoot.Instance?.AddRange(value);
                BattleRoom.OnBattleEnd += () => PlayerShoot.Instance?.AddRange(-value);
                break;
            case "Speed":
                PlayerStats.Instance.speedBonus += value;
                BattleRoom.OnBattleEnd += () => PlayerStats.Instance.speedBonus -= value;
                break;
        }
    }

    /// <summary>
    /// 添加Buff
    /// </summary>
    void AddBuff(string buffId)
    {
        // 对接你的Buff系统
        Debug.Log($"添加Buff: {buffId}");
        // TODO: 调用Buff系统添加Buff
    }

    /// <summary>
    /// 检查是否拥有指定Buff
    /// </summary>
    bool HasBuff(string buffId)
    {
        // 对接你的Buff系统
        // TODO: 调用Buff系统检查Buff
        return false;
    }

    /// <summary>
    /// 移除随机一个道具（需要你维护当前拥有的道具列表）
    /// </summary>
    void RemoveRandomProp()
    {
        // 从PropManager获取当前拥有的道具列表
        // 如果你有维护当前道具的列表，从那里随机移除
        // 这里需要你根据自己的系统实现
        var allProps = PropManager.Instance?.GetAllProps();
        if (allProps == null || allProps.Count == 0) return;

        int randomID = allProps[Random.Range(0, allProps.Count)].propID;
        PropManager.Instance?.NotifyPropUsed(randomID);
        Debug.Log($"事件移除道具ID: {randomID}");
    }

    /// <summary>
    /// 检查是否有任意道具
    /// </summary>
    bool HasAnyProp()
    {
        // 检查道具栏是否有道具
        // 通过DropHandler检查是否有激活的道具槽
        DropHandler[] slots = FindObjectsOfType<DropHandler>();
        foreach (var slot in slots)
        {
            if (slot.propID != -1)  // 判断槽位是否有道具
                return true;
        }
        return false;
    }

    /// <summary>
    /// 在场景中生成道具物体，玩家可拖拽拾取
    /// </summary>
    void SpawnProp(int propID)
    {
        PropData propData = PropManager.Instance.GetAllProps().Find(p => p.propID == propID);
        if (propData == null) return;

        propObject.SetActive(true);
        propImage.sprite = propData.icon;
    }

    /// <summary>
    /// 道具被拖到槽位时调用
    /// </summary>
    public void OnPropDropped(GameObject draggedObj, int slotIndex)
    {
        // 应用道具效果
        if (currentPropID >= 0)
        {
            PropManager.Instance.ApplyPropEffect(currentPropID);
            currentPropID = -1;
        }

        // 隐藏道具物体
        //if (propObject != null) propObject.SetActive(false);
    }

    /// <summary>
    /// 获取被拖拽道具的ID
    /// </summary>
    public int GetDraggedPropID(GameObject draggedObj)
    {
        return currentPropID;
    }

    // 加个变量记录当前道具ID
    private int currentPropID = -1;
}