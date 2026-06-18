using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SporeBehav : MonoBehaviour
{
    public float moveSpeed;             // 飞行速度
    public float lifetime;              // 存活时间
    public SporePool pool;              // 所属对象池，用于回收

    private Vector2 moveDirection;      // 飞行方向
    private float timer;                // 计时器
    public float slowdownTime = 0.3f;  // 多少秒后完全停下

    void OnEnable()
    {
        // 激活时重置计时器，随机一个飞行方向
        timer = 0f;
        moveDirection = Random.insideUnitCircle.normalized;
    }

    void Update()
    {
        timer += Time.deltaTime;

        // 速度从100%逐渐降到0%，然后停住
        float speedMultiplier = 1f - Mathf.Clamp01(timer / slowdownTime);
        transform.Translate(moveDirection * moveSpeed * speedMultiplier * Time.deltaTime);

        // 寿命到了就回收
        if (timer >= lifetime)
        {
            pool.ReleaseSpore(gameObject);
        }
    }
}
