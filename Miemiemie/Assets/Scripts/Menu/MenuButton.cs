using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuButton : MonoBehaviour
{
    public Button StartButton;
    public Button ContinueBut;
    public Button ExitButton;

    void Start()
    {
        StartButton.onClick.AddListener(OnNewGame);
        ContinueBut.onClick.AddListener(OnContinueClick);
        ExitButton.onClick.AddListener(OnQuit);
    }

    // 新游戏
    public void OnNewGame()
    {
        // 重置所有数据
        if (PlayerInventory.Instance != null)
            PlayerInventory.Instance.ResetData();

        if (ActionPointManager.Instance != null)
            ActionPointManager.Instance.ResetData();

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

    //继续游戏
    public void OnContinueClick()
    {
        if (SaveManager.Instance != null && SaveManager.Instance.SlotHasSave(0))
        {
            SaveManager.Instance.LoadFromSlot(0);
        }
    }

    // 退出游戏
    public void OnQuit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
    }
}
