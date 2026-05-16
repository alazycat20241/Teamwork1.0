using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 在 Canvas 上用 UI Image 画一条线连接两个 RectTransform
/// </summary>
public class UILine : MonoBehaviour
{
    public RectTransform pointA;   // 起点
    public RectTransform pointB;   // 终点
    public Color color = Color.white;
    public float lineWidth = 3f;

    private Image image;

    void Start()
    {
        image = GetComponent<Image>();
        if (image == null)
            image = gameObject.AddComponent<Image>();

        image.color = color;
        Draw();
    }

    public void SetPoints(RectTransform a, RectTransform b)
    {
        pointA = a;
        pointB = b;
        Draw();
    }

    void Draw()
    {
        if (pointA == null || pointB == null) return;

        // 计算两个点之间的方向和距离
        Vector2 dir = pointB.anchoredPosition - pointA.anchoredPosition;
        float distance = dir.magnitude;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

        // 设置线的 RectTransform
        RectTransform rt = GetComponent<RectTransform>();
        rt.anchoredPosition = pointA.anchoredPosition + dir * 0.5f;  // 放在中点
        rt.sizeDelta = new Vector2(distance, lineWidth);              // 长度和粗细
        rt.localRotation = Quaternion.Euler(0, 0, angle);            // 旋转到正确角度
    }
}