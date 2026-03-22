using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class CshakeApply : MonoBehaviour
{
    [Header("缩放设置")]
    public AnimationCurve zoomCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    public float minDistance = 4f;
    public float maxDistance = 8f;
    public float zoomSpeed = 5f;

    /// 相机抖动应用器
    /// 挂载到 Cinemachine Virtual Camera 上，把 CameraShaker 计算的抖动偏移应用到相机
    private CinemachineVirtualCamera vcam;  // 虚拟相机组件
    //private Vector3 originalPosition;        // 相机原本的位置（用于恢复）
    private CinemachineFramingTransposer transposer;
    private float currentChargePercent = 0;
    void Start()
    {
            vcam = GetComponent<CinemachineVirtualCamera>();

            // 如果找到了虚拟相机，记录它的原始本地位置
            if (vcam != null)
            {
                //originalPosition = vcam.transform.localPosition;
            }
            else
            {
                Debug.LogWarning("CameraShakeApplier: 没有找到 CinemachineVirtualCamera 组件！");
            }
    }

        void LateUpdate()
        {
            if (vcam == null) return;

            // 从 CameraShaker 获取当前帧的抖动偏移量
            // (Vector3) 是把 Vector2 转换成 Vector3（Z 轴保持 0
            //Vector3 shakeOffset = (Vector3)CameraShaker.Instance.ShakeOffset;

            // 把相机位置设置为：原始位置 + 抖动偏移
            //vcam.transform.localPosition = originalPosition + shakeOffset;

        // 2. 应用缩放
        if (transposer != null)
        {
            float curveValue = zoomCurve.Evaluate(currentChargePercent);
            float targetDistance = Mathf.Lerp(minDistance, maxDistance, curveValue);
            float currentDistance = transposer.m_CameraDistance;
            transposer.m_CameraDistance = Mathf.Lerp(currentDistance, targetDistance, Time.deltaTime * zoomSpeed);
        }
    }

    public void UpdateCharge(float chargePercent)
    {
        currentChargePercent = Mathf.Clamp01(chargePercent);
    }

    public void ResetZoom()
    {
        currentChargePercent = 0;
    }
}
