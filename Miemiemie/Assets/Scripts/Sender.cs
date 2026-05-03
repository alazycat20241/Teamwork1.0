using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sender : MonoBehaviour
{
    public BulletObject bulletObject;

    private float currentAngle = 0;//当前发射角度
    private float currentAngularVelocity = 0;//角速度
    private float currentTime;

    private void Awake()
    {//创建对象池
        pool=new BulletPool();
        pool.bulletObject=bulletObject;

        currentAngle = bulletObject.InitRotation;//初始旋转
        currentAngularVelocity = bulletObject.SenderAngularVelocity;
    }
    private void FixedUpdate()
    {
        //更新速度
        currentAngularVelocity= Mathf.Clamp(currentAngularVelocity+ bulletObject.SenderAngularAcceleration*Time.fixedDeltaTime,
            -bulletObject.SenderMaxAngularVelocity,bulletObject.SenderMaxAngularVelocity);
        //更新角度
        currentAngle += currentAngularVelocity*Time.fixedDeltaTime;
        //限制角度
        if (Mathf.Abs(currentAngle) > 720f)
        {
            currentAngle -=Mathf.Sign(currentAngle)*360f;
        }

        //更新时间
        currentTime += Time.fixedDeltaTime;
        if (currentTime >= bulletObject.SenderInterval)
        {
            currentTime-=bulletObject.SenderInterval;
            SendbyCount(bulletObject.LineCount,currentAngle);
        }
    }
    private void SendbyCount(int count, float angle)
    {
        float tmpAngle = count % 2 == 0 ? angle+bulletObject.LineAngle / 2: angle;
        //遍历每条线
        for(int i = 0; i< count; ++i)
        {
            tmpAngle += Mathf.Pow(-1, i) * i * bulletObject.LineAngle;
            Send(tmpAngle);
        }
    }

    public BulletPool pool;
    //发射方法
    private void Send(float angle)
    {
        var bh = pool.GetItem();

        bh.gameObject.transform.position = transform.position;
        bh.gameObject.transform.rotation = Quaternion.Euler(0,0,angle);

    }
}
