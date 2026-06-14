using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuUIManager : MonoBehaviour
{
    public static MenuUIManager Instance;

    [Header("面板引用")]
    public SlidePanel menuPanel;        // 菜单面板
    public SaveLoadPanel saveLoadPanel; // 存档面板
    public SlidePanel AudioSetPanel; // 存档面板

    [Header("存档读档按钮")]
    public Button btnSave;      // 存档按钮
    public Button btnLoad;      // 读档按钮
    public Button btnSet;      // 读档按钮

    // ★ 缓存的玩家引用
    //private GameObject playerInstance;
    void Awake()
    {
        // 单例设置
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // 绑定独立按钮事件
        if (btnSave != null)
            btnSave.onClick.AddListener(OnSaveButtonDirect);

        if (btnLoad != null)
            btnLoad.onClick.AddListener(OnLoadButtonDirect);

        if (btnSet != null)
            btnSet.onClick.AddListener(OnSetButtonDirect);
    }

    // ========== 打开菜单 ==========
    public void OpenMenu()
    {
        Time.timeScale = 0f;  // 暂停游戏
        menuPanel.Open();
    }

    // ========== 关闭菜单 ==========
    public void CloseMenu()
    {
        Time.timeScale = 1f;  // 恢复游戏
        menuPanel.Close();
    }

    public void CloseSet()
    {
        AudioSetPanel.Close();
    }
    /// <summary>
    /// 菜单面板-返回菜单场景
    /// </summary>
    public void OnReturnToMenuScene()
    {
        Time.timeScale = 1f;  // 恢复游戏

        // 获取玩家引用
        //if (FixedRoomManager.Instance != null)
        //{
        //    playerInstance = FixedRoomManager.Instance.GetPlayer();
        //}

        menuPanel.Close(() =>
        {
            // 销毁玩家
            //if (playerInstance != null)
            //{
            //    Destroy(playerInstance);
            //    playerInstance = null;
            //}

            if (SceneTransition.Instance != null)
            {
                SceneTransition.Instance.LoadScene("Menu");
            }
            else
            {
                SceneManager.LoadScene("Menu");  // 降级方案
            }
        });
    }

    // ========== 存档面板返回 ==========

    /// <summary>
    /// 从存档面板返回菜单面板
    /// </summary>
    public void OnSavePanelBack()
    {
        saveLoadPanel.Close(() =>
        {
            menuPanel.Open();
        });
    }

    /// <summary>
    /// 直接打开存档模式
    /// </summary>
    public void OnSaveButtonDirect()
    {
        saveLoadPanel.SetMode(true);
        saveLoadPanel.Open();
    }

    /// <summary>
    /// 直接打开读档模式
    /// </summary>
    public void OnLoadButtonDirect()
    {
        saveLoadPanel.SetMode(false);
        saveLoadPanel.Open();
    }

    /// <summary>
    /// 打开音量设置
    /// </summary>
    public void OnSetButtonDirect()
    {
        AudioSetPanel.Open();
    }

}