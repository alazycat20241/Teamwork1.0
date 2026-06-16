using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    public static PlayerStats Instance { get; private set; }

    [Header("永久属性（跨场景保留）")]
    public float attackBonus = 0f;
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
    }

    /// <summary>永久加射程</summary>
    public void AddPermanentRange(float amount)
    {
        rangeBonus += amount;
    }

    /// <summary>永久加射速</summary>
    public void AddPermanentSpeed(float amount)
    {
        speedBonus += amount;
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
        rangeBonus -= tempRangeBonus;
        speedBonus -= tempSpeedBonus;
        maxHealthBonus -= tempMaxHealthBonus;

        tempAttackBonus = 0f;
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
        tempAttackBonus += amount;
    }

    /// <summary>
    /// 临时加射程
    /// </summary>
    public void AddTempRange(float amount)
    {
        rangeBonus += amount;
        tempRangeBonus += amount;
    }

    /// <summary>
    /// 临时加速
    /// </summary>
    public void AddTempSpeed(float amount)
    {
        speedBonus += amount;
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
        rangeBonus -= tempRangeBonus;
        speedBonus -= tempSpeedBonus;
        maxHealthBonus -= tempMaxHealthBonus;

        tempAttackBonus = 0f;
        tempRangeBonus = 0f;
        tempSpeedBonus = 0f;
        tempMaxHealthBonus = 0f;
    }
}