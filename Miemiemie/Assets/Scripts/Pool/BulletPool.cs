using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class BulletPool
{
    private ObjectPool<BulletBehav> pool;          // Unity内置对象池
    public BulletObject bulletObject;              // 子弹配置数据（运动参数来源）

    /// <summary>
    /// 从池中获取一个子弹实例
    /// </summary>
    public BulletBehav GetItem() => pool.Get();

    /// <summary>
    /// 回收子弹到池中
    /// </summary>
    public void RealseItem(BulletBehav bh) => pool.Release(bh);

    /// <summary>
    /// 构造函数：创建对象池并绑定回调方法
    /// </summary>
    public BulletPool()
    {
        pool = new ObjectPool<BulletBehav>(
            OnCreateItem,    // 创建新对象时调用
            OnGetItem,       // 从池中取出时调用
            OnRealseItem,    // 回收时调用
            OnDestroyItem    // 销毁时调用
        );
    }

    /// <summary>
    /// 创建新子弹实例（对象池为空时调用）
    /// </summary>
    private BulletBehav OnCreateItem()
    {
        // 实例化预制体
        var bh = GameObject.Instantiate(bulletObject.prefabs).GetComponent<BulletBehav>();

        // 初始化运动参数（从BulletObject复制）
        InitBullet(bh);

        // 设置所属对象池
        bh.pool = this;

        return bh;
    }

    /// <summary>
    /// 初始化子弹的运动参数
    /// 注意：不覆盖 damage、targetLayer、hitEffectKey
    /// 这些值在预制体Inspector上手动配置
    /// </summary>
    private void InitBullet(BulletBehav bh)
    {
        // ===== 运动参数（从BulletObject复制） =====
        bh.LinearVelocity = bulletObject.LinearVelocity;
        bh.LinearAcceleration = bulletObject.LinearAcceleration;
        bh.AngularVelocity = bulletObject.AngularVelocity;
        bh.AngularAcceleration = bulletObject.AngularAcceleration;
        bh.LifeCycle = bulletObject.LifeCycle;
        bh.MaxVelocity = bulletObject.MaxVelocity;

        // ★ 注意：damage、targetLayer、hitEffectKey 不在这里初始化
        // 这些值在预制体上手动配置，每次 OnGetItem 也不会被覆盖
    }

    /// <summary>
    /// 销毁子弹对象（池子清空时调用）
    /// </summary>
    private void OnDestroyItem(BulletBehav bh)
    {
        if (bh != null && bh.gameObject != null)
        {
            GameObject.Destroy(bh.gameObject);
        }
    }

    /// <summary>
    /// 回收子弹：禁用物体
    /// </summary>
    private void OnRealseItem(BulletBehav bh)
    {
        bh.gameObject.SetActive(false);
    }

    /// <summary>
    /// 取出子弹：重新初始化运动参数并激活
    /// </summary>
    private void OnGetItem(BulletBehav bh)
    {
        if (bh == null || bh.gameObject == null) return;

        // 重新初始化运动参数（防止上次使用残留的值）
        InitBullet(bh);

        // 激活物体
        bh.gameObject.SetActive(true);
    }

    /// <summary>
    /// 清空对象池（场景切换时调用）
    /// </summary>
    public void Clear()
    {
        try
        {
            pool?.Clear();
        }
        catch (MissingReferenceException)
        {
            // 对象已销毁，忽略错误
        }
    }
}