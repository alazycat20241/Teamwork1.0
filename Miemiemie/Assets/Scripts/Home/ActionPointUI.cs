using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ActionPointUI : MonoBehaviour
{
    [Header("UI组件")]
    [SerializeField] private TextMeshProUGUI actionPointText;      // 显示 "行动点数: 2/2"
    [SerializeField] private TextMeshProUGUI dayText;              // 显示 "第3天"
    [SerializeField] private TextMeshProUGUI lastdayText;              // 显示 "第3天"

    void Start()
    {
        // 订阅事件
        ActionPointManager.Instance.OnActionPointsChanged += UpdateActionPointDisplay;
        ActionPointManager.Instance.OnDayChanged += UpdateDayDisplay;
        ActionPointManager.Instance.OnDayChanged += UpdateLastDayDisplay;

        // 初始显示
        UpdateActionPointDisplay(
            ActionPointManager.Instance.GetCurrentPoints(),
            ActionPointManager.Instance.maxActionPoints
        );
        UpdateDayDisplay(ActionPointManager.Instance.GetCurrentDay());
        UpdateLastDayDisplay(ActionPointManager.Instance.GetCurrentDay());
    }

    void UpdateActionPointDisplay(int current, int max)
    {
        actionPointText.text = $"行动点: {current}/{max}";
    }

    void UpdateDayDisplay(int day)
    {
        dayText.text = $"Day{day}";
    }

    void UpdateLastDayDisplay(int day)
    {
        int remainingDays = 15 - day;  // 从14开始递减
        lastdayText.text = $"距月圆之夜{remainingDays}天";
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
