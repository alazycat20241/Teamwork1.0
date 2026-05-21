using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuUIManager : MonoBehaviour
{
    public static MenuUIManager Instance;

    [Header("面板引用")]
    public SlidePanel menuPanel;        // 菜单面板
    public SaveLoadPanel saveLoadPanel; // 存档面板

    [Header("打开菜单的按钮（游戏界面那个按钮1）")]
    public GameObject openMenuButton;   // 按钮1（游戏界面中的）

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            transform.SetParent(null);
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // ========== 按钮1：打开菜单 ==========
    public void OpenMenu()
    {
        menuPanel.Open();
    }

    // ========== 按钮1：打开菜单 ==========
    public void CloseMenu()
    {
        menuPanel.Close();
    }

    // ========== 菜单面板按钮 ==========

    /// <summary>
    /// 菜单面板-存档按钮
    /// </summary>
    public void OnSaveButton()
    {
        menuPanel.Close(() =>
        {
            saveLoadPanel.SetMode(true);
            saveLoadPanel.Open();
        });
    }

    /// <summary>
    /// 菜单面板-读档按钮
    /// </summary>
    public void OnLoadButton()
    {
        menuPanel.Close(() =>
        {
            saveLoadPanel.SetMode(false);
            saveLoadPanel.Open();
        });
    }

    /// <summary>
    /// 菜单面板-返回菜单场景
    /// </summary>
    public void OnReturnToMenuScene()
    {
        menuPanel.Close(() =>
        {
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
}