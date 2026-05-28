using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class DropHandler : MonoBehaviour, IDropHandler
{
    public int slotIndex;  // 槽位编号 0-5

    public void OnDrop(PointerEventData eventData)
    {
        GameObject dragged = eventData.pointerDrag;
        if (dragged == null) return;

        // 放到这个槽位
        dragged.transform.SetParent(transform);
        dragged.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;

        // 通知 ShopRoom 道具被拾取
        ShopRoom.Instance.OnPropDropped(dragged, slotIndex);
    }
}