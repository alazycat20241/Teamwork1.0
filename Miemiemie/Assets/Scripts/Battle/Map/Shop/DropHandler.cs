using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/// <summary>
/// 道具栏槽位，接收拖拽的道具
/// </summary>
public class DropHandler : MonoBehaviour, IDropHandler
{
    public int slotIndex;        // 槽位编号 0-5
    public int propID = -1;      // 当前槽位里的道具ID，-1表示空

    private Image currentIcon;   // 拖进来的道具图标引用

    /// <summary>
    /// 道具拖到槽位上时调用
    /// </summary>
    public void OnDrop(PointerEventData eventData)
    {
        // 已有道具则不接收
        if (transform.childCount > 0) return;

        GameObject dragged = eventData.pointerDrag;
        if (dragged == null) return;

        // 吸附到槽位
        dragged.transform.SetParent(transform);
        dragged.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;

        // 记录道具图标
        currentIcon = dragged.GetComponent<Image>();

        // 通知商店房间
        if (ShopRoom.Instance != null)
            ShopRoom.Instance.OnPropDropped(dragged, slotIndex);

        // 从商店房间获取被拖拽道具的ID，存到槽位里
        propID = ShopRoom.Instance.GetDraggedPropID(dragged);
    }

    /// <summary>
    /// 记录放入的道具ID
    /// </summary>
    public void SetPropID(int id)
    {
        propID = id;
    }

    /// <summary>
    /// 道具用完变灰
    /// </summary>
    public void GrayOut()
    {
        if (currentIcon != null)
            currentIcon.color = Color.gray;
    }
}