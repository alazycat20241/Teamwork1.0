using System.Collections.Generic;
using UnityEngine;

public class SporeDamageManager : MonoBehaviour
{
    public static SporeDamageManager Instance;                    // 单例
    [SerializeField] private float damagePerSecond = 10f;         // ★ 每秒伤害，Inspector里调

    // 目标 → 计时器（记录每个目标已经累积了多少秒，满1秒就扣血）
    private Dictionary<Health, float> targets = new Dictionary<Health, float>();

    // 目标 → 覆盖的孢子数量（多个孢子重叠时，计数+1，全部离开才停止扣血）
    private Dictionary<Health, int> targetRefCount = new Dictionary<Health, int>();

    void Awake()
    {
        Instance = this;
    }

    void Update()
    {
        // ★ 把字典转成列表再遍历，防止遍历时修改字典报错
        var targetList = new List<KeyValuePair<Health, float>>(targets);
        List<Health> toRemove = new List<Health>();               // 待清理的无效目标

        foreach (var kvp in targetList)
        {
            Health target = kvp.Key;

            // 目标没了或已死亡 → 标记移除
            if (target == null || target.IsDead)
            {
                toRemove.Add(target);
                continue;
            }

            // 计时器累加本帧时间
            float timer = kvp.Value + Time.deltaTime;

            if (timer >= 1f)
            {
                // 满1秒 → 造成伤害 → 计时器归零
                target.TakeDamage(damagePerSecond);
                targets[target] = 0f;
            }
            else
            {
                // 还没满1秒 → 更新计时器
                targets[target] = timer;
            }
        }

        // 清理已死亡或空的目标
        foreach (var t in toRemove)
        {
            targets.Remove(t);
            targetRefCount.Remove(t);
        }
    }

    /// <summary>
    /// 孢子调用：敌人进入孢子范围
    /// </summary>
    public void RegisterTarget(Health target)
    {
        if (!targetRefCount.ContainsKey(target))
        {
            // 第一个孢子覆盖 → 开始计时
            targetRefCount.Add(target, 1);
            targets.Add(target, 0f);
        }
        else
        {
            // 已经有其他孢子覆盖 → 只增加计数，不重复计时
            targetRefCount[target]++;
        }
    }

    /// <summary>
    /// 孢子调用：敌人离开孢子范围
    /// </summary>
    public void UnregisterTarget(Health target)
    {
        if (targetRefCount.ContainsKey(target))
        {
            // 减少覆盖计数
            targetRefCount[target]--;

            // 所有孢子都离开了 → 停止扣血
            if (targetRefCount[target] <= 0)
            {
                targetRefCount.Remove(target);
                targets.Remove(target);
            }
        }
    }
}