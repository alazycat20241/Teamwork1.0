using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    public static PlayerStats Instance { get; private set; }

    [Header("属性最小值限制")]
    public float minAttackBonus = -2f;         // 攻击力最小-2
    public float minAttackPercentBonus = -0.9f;// 攻击百分比最小-90%
    public float minRangeBonus = -3f;          // 射程加成最小-3
    public float minSpeedBonus = -0.5f;        // 射速加成最小-0.5

    [Header("永久属性（跨场景保留）")]
    public float attackBonus = 0f;
    public float attackPercentBonus = 0f;  // 攻击百分比加成（0.2 = 20%）
    public float rangeBonus = 0f;
    public float speedBonus = 0f;
    public float maxHealthBonus = 0f;
    public float panicChance = 0f;
    public float panicDuration = 0f;
    public float stoneChance = 0f;
    public float stoneDuration = 0f;
    public float missChance = 0f;

    // ========== 临时效果记录 ==========
    private float tempAttackBonus = 0f;
    private float tempAttackPercentBonus = 0f;
    private float tempRangeBonus = 0f;
    private float tempSpeedBonus = 0f;
    private float tempMaxHealthBonus = 0f;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // ================================================================
    // 永久加成（跨地图保留，不会自动还原）科技树用
    // ================================================================

    /// <summary>永久加攻击力</summary>
    public void AddPermanentAttack(float amount)
    {
        attackBonus += amount;
        attackBonus = Mathf.Max(attackBonus, minAttackBonus);  // 限制最小值
    }

    /// <summary>永久加射程</summary>
    public void AddPermanentRange(float amount)
    {
        rangeBonus += amount;
        rangeBonus = Mathf.Max(rangeBonus, minRangeBonus);    // 限制最小值
    }

    /// <summary>永久加射速</summary>
    public void AddPermanentSpeed(float amount)
    {
        speedBonus += amount;
        speedBonus = Mathf.Max(speedBonus, minSpeedBonus);    // 限制最小值
    }

    /// <summary>永久加血量上限</summary>
    public void AddPermanentMaxHealth(float amount)
    {
        maxHealthBonus += amount;
    }

    /// <summary>
    /// 记录当前值作为地图开始时的基准（进入地图时调用）
    /// </summary>
    public void SnapshotBaseline()
    {
        // 清除上次残留的临时值
        attackBonus -= tempAttackBonus;
        attackPercentBonus -= tempAttackPercentBonus;
        rangeBonus -= tempRangeBonus;
        speedBonus -= tempSpeedBonus;
        maxHealthBonus -= tempMaxHealthBonus;

        tempAttackBonus = 0f;
        tempAttackPercentBonus = 0f;
        tempRangeBonus = 0f;
        tempSpeedBonus = 0f;
        tempMaxHealthBonus = 0f;
    }

    /// <summary>
    /// 临时加攻击力（本张地图有效，ReturnToHome 时还原）
    /// </summary>
    public void AddTempAttack(float amount)
    {
        attackBonus += amount;
        attackBonus = Mathf.Max(attackBonus, minAttackBonus);  // 限制最小值
        tempAttackBonus += amount;
    }

    /// <summary>
    /// 临时加攻击百分比（本张地图有效，ReturnToHome 时还原）
    /// </summary>
    public void AddTempAttackPercent(float percent)
    {
        attackPercentBonus += percent;
        attackPercentBonus = Mathf.Max(attackPercentBonus, minAttackPercentBonus);  // 限制最小值（最小-90%）
        tempAttackPercentBonus += percent;
    }

    /// <summary>
    /// 临时加射程
    /// </summary>
    public void AddTempRange(float amount)
    {
        rangeBonus += amount;
        rangeBonus = Mathf.Max(rangeBonus, minRangeBonus);    // 限制最小值
        tempRangeBonus += amount;
    }

    /// <summary>
    /// 临时加速
    /// </summary>
    public void AddTempSpeed(float amount)
    {
        speedBonus += amount;
        speedBonus = Mathf.Max(speedBonus, minSpeedBonus);    // 限制最小值
        tempSpeedBonus += amount;
    }

    /// <summary>
    /// 临时加血量上限
    /// </summary>
    public void AddTempMaxHealth(float amount)
    {
        maxHealthBonus += amount;
        tempMaxHealthBonus += amount;
    }

    /// <summary>
    /// 还原本张地图所有临时效果（ReturnToHome 时调用）
    /// </summary>
    public void RestoreTempEffects()
    {
        attackBonus -= tempAttackBonus;
        attackPercentBonus -= tempAttackPercentBonus;
        rangeBonus -= tempRangeBonus;
        speedBonus -= tempSpeedBonus;
        maxHealthBonus -= tempMaxHealthBonus;

        tempAttackBonus = 0f;
        tempAttackPercentBonus = 0f;
        tempRangeBonus = 0f;
        tempSpeedBonus = 0f;
        tempMaxHealthBonus = 0f;

        // 清理道具效果（本张地图有效）
        stoneChance = 0f;
        stoneDuration = 0f;
        panicChance = 0f;
        panicDuration = 0f;
        missChance = 0f;
    }

    /// <summary>
    /// 新游戏时重置所有属性
    /// </summary>
    public void ResetData()
    {
        attackBonus = 0f;
        attackPercentBonus = 0f;
        rangeBonus = 0f;
        speedBonus = 0f;
        maxHealthBonus = 0f;
        panicChance = 0f;
        panicDuration = 0f;
        stoneChance = 0f;
        stoneDuration = 0f;
        missChance = 0f;

        tempAttackBonus = 0f;
        tempAttackPercentBonus = 0f;
        tempRangeBonus = 0f;
        tempSpeedBonus = 0f;
        tempMaxHealthBonus = 0f;
    }
}