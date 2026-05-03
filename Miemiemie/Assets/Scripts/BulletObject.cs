using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName="Create BulletAsset")]
public class BulletObject : ScriptableObject
{
    [Header("子弹初始配置")]
    public float LifeCycle=5;//生命周期
    public float LinearVelocity = 0;//线速度
    public float LinearAcceleration = 0;//线加速度
    public float AngularVelocity = 0;//角速度
    public float AngularAcceleration = 0;//角加速度

    public float MaxVelocity = int.MaxValue;//最大速度

    [Header("发射器初始配置")]
    public float InitRotation=0;//初始旋转角
    public float SenderAngularVelocity = 0;//角速度
    public float SenderMaxAngularVelocity=int.MaxValue;//最大角速度
    public float SenderAngularAcceleration = 0;//角加速度

    public int LineCount = 0;//子弹路线条数
    public float LineAngle=30;//子弹路线夹角
    public float SenderInterval=0.01f;//发射间隔

    [Header("预制体")]
    public GameObject prefabs;
}
