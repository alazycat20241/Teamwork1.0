using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraShaker : MonoBehaviour
{
    /// 相机抖动管理器（单例）
    /// 管理当前的抖动强度，支持多个抖动叠加

    // 单例
    private static CameraShaker _instance;
    public static CameraShaker Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<CameraShaker>();
                if (_instance == null)
                {
                    GameObject go = new GameObject("CameraShaker");
                    _instance = go.AddComponent<CameraShaker>();
                    DontDestroyOnLoad(go);
                }
            }
            return _instance;
        }
    }

    [Header("抖动设置")]
    [SerializeField] private float globalShakeFactor = 1f;  // 全局抖动强度倍率

    // 当前活跃的抖动效果列表
    private System.Collections.Generic.List<ShakeEffect> activeShakes = new System.Collections.Generic.List<ShakeEffect>();

    // 当前总抖动强度（所有活跃抖动的最大值）
    private float currentIntensity = 0;

    /// 添加一个抖动效果
    /// <param name="intensity">抖动强度（0-1）</param>
    /// <param name="duration">持续时间（秒）</param>
    public void AddShake(float intensity, float duration)
    {
        // 限制强度范围
        intensity = Mathf.Clamp01(intensity);

        // 添加新的抖动效果
        activeShakes.Add(new ShakeEffect(intensity, duration));
    }

    /// 获取当前抖动强度（供外部使用）
    public float ShakeIntensity => currentIntensity * globalShakeFactor;

    private void Update()
    {
        // 更新所有活跃的抖动效果
        for (int i = activeShakes.Count - 1; i >= 0; i--)
        {
            ShakeEffect shake = activeShakes[i];
            shake.Update(Time.deltaTime);

            // 如果抖动结束了，从列表中移除
            if (shake.IsFinished)
            {
                activeShakes.RemoveAt(i);
            }
        }

        // 计算当前总强度（取所有活跃抖动的最大值）
        currentIntensity = 0;
        foreach (var shake in activeShakes)
        {
            currentIntensity = Mathf.Max(currentIntensity, shake.CurrentIntensity);
        }
    }

    /// 清空所有抖动
    public void ClearAllShakes()
    {
        activeShakes.Clear();
        currentIntensity = 0;
    }

    /// 设置全局抖动强度（用于设置菜单）
    public void SetGlobalShakeFactor(float factor)
    {
        globalShakeFactor = Mathf.Clamp01(factor);
    }
}

/// 单个抖动效果的数据结构
public class ShakeEffect
{
    private float startIntensity;   // 起始强度
    private float duration;         // 总持续时间
    private float elapsedTime;      // 已过去的时间

    public ShakeEffect(float intensity, float duration)
    {
        this.startIntensity = intensity;
        this.duration = duration;
        this.elapsedTime = 0;
    }

    public bool IsFinished => elapsedTime >= duration;

    /// 当前强度（随时间线性衰减）
    public float CurrentIntensity
    {
        get
        {
            if (IsFinished) return 0;
            // 线性衰减：强度 * (剩余时间比例)
            float t = 1 - (elapsedTime / duration);
            return startIntensity * t;
        }
    }

    public void Update(float deltaTime)
    {
        elapsedTime += deltaTime;
    }
}
