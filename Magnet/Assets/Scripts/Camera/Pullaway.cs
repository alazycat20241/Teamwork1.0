using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pullaway : MonoBehaviour
{
    [Header("缩放设置")]
    public AnimationCurve zoomCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    public float minFOV = 30f;
    public float maxFOV = 60f;
    public float zoomSpeed = 5f;

    private CinemachineVirtualCamera vcam;
    private float targetFOV;

    void Start()
    {
        vcam = GetComponent<CinemachineVirtualCamera>();
        if (vcam != null)
        {
            targetFOV = vcam.m_Lens.FieldOfView;
        }
    }

    void LateUpdate()
    {
        if (vcam == null) return;

        vcam.m_Lens.FieldOfView = Mathf.Lerp(
            vcam.m_Lens.FieldOfView,
            targetFOV,
            Time.deltaTime * zoomSpeed
        );
    }

    public void UpdateCharge(float chargePercent)
    {
        // 曲线控制变化节奏
        float curveValue = zoomCurve.Evaluate(chargePercent);
        targetFOV = Mathf.Lerp(minFOV, maxFOV, curveValue);
    }

    public void ResetZoom()
    {
        targetFOV = minFOV;
    }
}
