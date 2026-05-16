using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EveryLevel : MonoBehaviour
{
    public MapNode[] nodes;                     // 这一层的所有节点

    [Header("节点竖直间距")]
    [SerializeField] float nodeSpacing = 100f;  // 两个节点之间的Y轴间距（像素）

    /// <summary>
    /// 设置这一层所有节点的固定位置
    /// 1个节点时放中间(Y=0)
    /// 2个节点时上下对称分布
    /// </summary>
    public void SetRoomsPosition()
    {
        if (nodes == null || nodes.Length == 0) return;

        if (nodes.Length == 1)
        {
            // 只有一个节点，放在正中间
            SetNodeY(nodes[0], 0);
        }
        else if (nodes.Length == 2)
        {
            // 两个节点，上下对称分布
            float halfSpacing = nodeSpacing / 2f;
            SetNodeY(nodes[0], halfSpacing);   // 上路，Y = +50
            SetNodeY(nodes[1], -halfSpacing);  // 下路，Y = -50
        }
        else
        {
            // 如果超过2个节点，均匀分布
            float totalHeight = nodeSpacing * (nodes.Length - 1);
            float startY = totalHeight / 2f;
            for (int i = 0; i < nodes.Length; i++)
            {
                float y = startY - i * nodeSpacing;
                SetNodeY(nodes[i], y);
            }
        }
    }

    /// <summary>
    /// 设置单个节点的Y坐标，X保持0
    /// </summary>
    private void SetNodeY(MapNode node, float y)
    {
        if (node == null) return;

        RectTransform rt = node.gameObject.GetComponent<RectTransform>();
        if (rt != null)
        {
            rt.anchoredPosition = new Vector2(0, y);
        }
    }
}