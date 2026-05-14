using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoolManager : Singleton<PoolManager>
{
    // 子弹对象池
    public Dictionary<int, BulletPool> pools = new Dictionary<int, BulletPool>();

    /// <summary>
    /// 返回对应子弹对象池
    /// </summary>
    public BulletPool GetPool(BulletObject bulletObject)
    {
        if (!pools.ContainsKey(bulletObject.ID))
        {
            var pool = new BulletPool();
            pool.bulletObject = bulletObject;
            pools.Add(bulletObject.ID, pool);
        }
        return pools[bulletObject.ID];
    }
}
