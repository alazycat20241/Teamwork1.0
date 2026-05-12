using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoolManager : Singleton<PoolManager>
{
    public Dictionary<int,BulletPool>pools=new Dictionary<int,BulletPool>();
    /// <summary>
    /// 返回对应对象池
    /// </summary>
    /// <param name="bulletObject"></param>
    /// <returns></returns>
    public BulletPool GetPool(BulletObject bulletObject)
    {
        if (!pools.ContainsKey(bulletObject.ID))
        {
            var pool=new BulletPool();
            pool.bulletObject = bulletObject;
            pools.Add(bulletObject.ID, pool);
        }
        return pools[bulletObject.ID];
    }
}
