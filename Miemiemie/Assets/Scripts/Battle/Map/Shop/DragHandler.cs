using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/// <summary>
/// 道具拖拽处理
/// 拖拽时放大，松手后若未放入槽位则回到原位
/// </summary>
public class DragHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private CanvasGroup canvasGroup;          // 控制射线穿透
    private RectTransform rectTransform;      // 控制位置
    private Vector3 originalScale;            // 原始大小
    private Vector3 originalLocalPos;         // 原始本地位置
    private Transform originalParent;         // 原始父物体

    void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        rectTransform = GetComponent<RectTransform>();
        originalScale = transform.localScale;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        // 记录原位
        originalParent = transform.parent;
        originalLocalPos = rectTransform.localPosition;

        // 不阻挡射线，让下方槽位能接收
        canvasGroup.blocksRaycasts = false;

        // 放大
        transform.localScale = originalScale * 1.3f;
    }

    public void OnDrag(PointerEventData eventData)
    {
        // 跟随鼠标
        rectTransform.position = eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        // 恢复射线阻挡
        canvasGroup.blocksRaycasts = true;

        // 恢复大小
        transform.localScale = originalScale;

        // 如果父物体没变（没被槽位接收），回到原位
        if (transform.parent == originalParent)
        {
            rectTransform.localPosition = originalLocalPos;
        }
    }
}