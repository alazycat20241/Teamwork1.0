using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// 地图UI管理器
/// 负责管理地图总面板、各商店面板的显示切换
/// 同时控制玩家的定身/解除
/// </summary>
public class MapUIManager : MonoBehaviour
{
    public static MapUIManager Instance { get; private set; }

    [Header("地图面板（挂有SlidePanel组件）")]
    [SerializeField] private SlidePanel mapPanel;              // 地图总面板

    [Header("各功能面板（挂有SlidePanel组件）")]
    [SerializeField] private SlidePanel dollShopPanel;         // 玩偶商店面板
    [SerializeField] private SlidePanel farmShopPanel;         // 种地商店面板
    [SerializeField] private SlidePanel upgradeRoomPanel;      // 升级房面板

    [Header("地图内的按钮")]
    [SerializeField] private Button dollShopButton;            // 玩偶商店按钮
    [SerializeField] private Button farmShopButton;            // 种地商店按钮
    [SerializeField] private Button upgradeRoomButton;         // 升级房按钮
    [SerializeField] private Button huntButton;                // 狩猎按钮
    [SerializeField] private Button backButton;                // 返回按钮

    private SlidePanel currentOpenPanel;                       // 当前打开的商店面板
    //private PlayerMove currentPlayer;                          // 缓存的玩家引用，用于定身/解冻

    void Awake()
    {
        // 单例初始化
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    void Start()
    {
        // 绑定按钮事件
        dollShopButton.onClick.AddListener(OpenDollShop);
        farmShopButton.onClick.AddListener(OpenFarmShop);
        upgradeRoomButton.onClick.AddListener(OpenUpgradeRoom);
        huntButton.onClick.AddListener(GoHunting);
        backButton.onClick.AddListener(CloseMap);
    }

    // ==================== 打开面板 ====================

    /// <summary>
    /// 打开地图总面板（由玩家碰撞触发）
    /// </summary>
    /// <param name="player">玩家的PlayerMove组件，用于定身</param>
    public void OpenMap()
    {
        // 缓存玩家引用
        //currentPlayer = player;
        // 定住玩家，防止移动导致面板关闭
        //FreezePlayer();
        Time.timeScale = 0f;
        // 打开地图面板
        mapPanel.Open();
    }

    /// <summary>
    /// 打开玩偶商店（消耗1点行动点）
    /// </summary>
    void OpenDollShop()
    {
        // 行动点不足则无法打开
        if (!ActionPointManager.Instance.UseActionPoints(1)) return;
        SwitchPanel(dollShopPanel);
    }

    /// <summary>
    /// 打开种地商店（消耗1点行动点）
    /// </summary>
    void OpenFarmShop()
    {
        if (!ActionPointManager.Instance.UseActionPoints(1)) return;
        SwitchPanel(farmShopPanel);
        HomeAudioManager.Instance?.PlayShopBGM();
    }

    /// <summary>
    /// 打开升级房（消耗1点行动点）
    /// </summary>
    void OpenUpgradeRoom()
    {
        if (!ActionPointManager.Instance.UseActionPoints(1)) return;
        SwitchPanel(upgradeRoomPanel);
    }

    /// <summary>
    /// 去狩猎（消耗2点行动点，加载战斗场景）
    /// </summary>
    void GoHunting()
    {
        // 行动点不足则无法出发
        if (!ActionPointManager.Instance.UseActionPoints(2)) return;

        // 关闭所有面板
        CloseAll();
        Time.timeScale = 1f;
        // 加载战斗场景
        if (SceneTransition.Instance != null)
        {
            SceneTransition.Instance.LoadScene("Battle");
        }
        else
        {
            SceneManager.LoadScene("Battle");  // 降级方案
        }
    }

    // ==================== 关闭面板 ====================

    /// <summary>
    /// 关闭地图总面板（也会关闭当前商店面板）
    /// 由地图内"返回"按钮或外部调用
    /// </summary>
    public void CloseMap()
    {
        mapPanel.Close(() => CheckResume());
        HomeAudioManager.Instance?.PlayMapBGM();
    }


    // ==================== 内部方法 ====================

    /// <summary>
    /// 切换面板：地图滑出 → 新商店面板滑入
    /// </summary>
    /// <param name="newPanel">要打开的商店面板</param>
    void SwitchPanel(SlidePanel newPanel)
    {
        // 地图关闭，打开新面板
        //mapPanel.Close();
        newPanel.Open();
        currentOpenPanel = newPanel;
    }

    /// <summary>
    /// 关闭所有面板（地图 + 商店）
    /// 用于狩猎等需要完全退出地图的情况
    /// </summary>
    void CloseAll()
    {
        // 关闭商店面板
        if (currentOpenPanel != null)
        {
            currentOpenPanel.Close();
            currentOpenPanel = null;
        }

        // 关闭地图面板
        mapPanel.Close();
        if(HomeAudioManager.Instance!=null) HomeAudioManager.Instance?.PlayMapBGM();
    }

    /// 定住玩家
    //void FreezePlayer()
    //{
    //    if (currentPlayer != null)
    //    {
    //        currentPlayer.Freeze();
    //    }
    //}

    /// <summary>
    /// 判断是否应该恢复游戏时间
    /// </summary>
    void CheckResume()
    {
        bool mapClosed = !mapPanel.gameObject.activeSelf;
        bool shopClosed = currentOpenPanel == null || !currentOpenPanel.gameObject.activeSelf;

        if (mapClosed && shopClosed)
        {
            Time.timeScale = 1f;
        }
    }

    public void CloseCurrentShop()
    {
        if (currentOpenPanel != null)
        {
            currentOpenPanel.Close();
            currentOpenPanel = null;
        }
    }
}