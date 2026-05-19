using UnityEngine;
using UnityEngine.Pool;

public class EffectPool : MonoBehaviour
{
    public static EffectPool Instance { get; private set; }

    [SerializeField] private GameObject effectPrefab;      // 特效预制体
    [SerializeField] private int defaultCapacity = 10;

    private ObjectPool<GameObject> pool;

    void Awake()
    {
        // 单例安全检查
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        pool = new ObjectPool<GameObject>(
            createFunc: () => Instantiate(effectPrefab),
            actionOnGet: (obj) => obj.SetActive(true),
            actionOnRelease: (obj) => obj.SetActive(false),
            actionOnDestroy: (obj) => Destroy(obj),
            collectionCheck: false,
            defaultCapacity: defaultCapacity,
            maxSize: 50
        );
    }

    /// <summary>
    /// 播放特效
    /// </summary>
    public void PlayAt(Vector3 position)
    {
        GameObject effect = Get();
        if (effect != null)
        {
            effect.transform.position = position;
        }
    }

    /// <summary>
    /// 手动获取特效
    /// </summary>
    public GameObject Get()
    {
        if (pool == null) return null;

        GameObject obj = null;
        try
        {
            obj = pool.Get();
            if (obj == null)
            {
                obj = Instantiate(effectPrefab);
            }
        }
        catch (MissingReferenceException)
        {
            obj = Instantiate(effectPrefab);
        }
        return obj;
    }

    /// <summary>
    /// 回收特效
    /// </summary>
    public void Release(GameObject effect)
    {
        if (effect == null || pool == null) return;

        // 停止特效上的协程（如果有）
        MonoBehaviour behaviour = effect.GetComponent<MonoBehaviour>();
        if (behaviour != null)
        {
            behaviour.StopAllCoroutines();
        }

        pool.Release(effect);
    }

    public void Clear()
    {
        try
        {
            pool?.Clear();
        }
        catch (MissingReferenceException) { }
    }

    void OnDestroy()
    {
        // 清理池子
        try
        {
            pool?.Clear();
        }
        catch (MissingReferenceException) { }

        // 单例清理
        if (Instance == this)
        {
            Instance = null;
        }
    }
}