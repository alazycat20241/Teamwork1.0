using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerShoot : MonoBehaviour
{
    [Header("子弹配置")]
    [SerializeField] private BulletObject fireBallConfig;      // 灼烧火球（按键2）
    [SerializeField] private BulletObject windBulletConfig;    // 风弹（按键3）
    [SerializeField] private BulletObject mudBulletConfig;     // 泥弹（按键4）
    [SerializeField] private float fireRate = 0.15f;           // 射击间隔

    private BulletPool currentPool;        // 当前使用的对象池
    private BulletObject currentBullet;    // 当前子弹配置
    private float fireTimer;

    void Awake()
    {
        // 默认使用火球
        SwitchBullet(fireBallConfig);
    }

    void Update()
    {
        // ========== 切换子弹（按2/3/4） ==========
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            SwitchBullet(fireBallConfig);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            SwitchBullet(windBulletConfig);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            SwitchBullet(mudBulletConfig);
        }

        // ========== 发射子弹 ==========
        fireTimer += Time.deltaTime;

        if (Input.GetMouseButton(1) && fireTimer >= fireRate)
        {
            fireTimer = 0f;
            ShootTowardsMouse();
        }
    }

    /// <summary>
    /// 切换子弹类型
    /// </summary>
    void SwitchBullet(BulletObject newBullet)
    {
        currentBullet = newBullet;
        currentPool = PoolManager.Instance.GetPool(currentBullet);

        // 可选：在控制台显示当前子弹
        Debug.Log($"切换子弹: {currentBullet.name}");

    }

    /// <summary>
    /// 朝鼠标方向发射子弹
    /// </summary>
    void ShootTowardsMouse()
    {
        if (currentPool == null) return;

        // 获取鼠标在世界坐标的位置
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0f;

        // 计算朝向鼠标的方向和角度
        Vector2 direction = (mousePos - transform.position).normalized;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        // 从对象池获取子弹
        BulletBehav bullet = currentPool.GetItem();
        bullet.transform.position = transform.position;
        bullet.transform.rotation = Quaternion.Euler(0, 0, angle);

        Debug.Log($"子弹: {currentBullet.name}, damage={bullet.damage}, targetLayer={bullet.targetLayer.value}, hitEffect={bullet.hitEffectKey}");
    }
}