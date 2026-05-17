using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class SporePool : MonoBehaviour
{
    public static SporePool Instance { get; private set; }  // ← 加这行

    [Header("孢子预制体")]
    [SerializeField] private GameObject sporePrefab;

    [Header("爆发参数")]
    [SerializeField] private int burstCount = 20;       // 一次爆发多少孢子
    [SerializeField] private float moveSpeed = 8f;       // 飞行速度
    [SerializeField] private float lifetime = 5f;        // 存活时间

    private ObjectPool<GameObject> pool;                 // Unity内置对象池

    void Awake()
    {
        // 初始化对象池
        pool = new ObjectPool<GameObject>(
            createFunc: () =>                         // 创建新孢子
            {
                GameObject obj = Instantiate(sporePrefab);
                obj.GetComponent<SporeBehav>().pool = this;  // 把池引用给孢子
                return obj;
            },
            actionOnGet: (obj) => obj.SetActive(true),       // 取出时激活
            actionOnRelease: (obj) => obj.SetActive(false),   // 回收时隐藏
            actionOnDestroy: (obj) => Destroy(obj),          // 销毁时真删
            defaultCapacity: burstCount                      // 预分配容量
        );
    }

    /// <summary>
    /// 在指定位置爆发孢子
    /// </summary>
    public void BurstSpores(Vector3 position)
    {
        for (int i = 0; i < burstCount; i++)
        {
            GameObject spore = pool.Get();            // 从池里拿
            if (spore == null) continue;

            spore.transform.position = position;       // 设位置

            SporeBehav behav = spore.GetComponent<SporeBehav>();
            if (behav != null)
            {
                behav.moveSpeed = moveSpeed * Random.Range(0.2f, 0.8f);  // 随机速度，有远有近

                // 改成随机存活时间
                behav.lifetime = lifetime + Random.Range(-1f, 1f);   // 有的早1秒消失，有的晚1秒消失
            }

            // 随机大小：0.5倍 到 1.5倍之间
            float randomScale = Random.Range(0.08f, 0.2f);
            spore.transform.localScale = Vector3.one * randomScale;
        }
    }

    /// <summary>
    /// 回收单个孢子
    /// </summary>
    public void ReleaseSpore(GameObject spore)
    {
        if (spore != null) pool.Release(spore);
    }

    /// <summary>
    /// 清空池子（场景切换前调用）
    /// </summary>
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
