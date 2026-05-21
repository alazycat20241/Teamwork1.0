using UnityEngine;
using UnityEngine.Pool;
using System.Collections.Generic;

public class EffectPool : MonoBehaviour
{
    public static EffectPool Instance { get; private set; }

    [System.Serializable]
    public class PoolConfig
    {
        public string effectKey;      // 特效名称，如 "Spark", "BulletHit"
        public GameObject prefab;     // 特效预制体
        public int capacity = 10;
    }

    [SerializeField] private PoolConfig[] poolConfigs;
    private Dictionary<string, ObjectPool<GameObject>> pools;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        pools = new Dictionary<string, ObjectPool<GameObject>>();

        foreach (var config in poolConfigs)
        {
            if (config.prefab == null)
            {
                Debug.LogWarning($"EffectPool: {config.effectKey} 预制体为空，跳过");
                continue;
            }

            var cfg = config;
            pools[config.effectKey] = new ObjectPool<GameObject>(
                createFunc: () => Instantiate(cfg.prefab),
                actionOnGet: (obj) => obj.SetActive(true),
                actionOnRelease: (obj) => obj.SetActive(false),
                actionOnDestroy: (obj) => Destroy(obj),
                collectionCheck: false,
                defaultCapacity: cfg.capacity,
                maxSize: cfg.capacity * 2
            );
        }
    }

    /// <summary>
    /// 获取特效（新版本，需要指定key）
    /// </summary>
    public GameObject Get(string key)
    {
        if (!pools.ContainsKey(key))
        {
            Debug.LogError($"EffectPool: 未找到特效 '{key}'");
            return null;
        }
        return pools[key].Get();
    }

    /// <summary>
    /// 播放特效（带位置）
    /// </summary>
    public void PlayAt(string key, Vector3 position, Quaternion? rotation = null)
    {
        var obj = Get(key);
        if (obj != null)
        {
            obj.transform.position = position;
            if (rotation.HasValue) obj.transform.rotation = rotation.Value;
        }
    }

    /// <summary>
    /// 回收特效
    /// </summary>
    public void Release(string key, GameObject obj)
    {
        if (obj == null || !pools.ContainsKey(key)) return;

        // 重置粒子
        var ps = obj.GetComponent<ParticleSystem>();
        if (ps != null) ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        pools[key].Release(obj);
    }

    void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    // 在 EffectPool.cs 里添加这个方法
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
                // 忽略已销毁的对象
            }
        }
        pools.Clear();
    }
}