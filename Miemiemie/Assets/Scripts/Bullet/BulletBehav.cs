using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletBehav : MonoBehaviour
{
    public float LifeCycle = 5;//生命周期
    public float LinearVelocity=0;//线速度
    public float LinearAcceleration = 0;//线加速度
    public float AngularVelocity = 0;//角速度
    public float AngularAcceleration=0;//角加速度

    public float MaxVelocity=int.MaxValue;//最大速度

    public BulletPool pool;

    private bool isReleased = false;  //防止重复回收

    private void OnEnable()
    {
        isReleased = false;  //每次激活时重置
    }


    private void FixedUpdate()
    {
        // 如果已经回收，不再执行
        if (isReleased) return;

        //更新角速度线速度
        LinearVelocity =Mathf.Clamp(LinearVelocity+LinearAcceleration * Time.fixedDeltaTime,-MaxVelocity,MaxVelocity);
        AngularVelocity += AngularAcceleration * Time.fixedDeltaTime;

        //更新子弹位置
        transform.Translate(LinearVelocity * Vector2.right * Time.fixedDeltaTime,Space.Self);
        transform.rotation*= Quaternion.Euler(new Vector3(0,0,1)*AngularVelocity*Vector2.right*Time.fixedDeltaTime);

        LifeCycle-=Time.fixedDeltaTime;

        if (LifeCycle <= 0)
        {
            ReleaseToPool();  // 改用封装方法
        }
    }

    // ★ 新增：统一的回收方法
    public void ReleaseToPool()
    {
        if (!isReleased)
        {
            isReleased = true;
            pool.RealseItem(this);
        }
    }
}
