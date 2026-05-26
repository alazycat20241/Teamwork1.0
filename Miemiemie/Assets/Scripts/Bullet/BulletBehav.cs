using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletBehav : MonoBehaviour
{
    // ========== 运动参数（由BulletPool从BulletObject初始化） ==========
    public float LifeCycle = 5;                    // 生命周期（秒）
    public float LinearVelocity = 0;               // 线速度
    public float LinearAcceleration = 0;           // 线加速度
    public float AngularVelocity = 0;              // 角速度
    public float AngularAcceleration = 0;          // 角加速度
    public float MaxVelocity = int.MaxValue;       // 最大速度

    // ========== 对象池引用 ==========
    public BulletPool pool;                        // 所属对象池

    // ========== ★ 新增：伤害与特效（在预制体Inspector上手动配置） ==========
    public float damage = 10f;                     // 伤害值
    public LayerMask targetLayer;                  // 目标层（玩家或敌人）
    public string hitEffectKey = "BulletHit";      // 击中特效名（对应EffectPool的key）

    // ========== 内部状态 ==========
    private bool isReleased = false;               // 是否已回收（防止重复回收报错）

    /// <summary>
    /// 每次从池中激活时重置状态
    /// </summary>
    private void OnEnable()
    {
        isReleased = false;
    }

    /// <summary>
    /// 每帧固定更新：更新速度、位置、旋转、生命周期
    /// </summary>
    private void FixedUpdate()
    {
        // 已回收则不再更新
        if (isReleased) return;

        // 更新线速度（受加速度影响，限制最大值）
        LinearVelocity = Mathf.Clamp(
            LinearVelocity + LinearAcceleration * Time.fixedDeltaTime,
            -MaxVelocity,
            MaxVelocity
        );

        // 更新角速度
        AngularVelocity += AngularAcceleration * Time.fixedDeltaTime;

        // 更新位置（沿自身右侧移动）
        transform.Translate(LinearVelocity * Vector2.right * Time.fixedDeltaTime, Space.Self);

        // 更新旋转
        transform.rotation *= Quaternion.Euler(
            new Vector3(0, 0, 1) * AngularVelocity * Time.fixedDeltaTime
        );

        // 生命周期倒计时
        LifeCycle -= Time.fixedDeltaTime;

        // 生命周期结束，回收子弹
        if (LifeCycle <= 0)
        {
            ReleaseToPool();
        }
    }

    /// <summary>
    /// 安全回收方法（防止重复回收导致 InvalidOperationException）
    /// </summary>
    public void ReleaseToPool()
    {
        if (!isReleased)
        {
            isReleased = true;
            pool.RealseItem(this);
        }
    }
}