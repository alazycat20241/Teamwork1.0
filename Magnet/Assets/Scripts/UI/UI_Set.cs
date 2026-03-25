using UnityEngine;
using UnityEngine.UI;

public class UI_Set : MonoBehaviour
{
    // UI面板
    public GameObject targetUI;

    void Start()
    {
        // 一开始就强制隐藏
        if (targetUI != null)
        {
            targetUI.SetActive(false);
            GetComponent<Button>().onClick.AddListener(PauseGame);
        }
    }
    void PauseGame()
    {
        targetUI.SetActive(true);
        Time.timeScale = 0; // 暂停
    }

}