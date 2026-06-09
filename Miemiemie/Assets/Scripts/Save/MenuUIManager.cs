using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuUIManager : MonoBehaviour
{
    public static MenuUIManager Instance;

    [Header("面板引用")]
    public SlidePanel menuPanel;        // 菜单面板
    public SaveLoadPanel saveLoadPanel; // 存档面板

    [Header("存档读档按钮")]
    public Button btnSave;      // 存档按钮
    public Button btnLoad;      // 读档按钮

    void Awake()
    {
        //if (Instance == null)
        //{
        //    Instance = this;
        //    transform.SetParent(null);
        //    DontDestroyOnLoad(gameObject);
        //}
        //else
        //{
        //    Destroy(gameObject);
        //    return;
        //}

        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // 绑定独立按钮事件
        if (btnSave != null)
            btnSave.onClick.AddListener(OnSaveButtonDirect);

        if (btnLoad != null)
            btnLoad.onClick.AddListener(OnLoadButtonDirect);
    }

    // ========== 按钮1：打开菜单 ==========
    public void OpenMenu()
    {
        Debug.Log("OpenMenu被调用");
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
    //public void OnSaveButton()
    //{
    //    menuPanel.Close(() =>
    //    {
    //        saveLoadPanel.SetMode(true);
    //        saveLoadPanel.Open();
    //    });
    //}

    ///// <summary>
    ///// 菜单面板-读档按钮
    ///// </summary>
    //public void OnLoadButton()
    //{
    //    menuPanel.Close(() =>
    //    {
    //        saveLoadPanel.SetMode(false);
    //        saveLoadPanel.Open();
    //    });
    //}

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
}