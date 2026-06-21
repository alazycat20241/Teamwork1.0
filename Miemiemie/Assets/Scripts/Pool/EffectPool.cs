using UnityEngine;
using UnityEngine.Pool;
using System.Collections.Generic;

/// <summary>
/// 特效对象池管理器
/// 统一管理所有特效的创建、播放、回收
/// 每个特效通过 effectKey 标识
/// </summary>
public class EffectPool : MonoBehaviour
{
    public static EffectPool Instance { get; private set; }

    /// <summary>
    /// 单个特效池的配置
    /// </summary>
    [System.Serializable]
    public class PoolConfig
    {
        public string effectKey;      // 特效名称
        public GameObject prefab;     // 特效预制体
        public int capacity = 10;     // 初始容量
    }

    [SerializeField] private PoolConfig[] poolConfigs;                    // 所有特效池的配置（Inspector 里填）
    private Dictionary<string, ObjectPool<GameObject>> pools;             // 特效名 → 对象池

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        pools = new Dictionary<string, ObjectPool<GameObject>>();

        // ===== 根据配置创建所有对象池 =====
        foreach (var config in poolConfigs)
        {
            // 没拖预制体 → 跳过
            if (config.prefab == null) continue;

            // 闭包陷阱：存局部变量
            var cfg = config;

            // 创建 Unity 内置对象池
            pools[config.effectKey] = new ObjectPool<GameObject>(
                createFunc: () => Instantiate(cfg.prefab),           // 没货时克隆预制体
                actionOnGet: (obj) => obj.SetActive(true),            // 取出时激活
                actionOnRelease: (obj) => obj.SetActive(false),           // 回收时隐藏
                actionOnDestroy: (obj) => Destroy(obj),                   // 销毁时真删
                collectionCheck: false,                                     // 不检查重复回收（性能优化）
                defaultCapacity: cfg.capacity,                              // 预分配容量
                maxSize: cfg.capacity * 2                           // 最大容量（超过则销毁多余）
            );
        }
    }

    /// <summary>
    /// 从池中获取一个特效
    /// </summary>
    /// <param name="key">特效名称</param>
    public GameObject Get(string key)
    {
        if (!pools.ContainsKey(key)) return null;
        return pools[key].Get();
    }

    /// <summary>
    /// 在指定位置播放特效
    /// </summary>
    /// <param name="position">世界坐标位置</param>
    /// <param name="rotation">旋转（默认不旋转）</param>
    public void PlayAt(string key, Vector3 position, Quaternion? rotation = null)
    {
        var obj = Get(key);
        if (obj != null)
        {
            obj.transform.position = position;
            if (rotation.HasValue)
                obj.transform.rotation = rotation.Value;
        }
    }

    /// <summary>
    /// 回收特效到池中
    /// 回收前会停止粒子系统，防止残留粒子
    /// </summary>
    /// <param name="key">特效名称</param>
    /// <param name="obj">要回收的 GameObject</param>
    public void Release(string key, GameObject obj)
    {
        if (obj == null || !pools.ContainsKey(key)) return;

        // 停止粒子系统，清除残留粒子
        var ps = obj.GetComponent<ParticleSystem>();
        if (ps != null)
            ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        pools[key].Release(obj);
    }

    /// <summary>
    /// 清空所有对象池（fixedroom已调用
    /// </summary>
    public void Clear()
    {
        if (pools == null) return;

        foreach (var pool in pools.Values)
        {
            try
            {
                pool?.Clear();
            }
            catch (MissingReferenceException)
            {
                // 池中对象已被销毁，忽略错误
            }
        }
        pools.Clear();
    }

    void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }
}