using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class BulletPool
{
    private ObjectPool<BulletBehav> pool;
    public BulletObject bulletObject;

    public BulletBehav GetItem()=>pool.Get();
    public void RealseItem(BulletBehav bh) => pool.Release(bh);



    public BulletPool()
    {
        pool=new ObjectPool<BulletBehav> (OnCreateItem,OnGetItem,OnRealseItem,OnDestroyItem);
    }



    //创建时调用
    private BulletBehav OnCreateItem()
    {
        var bh = GameObject.Instantiate(bulletObject.prefabs).GetComponent<BulletBehav>();
        InitBullet(bh);
        bh.pool = this;
        return bh;
    }
    private void InitBullet(BulletBehav bh)
    {
        bh.LinearVelocity = bulletObject.LinearVelocity;
        bh.LinearAcceleration = bulletObject.LinearAcceleration;
        bh.AngularVelocity = bulletObject.AngularVelocity;
        bh.AngularAcceleration = bulletObject.AngularAcceleration;
        bh.LifeCycle = bulletObject.LifeCycle;
        bh.MaxVelocity = bulletObject.MaxVelocity;
    }

    private void OnDestroyItem(BulletBehav bh)
    {
        if (bh != null && bh.gameObject != null)
        {
            GameObject.Destroy(bh.gameObject);
        }
    }
    private void OnRealseItem(BulletBehav bh)
    {
        bh.gameObject.SetActive(false);
    }
    private void OnGetItem(BulletBehav bh)
    {
        if (bh == null || bh.gameObject == null) return;
        InitBullet(bh); 
        bh.gameObject.SetActive(true);
    }

    public void Clear()
    {
        try
        {
            pool?.Clear();
        }
        catch (MissingReferenceException)
        {
            // 对象已销毁，忽略
        }
    }
}
