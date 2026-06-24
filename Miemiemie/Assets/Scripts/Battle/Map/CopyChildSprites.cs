using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 子物体图片复制器
/// 将源 Image 子物体的 Sprite 复制到目标 Image 上
/// 用途：结算面板展示道具图标时，把道具栏的图标复制到结算面板
/// </summary>
public class CopyChildSprites : MonoBehaviour
{
    [Header("源Image（读取它们的子物体Sprite）")]
    [SerializeField] private Image[] sourceImages = new Image[6];   // 道具栏的 6 个槽位

    [Header("目标Image（接收复制的Sprite）")]
    [SerializeField] private Image[] targetImages = new Image[6];   // 结算面板的 6 个图标位

    private void Start()
    {
        CopySprites();
    }
    /// <summary>
    /// 从源Image的子物体读取Sprite，赋值给对应的目标Image
    /// 支持子物体是 SpriteRenderer 或 Image 两种情况
    /// </summary>
    public void CopySprites()
    {
        if (sourceImages == null || targetImages == null) return;
        // 取两者中较小的长度，防止越界
        int count = Mathf.Min(sourceImages.Length, targetImages.Length);

        for (int i = 0; i < count; i++)
        {
            // 源或目标为空 → 跳过
            if (sourceImages[i] == null || targetImages[i] == null) continue;

            // 获取源 Image 的第一个子物体
            Transform child = sourceImages[i].transform.childCount > 0
                ? sourceImages[i].transform.GetChild(0)
                : null;

            if (child != null)
            {
                // 尝试从 SpriteRenderer 获取图片（预制体通常用这个）
                SpriteRenderer sr = child.GetComponent<SpriteRenderer>();
                if (sr != null && sr.sprite != null)
                {
                    targetImages[i].sprite = sr.sprite;
                    continue;  // 拿到了，跳过后面的 Image 检测
                }

                // 兜底：尝试从子物体的 Image 组件获取图片
                Image childImage = child.GetComponent<Image>();
                if (childImage != null && childImage.sprite != null)
                {
                    targetImages[i].sprite = childImage.sprite;
                }
            }
        }
    }
}