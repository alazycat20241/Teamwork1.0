using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewEvent", menuName = "Game/Event Data")]
public class EventData : ScriptableObject
{
    [Header("基本信息")]
    public string eventName;              // 事件名
    [TextArea(3, 5)]
    public string description;            // 事件描述文字

    [Header("选项")]
    public List<EventChoice> choices = new List<EventChoice>();
}

[Serializable]
public class EventChoice
{
    public string choiceText;

    [Header("条件")]
    public bool requiresItem;             // 是否需要消耗道具
    public int requiredSoulStones;        // 需要灵魂石数量
    public int requiredGold;              // 需要金币
    public string requiredBuff;           // 需要的Buff ID

    [Header("成功率")]
    public float baseRate;                // 基础成功率 %
    public bool useHPPercent;             // 使用当前血量/最大血量
    public bool useMissingHPPercent;      // 使用(最大-当前)/最大血量
    public bool useAttackPercent;         // 使用攻击力×5%
    public bool useSpeedPercent;          // 使用射速×5%
    public bool useFireMode;              // 携带灼烧模式=100%

    [Header("成功")]
    public string successText;
    public List<EventEffect> successEffects = new List<EventEffect>();

    [Header("失败")]
    public string failText;
    public List<EventEffect> failEffects = new List<EventEffect>();
}

[Serializable]
public class EventEffect
{
    public EffectType effectType;
    public int value;
    public string extraId;
    public int durationRooms;   // 持续几个房间，0=本次战斗，-1=永久
}

public enum EffectType
{
    None,
    Heal,               // 回血
    Damage,             // 扣血
    MaxHPUp,            // 血量上限+value
    MaxHPDown,          // 血量上限-value
    AttackUp,           // 攻击+value%
    AttackDown,         // 攻击-value%
    RangeUp,            // 射程+value
    RangeDown,          // 射程-value
    SpeedUp,            // 射速+value
    SpeedDown,          // 射速-value
    AddGold,            // 金币+value
    LoseGold,           // 金币-value
    AddItem,            // 获得物品 extraId
    RemoveRandomItem,   // 消耗随机一个道具
    AddBuff,            // 添加Buff extraId
    NextDamageImmune,   // 下次受伤免疫
    MissRate,           // 失误率 value%
    StartBattle,        // 强制战斗 extraId=配置
    CurseNextRoom,      // 进入下个房间触发 effect
    LastXRooms,         // 持续X个房间的效果
}