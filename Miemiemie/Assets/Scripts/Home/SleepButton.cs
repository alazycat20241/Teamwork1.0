using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SleepButton : MonoBehaviour
{
    private Button button;
    
    void Awake()
    {
        button = gameObject.GetComponent<Button>();
    }
    
    void OnEnable()
    {
        // 确保只注册一次
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(NextDay);
    }
    
    void OnDisable()
    {
        // 移除监听器，防止重复调用
        button.onClick.RemoveListener(NextDay);
    }

    void NextDay()
    {
        ActionPointManager.Instance.AdvanceDay();
    }
}
