using Spine.Unity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 道具管理器
/// 管理所有道具数据和效果
/// 所有"永久有效"均为本张地图内有效，返回Home时自动还原
/// </summary>
public class PropManager : MonoBehaviour
{
    public static PropManager Instance { get; private set; }

    [Header("所有道具")]
    [SerializeField] private List<PropData> allProps = new List<PropData>();  // 12个拖进来

    // ==================== 计数 ====================
    private int kuMuCounter = 0;          // 道具01：枯木逢春剩余次数
    private bool huShenFuActive = false;  // 道具02：护身符是否激活
    private bool xueTaiActive = false;    // 道具07：血苔绷带是否激活

    void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }
    }

    /// <summary>
    /// 清理所有道具效果（在返回家园时调用）
    /// </summary>
    public void CleanupAllProps()
    {
        // 道具01：枯木逢春
        BattleRoom.OnBattleEnd -= OnBattleEnd_Heal;
        kuMuCounter = 0;
        
        // 道具02：护身符
        huShenFuActive = false;
        
        // 道具03：鼹鼠牙齿
        BattleRoom.OnBattleEnd -= OnBattleEnd_ExtraGold;
        
        // 道具04：萤火虫囊 - 由 PlayerStats.RestoreTempEffects() 处理
        
        // 道具05：腐烂号角
        BattleRoom.OnBattleStart -= OnBattleStart_StopEnemies;
        
        // 道具06：蜕皮壳 - 一次性使用，已在使用时取消订阅
        
        // 道具07：血苔绷带
        FixedRoomManager.OnRoomEntered -= OnRoomEntered_XueTai;
        xueTaiActive = false;
        
        // 道具08：枯叶斗篷
        douPengActive = false;
        isCamouflaged = false;
        standStillTimer = 0f;
        // 恢复玩家可见性
        SetPlayerVisible(true);
        
        // 道具09：蜂后蜜
        BattleRoom.OnBattleStart -= OnBattleStart_SpawnExtraEnemies;
        
        // 道具10：石化种子 - 由 PlayerStats.RestoreTempEffects() 处理
        
        // 道具11：狼人指尖 - 由 PlayerStats.RestoreTempEffects() 处理
        
        // 道具12：月蚀碎片 - 一次性使用，已在使用时取消订阅
    }

    /// <summary>
    /// 获取所有道具列表
    /// </summary>
    public List<PropData> GetAllProps() => allProps;

    /// <summary>
    /// 根据ID应用道具效果
    /// </summary>
    public void ApplyPropEffect(int propID)
    {
        switch (propID)
        {
            case 1: KuMuFengChun(); break;      // 枯木逢春
            case 2: SuiLieHuShenFu(); break;     // 碎裂的护身符
            case 3: YanShuYaChi(); break;        // 鼹鼠牙齿(查看金币
            case 4: YingHuoChongNang(); break;   // 萤火虫囊
            case 5: FuLanHaoJiao(); break;       // 腐烂号角
            case 6: TuiPiKe(); break;            // 蜕皮壳
            case 7: XueTaiBengDai(); break;      // 血苔绷带
            case 8: KuYeDouPeng(); break;        // 枯叶斗篷
            case 9: FengHouMi(); break;          // 蜂后蜜
            case 10: ShiHuaZhongZi(); break;     // 石化种子
            case 11: LangRenZhiJian(); break;    // 狼人指尖
            case 12: YueShiSuiPian(); break;     // 月蚀碎片
        }
    }

    /// <summary>
    /// 通知道具已用完，将对应槽位图标变灰
    /// </summary>
    /// <param name="propID">用完的道具ID</param>
    public void NotifyPropUsed(int propID)
    {
        // 找到场景里所有道具栏槽位
        DropHandler[] slots = FindObjectsOfType<DropHandler>();
        foreach (var slot in slots)
        {
            // 匹配道具ID
            if (slot.propID == propID)
            {
                // 变灰
                slot.GrayOut();
                return;
            }
        }
    }


    // ==================== 道具01：枯木逢春 ====================
    // 每次战斗结束时恢复5HP（0.5心），生效3次后枯萎消失

    private void KuMuFengChun()
    {
        kuMuCounter = 3;
        BattleRoom.OnBattleEnd += OnBattleEnd_Heal;
    }

    private void OnBattleEnd_Heal()
    {
        if (kuMuCounter <= 0) return;

        var health = FixedRoomManager.Instance.GetPlayer()?.GetComponent<Health>();
        if (health != null)
        {
            health.currentHealth = Mathf.Min(health.currentHealth + 5, health.maxHealth);
            kuMuCounter--;
        }

        if (kuMuCounter <= 0)
        {
            BattleRoom.OnBattleEnd -= OnBattleEnd_Heal;
            NotifyPropUsed(1);  
        }
    }

    // ==================== 道具02：碎裂的护身符 ====================
    // 受到致命伤害时保留5HP（0.5心）不死，随后护身符破碎消失（一次性）
    private void SuiLieHuShenFu()
    {
        huShenFuActive = true;// 标记护身符已激活
    }

    public bool TryUseHuShenFu()//锁血，health里调用
    {
        if (!huShenFuActive) return false;
        huShenFuActive = false;
        NotifyPropUsed(2); 
        return true;
    }

    // ==================== 道具03：鼹鼠牙齿 ====================
    // 每次战斗额外掉落2金币，本张地图有效

    private void YanShuYaChi()
    {
        BattleRoom.OnBattleEnd += OnBattleEnd_ExtraGold;
    }

    private void OnBattleEnd_ExtraGold()
    {
        PlayerInventory.Instance?.AddGold(2);
    }

    // ==================== 道具04：萤火虫囊 ====================
    // 射程+1，本张地图有效

    private void YingHuoChongNang()
    {
        if (PlayerShoot.Instance != null)
            PlayerShoot.Instance.AddRange(1);
    }

    // ==================== 道具05：腐烂号角 ====================
    // 每场战斗开始时，所有敌人停止移动3秒，本张地图有效

    /// 应用道具效果：订阅战斗开始事件
    private void FuLanHaoJiao()
    {
        BattleRoom.OnBattleStart += OnBattleStart_StopEnemies;
    }

    /// 战斗开始时：遍历所有敌人，暂停移动3秒后恢复
    private void OnBattleStart_StopEnemies()
    {
        // 检查自身是否已被销毁
        if (this == null || gameObject == null) return;

        // 找到场景里所有带Enemy标签的物体
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        foreach (GameObject enemy in enemies)
        {
            // 获取可移动接口
            IMovable movable = enemy.GetComponent<IMovable>();
            if (movable != null)
            {
                // 暂停移动
                movable.PauseMovement();

                // 3秒后恢复移动
                StartCoroutine(ResumeEnemyAfterDelay(movable, 3f));
            }
        }
    }

    /// <summary>
    /// 延迟指定秒数后恢复敌人移动
    /// </summary>
    /// <param name="movable">敌人移动接口</param>
    /// <param name="delay">延迟秒数</param>
    IEnumerator ResumeEnemyAfterDelay(IMovable movable, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (movable != null)
            movable.ResumeMovement();
    }

    // ==================== 道具06：蜕皮壳 ====================
    // 获得1次庇护（阻挡任意一次攻击），生效后破损
    // 立刻在自身3格范围内生成毒气圈，维持2秒，1秒2伤（对自身无伤，只对敌）

    private bool tuiPiKeActive = false;     // 护盾是否激活
    private bool tuiPiKeBroken = false;     // 护盾是否已破损
    [Header("毒气孢子")]
    [SerializeField] private GameObject gasSporePrefab;  // 毒气孢子预制体

    /// <summary>
    /// 应用道具：激活护盾，订阅受伤事件
    /// </summary>
    private void TuiPiKe()
    {
        tuiPiKeActive = true;
        tuiPiKeBroken = false;

        var health = FixedRoomManager.Instance.GetPlayer()?.GetComponent<Health>();
        if (health != null)
            health.OnDamaged += CheckTuiPiKe;
    }

    /// <summary>
    /// 受伤时检查护盾
    /// </summary>
    private void CheckTuiPiKe(float damage)
    {
        if (!tuiPiKeActive || tuiPiKeBroken) return;

        // 阻挡这次伤害
        tuiPiKeBroken = true;
        tuiPiKeActive = false;

        var health = FixedRoomManager.Instance.GetPlayer()?.GetComponent<Health>();
        if (health != null)
        {
            health.currentHealth += damage;  // 回退伤害
            health.OnDamaged -= CheckTuiPiKe;
        }

        NotifyPropUsed(6); 

        // 释放毒气
        var player = FixedRoomManager.Instance.GetPlayer();
        if (player != null)
        {
            Vector3 gasCenter = player.transform.position;

            // 释放孢子视觉效果
            if (gasSporePrefab != null)
            {
                int burstCount = 20;
                for (int i = 0; i < burstCount; i++)
                {
                    GameObject spore = Instantiate(gasSporePrefab, gasCenter, Quaternion.identity);
                    SporeBehav behav = spore.GetComponent<SporeBehav>();
                    if (behav != null)
                    {
                        behav.moveSpeed = Random.Range(1f, 3f);
                        behav.lifetime = Random.Range(1f, 2f);
                    }
                    spore.transform.localScale = Vector3.one * Random.Range(0.08f, 0.2f);
                }
            }

            // 毒气伤害协程
            StartCoroutine(GasDamageCoroutine(gasCenter));
        }
    }

    /// <summary>
    /// 毒气伤害：以释放点为中心，半径3格，持续2秒，每0.5秒1伤
    /// </summary>
    IEnumerator GasDamageCoroutine(Vector3 center)
    {
        float elapsed = 0f;//计时
        float duration = 2f;//总持续时间
        float interval = 0.5f;//检测间隔
        float radius = 3f;//范围半径

        while (elapsed < duration)
        {
            Collider2D[] hits = Physics2D.OverlapCircleAll(center, radius);
            foreach (var hit in hits)
            {
                if (hit.CompareTag("Enemy"))
                {
                    hit.GetComponent<Health>()?.TakeDamage(1);
                }
            }

            elapsed += interval;
            yield return new WaitForSeconds(interval);
        }
    }

    // ==================== 道具07：血苔绷带 ====================
    // 血量＞5HP（0.5心）时，每次进入新房间扣除5HP
    // 伤害+0.5，本张地图有效

    private void XueTaiBengDai()
    {
        xueTaiActive = true;
        FixedRoomManager.OnRoomEntered += OnRoomEntered_XueTai;
        PlayerStats.Instance?.AddTempAttack(0.5f); 
    }

    private void OnRoomEntered_XueTai()
    {
        if (!xueTaiActive) return;

        var health = FixedRoomManager.Instance.GetPlayer()?.GetComponent<Health>();
        if (health != null && health.currentHealth > 5)
        {
            health.currentHealth -= 5;
        }
    }

    // ==================== 道具08：枯叶斗篷 ====================
    // 站立不动3秒后进入伪装，敌人无法发现你（仇恨解除，正常巡逻且不触发仇恨），本张地图有效

    private bool douPengActive = false;//是否激活
    private float standStillTimer = 0f;//计时器
    private bool isCamouflaged = false;//是否在伪装
    private Vector3 lastPosition;

    private void KuYeDouPeng()
    {
        douPengActive = true;
        lastPosition = FixedRoomManager.Instance.GetPlayer().transform.position;
    }

    // 需要在 Update 里检测，一个公共方法,给 PlayerController 调用
    public void UpdateDouPeng(bool isMoving, bool isShooting)
    {
        if (!douPengActive) return;

        // 移动或攻击 → 解除伪装
        if (isMoving || isShooting)
        {
            if (isCamouflaged)
            {
                isCamouflaged = false;
                SetPlayerVisible(true);
            }
            standStillTimer = 0f;
            return;
        }

        // 站立不动计时
        standStillTimer += Time.deltaTime;
        if (standStillTimer >= 3f && !isCamouflaged)
        {
            isCamouflaged = true;
            SetPlayerVisible(false);
        }
    }

    private void SetPlayerVisible(bool visible)
    {
        var player = FixedRoomManager.Instance.GetPlayer();
        if (player == null) return;

        // 改Tag让敌人检测不到
        player.tag = visible ? "Player" : "CamouflagedPlayer";

        // 半透明效果
        SpriteRenderer sr = player.GetComponent<SpriteRenderer>();
        if (sr != null)
            sr.color = visible ? Color.white : new Color(1, 1, 1, 0.3f);
    }

    // ==================== 道具09：蜂后蜜 ====================
    // 攻击力+0.5，但每次战斗开始时有50%概率额外生成2个敌人，本张地图有效

    private void FengHouMi()
    {
        PlayerStats.Instance?.AddTempAttack(0.5f);  // 临时加成，地图结束还原
        BattleRoom.OnBattleStart += OnBattleStart_SpawnExtraEnemies;
    }

    private void OnBattleStart_SpawnExtraEnemies()
    {
        // 50%概率
        if (Random.value > 0.5f) return;

        BattleRoom room = BattleRoom.Current;
        if (room == null) return;

        var config = room.GetRoomConfig();
        if (config?.battleSetting?.enemies == null || config.battleSetting.enemies.Count == 0) return;

        // 随机生成2个敌人
        for (int i = 0; i < 2; i++)
        {
            var info = config.battleSetting.enemies[Random.Range(0, config.battleSetting.enemies.Count)];
            room.SpawnExtraEnemy(info);
        }
    }

    // ==================== 道具10：石化种子 ====================
    // 每次攻击有10%概率对敌人造成石化（静止不动2秒，无法攻击移动，无法受到伤害），本张地图有效

    /// <summary>
    /// 应用道具：设置石化概率和持续时间
    /// </summary>
    private void ShiHuaZhongZi()
    {
        PlayerStats.Instance.stoneChance = 0.1f;       // 10%概率触发
        PlayerStats.Instance.stoneDuration = 2f;        // 石化持续2秒
    }

    /// <summary>
    /// 对敌人施加石化（由BaseBullet命中时调用）
    /// </summary>
    /// <param name="enemy">被石化的敌人</param>
    /// <param name="duration">石化持续时间</param>
    public void ApplyStone(GameObject enemy, float duration)
    {
        // 先检查是否已经在石化中
        Health health = enemy.GetComponent<Health>();
        if (health == null || health.isStoned) return; // 已经在石化中，不重复触发

        StartCoroutine(StoneCoroutine(enemy, duration));
    }

    /// <summary>
    /// 石化协程：敌人停止移动、变灰、无法受伤，持续指定秒数后恢复
    /// </summary>
    IEnumerator StoneCoroutine(GameObject enemy, float duration)
    {
        if (enemy == null) yield break;

        // 获取敌人组件
        Health health = enemy.GetComponent<Health>();
        IMovable movable = enemy.GetComponent<IMovable>();

        var skeleton = enemy.GetComponentInChildren<SkeletonAnimation>();//从自己开始往子物体找

        // === 进入石化 ===

        // 标记石化状态，Health里会跳过伤害
        if (health != null) health.isStoned = true;
        // 暂停移动
        movable?.PauseMovement();

        // 变灰
        object originalSpineColor = null;
        if (skeleton != null)
        {
            originalSpineColor = skeleton.Skeleton.GetColor();
            skeleton.Skeleton.SetColor(UnityEngine.Color.gray);
        }

        // 等待石化时间
        yield return new WaitForSeconds(duration);

        // === 解除石化 ===

        if (health != null) health.isStoned = false;
        movable?.ResumeMovement();

        if (skeleton != null && originalSpineColor != null)
            skeleton.Skeleton.SetColor((UnityEngine.Color)originalSpineColor);

    }

    // ==================== 道具11：狼人指尖 ====================（可能恐慌会抽搐，看看试玩
    // 射程-0.5，攻击+0.5，每次攻击敌人有20%概率造成恐慌1.5秒
    // 恐慌：停止攻击，尽可能远离主角
    // 本张地图有效

    private void LangRenZhiJian()
    {
        PlayerStats.Instance?.AddTempAttack(0.5f);   
        PlayerStats.Instance?.AddTempRange(-0.5f);   
        PlayerStats.Instance.panicChance = 0.2f;
        PlayerStats.Instance.panicDuration = 1.5f;
    }

    // 在 BaseBullet 命中时调用
    public void ApplyPanic(GameObject enemy, float duration)
    {
        StartCoroutine(PanicCoroutine(enemy, duration));
    }

    IEnumerator PanicCoroutine(GameObject enemy, float duration)
    {
        if (enemy == null) yield break;

        // 获取玩家位置用于远离
        Transform player = FixedRoomManager.Instance.GetPlayer()?.transform;
        if (player == null) yield break;

        // 禁用攻击脚本
        MonoBehaviour[] scripts = enemy.GetComponents<MonoBehaviour>();
        List<MonoBehaviour> disabledScripts = new List<MonoBehaviour>();
        foreach (var script in scripts)
        {
            if (script != null && script != this && script.GetType().Name.Contains("Attack"))
            {
                script.enabled = false;
                disabledScripts.Add(script);
            }
        }

        // 远离玩家
        IMovable movable = enemy.GetComponent<IMovable>();
        Rigidbody2D rb = enemy.GetComponent<Rigidbody2D>();

        float elapsed = 0f;
        while (elapsed < duration)
        {
            if (enemy == null) yield break;

            Vector2 fleeDir = (enemy.transform.position - player.position).normalized;
            rb.velocity = fleeDir * 2f;  // 恐慌逃跑速度

            elapsed += Time.deltaTime;
            yield return null;
        }

        // 恢复
        foreach (var script in disabledScripts)
        {
            if (script != null) script.enabled = true;
        }
    }

    // ==================== 道具12：月蚀碎片 ====================
    // 本场战斗内血量上限+1心（+10HP），一次性

    /// 记录月蚀碎片加的血量，用于战斗结束后还原
    private int yueShiBonus = 0;

    /// 应用道具：本场战斗血量上限+10，战斗结束后扣回
    private void YueShiSuiPian()
    {
        // 获取玩家血量组件
        var health = FixedRoomManager.Instance.GetPlayer()?.GetComponent<Health>();
        if (health != null)
        {
            // 加血量上限和当前血量
            health.maxHealth += 10;
            health.currentHealth += 10;
            yueShiBonus = 10;
        }

        // 订阅战斗结束事件，结束后还原
        BattleRoom.OnBattleEnd += OnBattleEnd_YueShi;
    }

    /// <summary>
    /// 战斗结束：扣除月蚀碎片加的血量上限
    /// </summary>
    private void OnBattleEnd_YueShi()
    {
        var health = FixedRoomManager.Instance.GetPlayer()?.GetComponent<Health>();
        if (health != null)
        {
            // 还原血量上限
            health.maxHealth -= yueShiBonus;

            NotifyPropUsed(12);  

            // 当前血量不超过新上限
            health.currentHealth = Mathf.Min(health.currentHealth, health.maxHealth);

            // 重置记录
            yueShiBonus = 0;
        }

        // 取消订阅，只生效一次
        BattleRoom.OnBattleEnd -= OnBattleEnd_YueShi;
    }
}