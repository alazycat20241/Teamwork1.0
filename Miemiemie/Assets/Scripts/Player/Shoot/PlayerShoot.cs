using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerShoot : MonoBehaviour
{
    [Header("子弹配置")]
    [SerializeField] private BulletObject bulletObject;
    [SerializeField] private float fireRate = 0.15f;         // 射击间隔

    private BulletPool pool;
    private float fireTimer;

    void Awake()
    {
        pool = PoolManager.Instance.GetPool(bulletObject);
    }

    void Update()
    {
        // 更新时间
        fireTimer += Time.deltaTime;

        // 按下鼠标发射
        if (Input.GetMouseButton(0) && fireTimer >= fireRate)
        {
            fireTimer = 0f;
            ShootTowardsMouse();
        }
    }

    void ShootTowardsMouse()
    {
        // 获取鼠标在世界的坐标
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0f;

        // 计算朝向鼠标的角度
        Vector2 direction = (mousePos - transform.position).normalized;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        // 从对象池拿子弹
        BulletBehav bullet = pool.GetItem();
        bullet.transform.position = transform.position;
        bullet.transform.rotation = Quaternion.Euler(0, 0, angle);
    }
}
