using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoolManager : Singleton<PoolManager>
{
    /// <summary>
    /// 子弹对象池字典，Key = 子弹ID，Value = 对应的对象池
    /// </summary>
    public Dictionary<int, BulletPool> pools = new Dictionary<int, BulletPool>();

    /// <summary>
    /// 获取指定 BulletObject 对应的对象池（不存在则自动创建）
    /// </summary>
    public BulletPool GetPool(BulletObject bulletObject)
    {
        // 如果池不存在，创建新的
        if (!pools.ContainsKey(bulletObject.ID))
        {
            var pool = new BulletPool();
            pool.bulletObject = bulletObject;  // 绑定配置
            pools.Add(bulletObject.ID, pool);
        }
        return pools[bulletObject.ID];
    }

    /// <summary>
    /// 便捷方法：直接获取子弹实例
    /// </summary>
    public BulletBehav GetBullet(BulletObject bulletObject)
    {
        var pool = GetPool(bulletObject);
        return pool.GetItem();
    }

    /// <summary>
    /// 便捷方法：回收子弹实例
    /// </summary>
    public void ReleaseBullet(BulletObject bulletObject, BulletBehav bullet)
    {
        var pool = GetPool(bulletObject);
        pool.RealseItem(bullet);
    }

    /// <summary>
    /// 清空所有对象池（场景切换时调用）
    /// </summary>
    public void ClearAllPools()
    {
        foreach (var pool in pools.Values)
        {
            pool.Clear();
        }
        pools.Clear();
    }
}