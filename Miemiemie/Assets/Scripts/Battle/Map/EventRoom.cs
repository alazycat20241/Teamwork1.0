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
    [Header("事件UI（挂在这个房间预制体上）")]
    [SerializeField] private SlidePanel eventPanel;
    [SerializeField] private TextMeshProUGUI descriptionText; // 事件描述
    [SerializeField] private Button[] choiceButtons;          // 选项按钮数组
    [SerializeField] private TextMeshProUGUI[] choiceTexts;   // 按钮文字
    [SerializeField] private TextMeshProUGUI resultText;      // 结果文字

    // 玩家属性（从战斗系统获取）
    private float currentHP;
    private float maxHP;
    private float attackBonus;
    private float speedBonus;
    private float rangeBonus;
    private bool hasFireBuff;

    private EventData eventData;

    public override void SetupRoom(RoomConfig config)
    {
        roomConfig = config;
        SetupExitData();

        if (FixedRoomManager.Instance.IsRoomCleared(config.roomId))
        {
            ActivateExits();  // 通关过才激活
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
    IEnumerator OpenPanelDelayed()
    {
        eventPanel.gameObject.SetActive(true);
        yield return null;  // 等一帧
        eventPanel.Open(OnPanelOpened);
    }
    // ==================== 读取玩家属性 ====================

    void ReadPlayerStats()
    {
        // 根据你的战斗系统，从对应管理器读取
        // 示例：
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

        // attackBonus / speedBonus / rangeBonus / hasFireBuff
        // 从你的Buff系统或玩家属性管理器读取，这里留好位置
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
        //if (choice.requiresItem && !PlayerInventory.Instance.HasAnyItem())
            //return false;

        //if (choice.requiredSoulStones > 0 && PlayerInventory.Instance.GetSoulStones() < choice.requiredSoulStones)
            //return false;

        //if (choice.requiredGold > 0 && PlayerInventory.Instance.GetGold() < choice.requiredGold)
            //return false;

        // 如果需要特定Buff
        if (!string.IsNullOrEmpty(choice.requiredBuff) && !HasBuff(choice.requiredBuff))
            return false;

        return true;
    }

    // ==================== 计算成功率 ====================

    float CalculateSuccessRate(EventChoice choice)
    {
        float rate = choice.baseRate;

        if (choice.useHPPercent && maxHP > 0)
            rate += (currentHP / maxHP) * 100f;

        if (choice.useMissingHPPercent && maxHP > 0)
            rate += ((maxHP - currentHP) / maxHP) * 40f;

        if (choice.useAttackPercent)
            rate += attackBonus * 5f;

        if (choice.useSpeedPercent)
            rate += speedBonus * 5f;

        if (choice.useFireMode && hasFireBuff)
            rate = 100f;

        return Mathf.Clamp(rate, 0f, 100f);
    }

    // ==================== 选择后 ====================
    void OnChoiceSelected(EventChoice choice)
    {

        // 消耗条件
        //if (choice.requiresItem) PlayerInventory.Instance?.RemoveRandomItem();
        //if (choice.requiredSoulStones > 0) PlayerInventory.Instance?.SpendSoulStones(choice.requiredSoulStones);
        // 掷骰
        float roll = Random.Range(0f, 100f);
        float rate = CalculateSuccessRate(choice);
        bool success = roll < rate;

        resultText.text = success ? choice.successText : choice.failText;
        Debug.Log($"成功率: {rate}, 掷骰: {roll}, 结果: {(success ? "成功" : "失败")}");

        var effects = success ? choice.successEffects : choice.failEffects;
        Debug.Log($"执行效果数量: {effects.Count}");

        ExecuteEffects(success ? choice.successEffects : choice.failEffects);

        //禁用按钮
        foreach (var btn in choiceButtons) btn.interactable = false;

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
    //            case EffectType.Heal:
    //                HealPlayer(effect.value);
    //                break;

    //            case EffectType.Damage:
    //                DamagePlayer(effect.value);
    //                break;

                case EffectType.MaxHPUp:
                    ChangeMaxHP(effect.value);
                    break;

                case EffectType.MaxHPDown:
                    ChangeMaxHP(-effect.value);
                    break;

    //            case EffectType.AttackUp:
    //                AddAttackBonus(effect.value, effect.durationRooms);
    //                break;

    //            case EffectType.AttackDown:
    //                AddAttackBonus(-effect.value, effect.durationRooms);
    //                break;

    //            case EffectType.RangeUp:
    //                AddRangeBonus(effect.value, effect.durationRooms);
    //                break;

    //            case EffectType.RangeDown:
    //                AddRangeBonus(-effect.value, effect.durationRooms);
    //                break;

    //            case EffectType.SpeedUp:
    //                AddSpeedBonus(effect.value, effect.durationRooms);
    //                break;

    //            case EffectType.SpeedDown:
    //                AddSpeedBonus(-effect.value, effect.durationRooms);
    //                break;

    //            case EffectType.AddGold:
    //                PlayerInventory.Instance?.AddGold(effect.value);
    //                break;

    //            case EffectType.LoseGold:
    //                PlayerInventory.Instance?.SpendGold(effect.value);
    //                break;

    //            case EffectType.AddItem:
    //                PlayerInventory.Instance?.AddItem(effect.extraId, effect.value);
    //                break;

    //            case EffectType.RemoveRandomItem:
    //                PlayerInventory.Instance?.RemoveRandomItem();
    //                break;

    //            case EffectType.NextDamageImmune:
    //                SetNextDamageImmune();
    //                break;

    //            case EffectType.StartBattle:
    //                // 特殊战斗：加载战斗房间
    //                FixedRoomManager.Instance.MoveToRoom(effect.extraId);
    //                break;

    //            case EffectType.CurseNextRoom:
    //                SetCurseForNextRoom(effect);
    //                break;

    //            case EffectType.LastXRooms:
    //                AddTimedBuff(effect);
    //                break;

    //            case EffectType.MissRate:
    //                SetMissRate(effect.value, effect.durationRooms);
    //                break;

    //            case EffectType.AddBuff:
    //                AddBuff(effect.extraId);
    //                break;
            }
        }
    }

    // ==================== 具体效果实现（对接你的系统） ====================

    //void HealPlayer(float amount)
    //{
    //    var player = FixedRoomManager.Instance.GetPlayer();
    //    player?.GetComponent<Health>()?.Heal(amount);
    //}

    void DamagePlayer(float amount)
    {
        var player = FixedRoomManager.Instance.GetPlayer();
        player?.GetComponent<Health>()?.TakeDamage(amount);
    }

    void ChangeMaxHP(int amount)
    {
        var player = FixedRoomManager.Instance.GetPlayer();
        var health = player?.GetComponent<Health>();
        if (health != null)
            health.maxHealth = Mathf.Max(1, health.maxHealth + amount);
        health.currentHealth = Mathf.Clamp(health.currentHealth, 0, health.maxHealth);
        Debug.Log($"血量上限变化: {amount}, 当前上限: {health.maxHealth}");
    }

    void AddAttackBonus(float amount, int rooms) { /* 对接Buff系统 */ }
    void AddRangeBonus(float amount, int rooms) { /* 对接Buff系统 */ }
    void AddSpeedBonus(float amount, int rooms) { /* 对接Buff系统 */ }
    void SetMissRate(float rate, int rooms) { /* 对接Buff系统 */ }
    void SetNextDamageImmune() { /* 对接Buff系统 */ }
    void SetCurseForNextRoom(EventEffect effect) { /* 对接Buff系统 */ }
    void AddTimedBuff(EventEffect effect) { /* 对接Buff系统 */ }
    void AddBuff(string buffId) { /* 对接Buff系统 */ }
    bool HasBuff(string buffId) { return false; /* 对接Buff系统 */ }
}