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
    /// 手动获取特效（不自动回收）
    /// </summary>
    public GameObject Get()
    {
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
        try
        {
            pool?.Clear();
        }
        catch (MissingReferenceException) { }
    }
}