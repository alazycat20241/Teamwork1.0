using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 事件效果执行器
/// 负责处理所有事件效果，解决 EventRoom 销毁后回调失效的问题
/// </summary>
public class EventRoomEffect : MonoBehaviour
{
    public static EventRoomEffect Instance { get; private set; }

    // ==================== 订阅清理 ====================
    private List<Action> battleEndCleanups = new List<Action>();         // 战斗结束时的还原回调
    private List<Action> roomEnteredCleanups = new List<Action>();       // 进入房间时的还原回调

    // ==================== 生命周期 ====================

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void OnDestroy()
    {
        // 还原所有"本场战斗"效果
        foreach (var action in battleEndCleanups)
        {
            action?.Invoke();                       // 执行还原
            BattleRoom.OnBattleEnd -= action;       // 取消订阅
        }
        battleEndCleanups.Clear();

        // 清理跨房间订阅
        foreach (var action in roomEnteredCleanups)
        {
            FixedRoomManager.OnRoomEntered -= action;
        }
        roomEnteredCleanups.Clear();

        if (Instance == this) Instance = null;
    }

    /// <summary>清理所有回调</summary>
    public void CleanupAll()
    {
        foreach (var action in battleEndCleanups)
            BattleRoom.OnBattleEnd -= action;
        battleEndCleanups.Clear();

        foreach (var action in roomEnteredCleanups)
            FixedRoomManager.OnRoomEntered -= action;
        roomEnteredCleanups.Clear();
    }

    // ================================================================
    // 效果分发
    // ================================================================

    /// <summary>遍历效果列表，根据类型分发到对应方法</summary>
    public void ExecuteEffects(List<EventEffect> effects)
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
        }
    }

    /// <summary>扣除血量（1心=10HP）</summary>
    void DamagePlayer(float amount)
    {
        FixedRoomManager.Instance.GetPlayer()?.GetComponent<Health>()?.TakeDamage(amount);
    }

    /// <summary>给予道具（委托给 EventRoom 生成场景物体）</summary>
    void GiveProp(EventEffect effect)
    {
        EventRoom.Current?.SpawnPropByEffect(effect);
    }

    /// <summary>设置下次受伤免疫（事件04B）</summary>
    void SetNextDamageImmune()
    {
        var health = FixedRoomManager.Instance.GetPlayer()?.GetComponent<Health>();
        if (health != null)
        {
            health.isNextDamageImmune = true;
        }
    }

    /// <summary>强制战斗，委托给 EventRoom 生成敌人</summary>
    void StartForcedBattle(EventEffect effect)
    {
        EventRoom.Current?.StartForcedBattle(effect);
    }

    /// <summary>血量上限变化（事件02A失败：-0.5心）</summary>
    void ChangeMaxHP(int amount)
    {
        var health = FixedRoomManager.Instance.GetPlayer()?.GetComponent<Health>();
        if (health == null) return;
        health.maxHealth = Mathf.Max(1, health.maxHealth + amount);
        health.currentHealth = Mathf.Clamp(health.currentHealth, 0, health.maxHealth);
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
        };
        BattleRoom.OnBattleEnd += cleanup;
        battleEndCleanups.Add(cleanup);
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
        };
        BattleRoom.OnBattleEnd += cleanup;
        battleEndCleanups.Add(cleanup);
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
        };
        BattleRoom.OnBattleEnd += cleanup;
        battleEndCleanups.Add(cleanup);
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
        };
        BattleRoom.OnBattleEnd += cleanup;
        battleEndCleanups.Add(cleanup);
    }

    // ================================================================
    // 跨房间诅咒（事件03A失败，进入下2个房间时各扣0.5心）
    // ================================================================

    /// <summary>进入下2个房间时触发扣血</summary>
    /// <param name="damageAmount">每次扣血量（5=0.5心）</param>
    void SetCurseForNextRoom(float damageAmount)
    {
        int remaining = 2;  // 固定持续2个房间

        Action onRoomEntered = null;
        onRoomEntered = () =>
        {
            remaining--;
            if (remaining <= 0)
            {
                FixedRoomManager.OnRoomEntered -= onRoomEntered;
                roomEnteredCleanups.Remove(onRoomEntered);
                return;
            }
            DamagePlayer(damageAmount);
        };

        FixedRoomManager.OnRoomEntered += onRoomEntered;
        roomEnteredCleanups.Add(onRoomEntered);
    }

    // ================================================================
    // 道具相关
    // ================================================================

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

        DropHandler target = occupiedSlots[UnityEngine.Random.Range(0, occupiedSlots.Count)];
        int removedID = target.propID;
        target.GrayOut();
        target.propID = -1;
    }
}