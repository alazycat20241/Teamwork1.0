using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// 鼠标悬浮显示子物体，离开隐藏
/// 挂到父物体上，子物体（如 Tooltip）会自动显示/隐藏
/// 所有物体通用
/// </summary>
public class ShowTips: MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("要显示的子物体（留空则显示第一个子物体）")]
    [SerializeField] private GameObject childToShow;

    private void Start()
    {
        // 如果没指定，默认取第一个子物体
        if (childToShow == null && transform.childCount > 0)
            childToShow = transform.GetChild(0).gameObject;

        // 初始隐藏
        if (childToShow != null)
            childToShow.SetActive(false);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (childToShow != null)
            childToShow.SetActive(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (childToShow != null)
            childToShow.SetActive(false);
    }
}