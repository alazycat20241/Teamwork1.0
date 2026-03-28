using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Magnet : MonoBehaviour
{
    public PlayerController.MagneticPole pole = PlayerController.MagneticPole.North;

    // 抖动相关变量
    private bool isShaking = false;
    public float shakeDuration = 0f;
    public float shakeMagnitude = 0f;
    private Vector3 originalPosition;
    private Coroutine continuousShakeCoroutine;

    void Start()
    {
        originalPosition = transform.position;
    }

    void Update()
    {
        // 处理抖动
        if (isShaking)
        {
            if (shakeDuration > 0)
            {
                // 随机偏移，产生抖动效果
                float x = Random.Range(-1f, 1f) * shakeMagnitude;
                float y = Random.Range(-1f, 1f) * shakeMagnitude;
                transform.position = originalPosition + new Vector3(x, y, 0);

                shakeDuration -= Time.deltaTime;
            }
            else
            {
                // 抖动结束，恢复位置
                transform.position = originalPosition;
                isShaking = false;
            }
        }
    }

    /// <summary>
    /// 让磁铁本身产生抖动
    /// </summary>
    /// <param name="duration">抖动持续时间（秒）</param>
    /// <param name="magnitude">抖动强度</param>
    public void Shake(float duration = 0.15f, float magnitude = 0.05f)
    {
        if (!isShaking)
        {
            originalPosition = transform.position;
        }
        isShaking = true;
        shakeDuration = duration;
        shakeMagnitude = magnitude;
    }

    // 添加新方法：开始持续抖动
    public void StartContinuousShake(float magnitude = 0.05f)
    {
        // 停止之前的持续抖动
        StopContinuousShake();

        // 记录原始位置
        originalPosition = transform.position;

        // 开始持续抖动协程
        continuousShakeCoroutine = StartCoroutine(ContinuousShakeCoroutine(magnitude));
    }

    // 添加新方法：停止持续抖动
    public void StopContinuousShake()
    {
        if (continuousShakeCoroutine != null)
        {
            StopCoroutine(continuousShakeCoroutine);
            continuousShakeCoroutine = null;
        }

        // 恢复位置
        transform.position = originalPosition;
        isShaking = false;
    }

    // 添加协程：持续抖动逻辑
    private IEnumerator ContinuousShakeCoroutine(float magnitude)
    {
        while (true)  // 无限循环，直到手动停止
        {
            // 随机偏移
            float x = Random.Range(-1f, 1f) * magnitude;
            float y = Random.Range(-1f, 1f) * magnitude;
            transform.position = originalPosition + new Vector3(x, y, 0);

            // 每帧更新一次
            yield return null;
        }
    }
}
