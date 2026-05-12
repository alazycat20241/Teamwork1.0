using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ActionPointUI : MonoBehaviour
{
    [Header("UI组件")]
    [SerializeField] private TextMeshProUGUI actionPointText;      // 显示 "行动点数: 2/2"
    [SerializeField] private TextMeshProUGUI dayText;              // 显示 "第3天"
    [SerializeField] private GameObject insufficientWarning;       // 点数不足提示

    void Start()
    {
        // 订阅事件
        ActionPointManager.Instance.OnActionPointsChanged += UpdateActionPointDisplay;
        ActionPointManager.Instance.OnDayChanged += UpdateDayDisplay;

        // 初始显示
        UpdateActionPointDisplay(
            ActionPointManager.Instance.GetCurrentPoints(),
            ActionPointManager.Instance.maxActionPoints
        );
        UpdateDayDisplay(ActionPointManager.Instance.GetCurrentDay());
    }

    void UpdateActionPointDisplay(int current, int max)
    {
        actionPointText.text = $"ActionPoint: {current}/{max}";
    }

    void UpdateDayDisplay(int day)
    {
        dayText.text = $"Day{day}";
    }

    /// <summary>
    /// 显示点数不足提示
    /// </summary>
    public void ShowInsufficientWarning()
    {
        if (insufficientWarning != null)
        {
            insufficientWarning.SetActive(true);
            Invoke(nameof(HideWarning), 2f);
        }
    }

    void HideWarning()
    {
        insufficientWarning.SetActive(false);
    }

    void OnDestroy()
    {
        // 取消订阅，避免报错
        if (ActionPointManager.Instance != null)
        {
            ActionPointManager.Instance.OnActionPointsChanged -= UpdateActionPointDisplay;
            ActionPointManager.Instance.OnDayChanged -= UpdateDayDisplay;
        }
    }
}
