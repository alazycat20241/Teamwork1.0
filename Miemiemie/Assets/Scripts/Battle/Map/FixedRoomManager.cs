using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro; 

public class FixedRoomManager : MonoBehaviour
{
    public static FixedRoomManager Instance;

    [Header("当前地图")]
    [SerializeField] private FixedMapData currentMap;

    [Header("房间预制体")]
    [SerializeField] private GameObject emptyRoomPrefab;
    [SerializeField] private GameObject battleRoomPrefab;
    [SerializeField] private GameObject eventRoomPrefab;
    [SerializeField] private GameObject shopRoomPrefab;
    [SerializeField] private GameObject bossRoomPrefab;

    [Header("玩家")]
    [SerializeField] private GameObject playerPrefab;

    [Header("音效")]
    [SerializeField] private AudioClip pageFlipSound;

    [Header("结算面板")]
    [SerializeField] private SlidePanel resultPanel;     // 结算UI面板
    [SerializeField] private Button btnContinue;         // 继续按钮

    // 结算面板上的统计文本
    [Header("结算统计")]
    [SerializeField] private TMP_Text goldCollectedText;     // 金币收集数量
    [SerializeField] private TMP_Text soulStoneCollectedText; // 灵魂石收集数量
    [SerializeField] private TMP_Text timeText;               // 用时显示

    // ★ 地图收集统计
    private int goldCollectedThisRun = 0;
    private int soulStonesCollectedThisRun = 0;

    // ★ 计时器
    private float runStartTime;       // 计时开始时间
    private float runElapsedTime;     // 已用时间
    private bool isTiming = false;    // 是否正在计时

    // 运行时数据
    private RoomConfig currentRoom;
    private GameObject currentRoomInstance;
    private GameObject playerInstance;
    private Dictionary<string, bool> clearedRooms = new Dictionary<string, bool>();

    [SerializeField] private Image progressFillImage;  // ★ Filled模式的Image
    [SerializeField] private Image iconImage;              // 图标Image
    [SerializeField] private Image crossImage;             // 叉叉Image（盖在图标上）
    [SerializeField] private Image crossImageOnFill;       // 盖在filled到达位置的叉叉Image

    private int roomsClearedCount = 0;  // ★ 已通关房间数
    private int totalRooms = 0;         // ★ 总房间数

    private PlayerPet pet ;  // 找跟宠

    /// <summary>
    /// 进入房间记号（供道具使用
    /// </summary>
    public static event System.Action OnRoomEntered;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        // ★ 绑定继续按钮
        if (btnContinue != null)
        {
            btnContinue.onClick.AddListener(ReturnToHome);
        }
    }

    void Start()
    {
        // ★ 地图开始时清除上次残留的临时效果
        PlayerStats.Instance?.SnapshotBaseline();

        // 初始化通关记录
        clearedRooms.Clear();

        totalRooms = 12;  // ★ 总房间数

        // 重置本次地图收集统计
        goldCollectedThisRun = 0;
        soulStonesCollectedThisRun = 0;

        foreach (var room in currentMap.rooms)
        {
            clearedRooms[room.roomId] = false;
        }

        // 创建玩家（只创建一次
        CreatePlayer();

        //开始计时
        StartTiming();

        // 加载起始房间
        RoomConfig startRoom = currentMap.GetStartRoom();
        if (startRoom != null)
        {
            LoadRoom(startRoom);
        }

        pet = FindObjectOfType<PlayerPet>(); 
    }

    // ★ 更新计时器（每帧调用）
    void Update()
    {
        if (isTiming)
        {
            runElapsedTime = Time.time - runStartTime;
        }
    }

    // 创建玩家（只执行一次）
    private void CreatePlayer()
    {
        playerInstance = Instantiate(playerPrefab);

        // 把 PlayerStats 的永久属性应用到新创建玩家上
        ApplyPlayerStats();
    }

    /// <summary>
    /// 将 PlayerStats 中的永久加成应用到玩家组件
    /// </summary>
    private void ApplyPlayerStats()
    {
        if (PlayerStats.Instance == null || playerInstance == null) return;

        var ps = PlayerStats.Instance;

        // 射程
        var shoot = playerInstance.GetComponent<PlayerShoot>();
        if (shoot != null)
            shoot.AddRange(ps.rangeBonus);

        // 血量上限
        var health = playerInstance.GetComponent<Health>();
        if (health != null)
        {
            health.maxHealth += ps.maxHealthBonus;
            health.currentHealth = health.maxHealth;
        }

        // 攻击力、射速、石化概率、恐慌概率、失误率
        // 这些在 PlayerShoot / BaseBullet 里每帧或每次射击时直接读 PlayerStats.Instance
        // 不需要额外应用
    }

    // 加载房间
    public void LoadRoom(RoomConfig roomConfig)
    {
        StartCoroutine(TransitionToRoom(roomConfig));
    }

    private IEnumerator TransitionToRoom(RoomConfig newRoom)
    {
        // 清理旧房间
        if (currentRoomInstance != null)
        {
            Destroy(currentRoomInstance);
        }

        currentRoom = newRoom;

        // 创建新房间
        GameObject prefab = GetRoomPrefab(newRoom);
        // 移动新房间
        Vector3 roomPos = new Vector3(0, newRoom.mapPosition.y * 20f, 0);
        currentRoomInstance = Instantiate(prefab, roomPos, Quaternion.identity);

        //摄像头跟随房间
        Camera.main.GetComponent<CameraFollow>()?.MoveToRoom(roomPos);

        // 翻页音效
        AudioManager.Instance.PlaySound(pageFlipSound);

        // 设置房间
        RoomBase roomScript = currentRoomInstance.GetComponent<RoomBase>();
        if (roomScript != null)
        {
            roomScript.SetupRoom(newRoom);

            // 把玩家和跟宠移动到新房间的出生点
            if (playerInstance != null && roomScript.playerSpawnPoint != null)
            {
                playerInstance.transform.position = roomPos + roomScript.playerSpawnPoint.localPosition;

                if(pet!=null)pet.TeleportToPlayer();

                // 重置玩家状态
                ResetPlayerState();
            }
        }

        //进入新房间
        OnRoomEntered?.Invoke();

        yield return null;
    }

    // 移动到目标房间
    public void MoveToRoom(string targetRoomId)
    {
        RoomConfig targetRoom = currentMap.GetRoomById(targetRoomId);
        if (targetRoom != null)
        {
            LoadRoom(targetRoom);
        }
    }

    // 标记房间已通关
    public void MarkRoomCleared(string roomId)
    {
        if (clearedRooms.ContainsKey(roomId))
        {
            clearedRooms[roomId] = true;
            roomsClearedCount++;
        }
    }

    // 检查房间是否已通关
    public bool IsRoomCleared(string roomId)
    {
        return clearedRooms.ContainsKey(roomId) && clearedRooms[roomId];
    }

    // 获取当前地图
    public FixedMapData GetCurrentMap()
    {
        return currentMap;
    }

    // 获取玩家
    public GameObject GetPlayer()
    {
        return playerInstance;
    }
    private void ResetPlayerState()
    {
        if (playerInstance == null) return;

        Rigidbody2D rb = playerInstance.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.velocity = Vector2.zero;
        }
    }
    private GameObject GetRoomPrefab(RoomConfig roomConfig)
    {
        if (roomConfig.customRoomPrefab != null)
            return roomConfig.customRoomPrefab;

        switch (roomConfig.roomType)
        {
            case RoomType.Empty: return emptyRoomPrefab;
            case RoomType.Battle: return battleRoomPrefab;
            case RoomType.Event: return eventRoomPrefab;
            case RoomType.Shop: return shopRoomPrefab;
            case RoomType.Boss: return bossRoomPrefab;
            default: return emptyRoomPrefab;
        }
    }

    // 返回家园
    public void ReturnToHome(bool victory)
    {
        // 停止计时
        StopTiming();

        // 还原本张地图所有临时效果
        PlayerStats.Instance?.RestoreTempEffects();
        
        // 清理事件效果的所有回调（防止跨场景残留）
        EventRoomEffect.Instance?.CleanupAll();
        
        // 清理道具效果（防止跨场景残留）
        PropManager.Instance?.CleanupAllProps();

        // 战败处理：通知行动点管理器
        if (!victory && ActionPointManager.Instance != null)
        {
            ActionPointManager.Instance.DefeatedInHunt();
        }

        // 在打开面板前更新统计文本
        UpdateCollectionStatsText();

        if (resultPanel != null)
        {
            // 更新进度条
            UpdateProgressFill(victory);
            resultPanel.Open();
        }

        // ========== 清空所有对象池 ==========
        if (PoolManager.Instance != null)PoolManager.Instance.ClearAllPools();//子弹池

        if (EffectPool.Instance != null)EffectPool.Instance.Clear();//特效池

        if (SporePool.Instance != null)SporePool.Instance.Clear();//孢子池

        // 销毁玩家
        if (playerInstance != null)
        {
            Destroy(playerInstance);
        }

        // 销毁当前房间
        if (currentRoomInstance != null)
        {
            Destroy(currentRoomInstance);
        }
    }

    public void ReturnToHome()
    {
        // 过渡加载
        if (SceneTransition.Instance != null)
        {
            SceneTransition.Instance.LoadScene("Home");
        }
        else
        {
            SceneManager.LoadScene("Home");  // 降级方案
        }
    }

    // 更新结算面板的收集统计文本
    private void UpdateCollectionStatsText()
    {
        if (goldCollectedText != null)
        {
            goldCollectedText.text = goldCollectedThisRun.ToString();
        }

        if (soulStoneCollectedText != null)
        {
            soulStoneCollectedText.text = soulStonesCollectedThisRun.ToString();
        }

        if (timeText != null)
        {
            timeText.text = FormatTime(runElapsedTime);
        }

        // 复制道具
        if(CopyChildSprites.Instance!=null)
        CopyChildSprites.Instance.CopySprites();
    }

    public float deletex;
    // 更新进度条填充
    private void UpdateProgressFill(bool victory)
    {
        // 更新进度条
        if (progressFillImage != null)
        {
            if (roomsClearedCount < totalRooms)
            {
                progressFillImage.fillAmount = roomsClearedCount * 0.065f;
            }
            else 
            {
                progressFillImage.fillAmount = 1f;
            }
        }

        // 移动叉叉到filled位置
        if (crossImageOnFill != null && progressFillImage != null)
        {
            float fillAmount = progressFillImage.fillAmount;
            // 根据fillAmount设置叉叉的x位置（根据你进度条的实际宽度调整）
            crossImageOnFill.rectTransform.anchoredPosition = new Vector2(deletex + fillAmount * 1124f, crossImageOnFill.rectTransform.anchoredPosition.y);
        }
        if (crossImage != null && progressFillImage != null)
        {
            float fillAmount = progressFillImage.fillAmount;
            crossImage.rectTransform.anchoredPosition = new Vector2(deletex + fillAmount * 1124f, crossImage.rectTransform.anchoredPosition.y);
        }
        if (iconImage != null && progressFillImage != null)
        {
            float fillAmount = progressFillImage.fillAmount;
            iconImage.rectTransform.anchoredPosition = new Vector2(deletex + fillAmount * 1124f, iconImage.rectTransform.anchoredPosition.y);
        }

        // Boss全通，隐藏叉叉
        if (victory&&roomsClearedCount == totalRooms)
        {
            if (crossImage != null) crossImage.gameObject.SetActive(false);
            if (crossImageOnFill != null) crossImageOnFill.gameObject.SetActive(false);
        }
    }

    //记录收集的金币
    public void AddCollectedGold(int amount)
    {
        goldCollectedThisRun += amount;
    }

    //记录收集的灵魂石
    public void AddCollectedSoulStone(int amount)
    {
        soulStonesCollectedThisRun += amount;
    }

    // 开始计时
    private void StartTiming()
    {
        runStartTime = Time.time;//游戏运行的总时间
        runElapsedTime = 0f;//已用时间
        isTiming = true;
    }

    // 停止计时
    private void StopTiming()
    {
        isTiming = false;
    }

    // ★ 格式化时间为 分:秒 格式
    private string FormatTime(float timeInSeconds)
    {
        int minutes = Mathf.FloorToInt(timeInSeconds / 60f);
        int seconds = Mathf.FloorToInt(timeInSeconds % 60f);
        return string.Format("{0:00}:{1:00}", minutes, seconds);
    }
}