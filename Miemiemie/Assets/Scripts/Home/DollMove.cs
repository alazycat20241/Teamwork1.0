using UnityEngine;

public class DollMove : MonoBehaviour
{
    [Header("随机移动设置")]
    [SerializeField] private float moveRadius = 4f;           // 移动范围半径
    [SerializeField] private float moveSpeed = 2f;            // 移动速度
    [SerializeField] private float arriveThreshold = 0.3f;    // 到达目标点的判定距离
    [SerializeField] private float smoothTime = 0.3f;         // 移动平滑时间

    [Header("朝向")]
    [SerializeField] private bool faceMoveDirection = true;   // 是否根据移动方向翻转

    [Header("浮动")]
    [SerializeField] private bool enableFloat = true;
    [SerializeField] private float floatAmplitude = 0.15f;
    [SerializeField] private float floatFrequency = 2f;
    [SerializeField] private Transform floatTarget;          // 用于浮动的额外物体

    private Vector3 velocity = Vector3.zero;
    private Vector3 targetPosition;
    private float baseZ;   // 锁定的 Z 轴值

    void Start()
    {
        // 如果没有指定浮动目标，自动创建一个空物体作为父级
        if (floatTarget == null && transform.childCount > 0)
        {
            GameObject floatObj = new GameObject("FloatPivot");
            floatObj.transform.SetParent(transform);
            floatObj.transform.localPosition = Vector3.zero;
            transform.GetChild(0).SetParent(floatObj.transform);
            floatTarget = floatObj.transform;
        }

        baseZ = transform.position.z;   // 记录初始 Z
        PickNewTarget();
    }

    void Update()
    {
        // ---- 随机移动（XY 平面，Z 锁定） ----
        Vector3 currentPos = transform.position;
        float distance = Vector3.Distance(currentPos, targetPosition);

        if (distance <= arriveThreshold)
        {
            PickNewTarget();
        }
        else
        {
            // 目标位置：只取 targetPosition 的 X 和 Y，Z 保持当前 Z（即 baseZ）
            Vector3 targetXY = new Vector3(targetPosition.x, targetPosition.y, currentPos.z);
            transform.position = Vector3.SmoothDamp(
                currentPos,
                targetXY,
                ref velocity,
                smoothTime,
                moveSpeed
            );
        }

        // ---- 根据移动方向翻转（作用于 floatTarget） ----
        if (faceMoveDirection && floatTarget != null)
        {
            if (Mathf.Abs(velocity.x) > 0.01f)
            {
                Vector3 scale = floatTarget.localScale;
                scale.x = velocity.x > 0 ? Mathf.Abs(scale.x) : -Mathf.Abs(scale.x);
                floatTarget.localScale = scale;
            }
        }

        // ---- 浮动（作用于 floatTarget 的本地 Y） ----
        if (enableFloat && floatTarget != null)
        {
            float floatValue = Mathf.Sin(Time.time * floatFrequency) * floatAmplitude;
            Vector3 localPos = floatTarget.localPosition;
            localPos.y = floatValue;
            floatTarget.localPosition = localPos;
        }
    }

    private void PickNewTarget()
    {
        // 在圆形范围内随机偏移，应用于 XY 平面
        Vector2 randomOffset = Random.insideUnitCircle * moveRadius;
        targetPosition = new Vector3(
            transform.position.x + randomOffset.x,
            transform.position.y + randomOffset.y,
            baseZ   // Z 永远固定
        );
        velocity = Vector3.zero;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, moveRadius);
    }
}


