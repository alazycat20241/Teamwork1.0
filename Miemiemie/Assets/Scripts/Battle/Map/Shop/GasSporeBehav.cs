using UnityEngine;

/// <summary>
/// 毒气孢子：飞到半径边缘停下，定时自毁
/// </summary>
public class GasSporeBehav : MonoBehaviour
{
    public float lifetime = 2f;
    public float radius = 3f;
    public float moveSpeed = 2f;

    private Vector3 center;
    private Vector3 targetPos;
    private float timer = 0f;
    private bool arrived = false;

    void Start()
    {
        center = transform.position;

        // 朝随机方向飞到半径边缘
        Vector2 dir = Random.insideUnitCircle.normalized;
        targetPos = center + (Vector3)(dir * radius);
    }

    void Update()
    {
        timer += Time.deltaTime;
        if (timer >= lifetime)
        {
            Destroy(gameObject);
            return;
        }

        if (!arrived)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPos, moveSpeed * Time.deltaTime);
            if (Vector3.Distance(transform.position, targetPos) < 0.1f)
                arrived = true;
        }
    }
}