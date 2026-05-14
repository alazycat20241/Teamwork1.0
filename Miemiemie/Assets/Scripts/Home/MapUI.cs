using System;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 地图UI管理器
/// 负责管理地图总面板、各商店面板的显示切换
/// 地图面板通过玩家碰撞Collider触发打开
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
    [SerializeField] private Button backButton;                // 返回按钮（不消耗行动点）

    private SlidePanel currentOpenPanel;                       // 当前打开的商店面板（用于追踪）

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
        }
    }

    void Start()
    {
        // 绑定地图内各按钮的点击事件
        dollShopButton.onClick.AddListener(OpenDollShop);
        farmShopButton.onClick.AddListener(OpenFarmShop);
        upgradeRoomButton.onClick.AddListener(OpenUpgradeRoom);
        huntButton.onClick.AddListener(GoHunting);
        backButton.onClick.AddListener(CloseMap);
    }

    /// <summary>
    /// 打开地图总面板
    /// 由玩家碰撞Collider时调用
    /// </summary>
    public void OpenMap()
    {
        mapPanel.Open();
    }

    /// <summary>
    /// 关闭地图总面板（也会关闭当前商店面板）
    /// </summary>
    public void CloseMap()
    {
        // 如果有商店面板打开着，先关闭
        if (currentOpenPanel != null)
        {
            currentOpenPanel.Close();
            currentOpenPanel = null;
        }
        // 关闭地图
        mapPanel.Close();
    }

    /// <summary>
    /// 打开玩偶商店（消耗1点行动点）
    /// </summary>
    void OpenDollShop()
    {
        if (!ActionPointManager.Instance.UseActionPoints(1)) return;  // 行动点不足，不打开
        SwitchPanel(dollShopPanel);
    }

    /// <summary>
    /// 打开种地商店（消耗1点行动点）
    /// </summary>
    void OpenFarmShop()
    {
        if (!ActionPointManager.Instance.UseActionPoints(1)) return;
        SwitchPanel(farmShopPanel);
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
    /// 去狩猎（消耗2点行动点，加载狩猎场景）
    /// </summary>
    void GoHunting()
    {
        if (!ActionPointManager.Instance.UseActionPoints(2)) return;
        // 关闭所有面板后加载场景
        CloseMap();
        UnityEngine.SceneManagement.SceneManager.LoadScene("HuntScene");
    }

    /// <summary>
    /// 切换面板：地图滑出 → 新商店面板滑入
    /// </summary>
    /// <param name="newPanel">要打开的商店面板</param>
    void SwitchPanel(SlidePanel newPanel)
    {
        // 地图关闭动画完成后，再打开新面板
        mapPanel.Close(() =>
        {
            newPanel.Open();
            currentOpenPanel = newPanel;
        });
    }

    /// <summary>
    /// 关闭当前商店面板，返回地图
    /// 由商店面板内的"返回"按钮调用
    /// </summary>
    public void CloseCurrentPanel()
    {
        if (currentOpenPanel != null)
        {
            // 商店关闭后，地图重新滑入
            currentOpenPanel.Close(() =>
            {
                mapPanel.Open();
                currentOpenPanel = null;
            });
        }
    }

    /// <summary>
    /// 从商店直接返回主界面（不经过地图）
    /// 由商店面板内的"返回主界面"按钮调用
    /// </summary>
    public void ReturnToMainFromShop()
    {
        if (currentOpenPanel != null)
        {
            currentOpenPanel.Close();
            currentOpenPanel = null;
        }
    }
}