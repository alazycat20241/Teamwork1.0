using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActionPointManager : MonoBehaviour
{
    public static ActionPointManager Instance { get; private set; }

    [Header("行动点数设置")]
    public int maxActionPoints = 2;           // 最大行动点数
    [SerializeField] private int currentActionPoints;           // 当前行动点数
    [SerializeField] private int currentDay = 1;                // 当前天数

    // 事件：点数变化时通知UI更新
    public event Action<int, int> OnActionPointsChanged;        // (当前值, 最大值)
    public event Action<int> OnDayChanged;                      // (当前天数)

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        currentActionPoints = maxActionPoints;
        OnActionPointsChanged?.Invoke(currentActionPoints, maxActionPoints);
    }

    /// <summary>
    /// 尝试消耗行动点数
    /// </summary>
    /// <param name="cost">消耗数量</param>
    /// <returns>是否消耗成功</returns>
    public bool UseActionPoints(int cost)
    {
        if (currentActionPoints >= cost)
        {
            currentActionPoints -= cost;
            OnActionPointsChanged?.Invoke(currentActionPoints, maxActionPoints);

            // 点数耗尽自动进入下一天
            if (currentActionPoints <= 0)
            {
                AdvanceDay();
            }
            return true;
        }
        else
        {
            Debug.Log("行动点数不足！");
            return false;
        }
    }

    /// <summary>
    /// 进入下一天，回复行动点数
    /// </summary>
    public void AdvanceDay()
    {
        currentDay++;
        OnDayChanged?.Invoke(currentDay);

        // 默认回满
        currentActionPoints = maxActionPoints;
        OnActionPointsChanged?.Invoke(currentActionPoints, maxActionPoints);
    }

    /// <summary>
    /// 狩猎战败：回复上限的一半（向下取整）
    /// </summary>
    public void DefeatedInHunt()
    {
        currentDay++;
        OnDayChanged?.Invoke(currentDay);

        // 回复上限的一半，向下取整
        currentActionPoints = Mathf.FloorToInt(maxActionPoints / 2f);
        OnActionPointsChanged?.Invoke(currentActionPoints, maxActionPoints);
    }

    /// <summary>
    /// 查询当前剩余点数（方便其他脚本检查）
    /// </summary>
    public int GetCurrentPoints()
    {
        return currentActionPoints;
    }

    /// <summary>
    /// 获取当前天数
    /// </summary>
    public int GetCurrentDay()
    {
        return currentDay;
    }
}
