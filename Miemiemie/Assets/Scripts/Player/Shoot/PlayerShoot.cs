using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerShoot : MonoBehaviour
{
    public static PlayerShoot Instance { get; private set; }

    [Header("子弹配置")]
    [SerializeField] private BulletObject normalBulletConfig;  //基础攻击（按键1）
    [SerializeField] private BulletObject fireBallConfig;      // 灼烧火球（按键2）
    [SerializeField] private BulletObject windBulletConfig;    // 风弹（按键3）
    [SerializeField] private BulletObject mudBulletConfig;     // 泥弹（按键4）
    [SerializeField] private float fireRate = 0.15f;           // 射击间隔

    private BulletPool currentPool;        // 当前使用的对象池
    private BulletObject currentBullet;    // 当前子弹配置
    private float fireTimer;

    [Header("射程")]
    [SerializeField] private float shootRange = 10f;           // ★ 射程范围
    [SerializeField] private LayerMask enemyLayer;             // ★ 敌人层（用于检测射程内是否有敌人）

    void Awake()
    {
        Instance=this;
        // 默认使用1
        SwitchBullet(normalBulletConfig);
    }

    void Update()
    {
        // ========== 切换子弹（按2/3/4） ==========
        if (Input.GetKeyDown(KeyCode.Alpha1))        // ★ 新增
        {
            SwitchBullet(normalBulletConfig);
        } 
        else if (Input.GetKeyDown(KeyCode.Alpha2))
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
            if (EnemyInRange())
            {
                fireTimer = 0f;
                ShootTowardsMouse();
            }
        }
    }

    /// <summary>
    /// 切换子弹类型
    /// </summary>
    void SwitchBullet(BulletObject newBullet)
    {
        currentBullet = newBullet;
        currentPool = PoolManager.Instance.GetPool(currentBullet);
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
    }

    /// <summary>
    /// 检测射程范围内是否有敌人
    /// </summary>
    bool EnemyInRange()
    {
        Collider2D hit = Physics2D.OverlapCircle(transform.position, shootRange, enemyLayer);
        return hit != null;
    }

    /// <summary>
    /// 加射程
    /// </summary>
    /// <param name="amount"></param>
    public void AddRange(float amount)
    {
        shootRange += amount;
        shootRange = Mathf.Max(shootRange, 1.5f);  // 最小射程不能是0或负数
    }
    /// <summary>
    /// 在 Scene 视图绘制射程范围
    /// </summary>
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, shootRange);
    }
}