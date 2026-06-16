using UnityEngine;

/// <summary>
/// 激活时从上方掉落到位，之后在原地上下漂浮
/// </summary>
public class DropAndFloat : MonoBehaviour
{
    [Header("掉落")]
    [SerializeField] private float dropDistance = 0.5f;    // 从上方多远掉下来
    [SerializeField] private float dropDuration = 0.3f;    // 掉落耗时

    [Header("漂浮")]
    [SerializeField] private float floatHeight = 0.15f;    // 漂浮幅度
    [SerializeField] private float floatSpeed = 1.5f;      // 漂浮速度

    private Vector3 targetPos;
    private Vector3 startPos;
    private Coroutine currentRoutine;

    void OnEnable()
    {
        // 记录目标位置，从上方开始
        targetPos = transform.position;
        startPos = targetPos + new Vector3(0, dropDistance, 0);
        transform.position = startPos;

        if (currentRoutine != null) StopCoroutine(currentRoutine);
        currentRoutine = StartCoroutine(DropThenFloat());
    }

    void OnDisable()
    {
        if (currentRoutine != null)
        {
            StopCoroutine(currentRoutine);
            currentRoutine = null;
        }
        transform.position = targetPos;
    }

    private System.Collections.IEnumerator DropThenFloat()
    {
        // 掉落
        float t = 0f;
        while (t < dropDuration)
        {
            t += Time.deltaTime;
            transform.position = Vector3.Lerp(startPos, targetPos, t / dropDuration);
            yield return null;
        }
        transform.position = targetPos;

        // 原地漂浮
        while (true)
        {
            float y = targetPos.y + Mathf.Sin(Time.time * floatSpeed) * floatHeight;
            transform.position = new Vector3(targetPos.x, y, targetPos.z);
            yield return null;
        }
    }
}