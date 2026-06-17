using UnityEngine;
using UnityEngine.UI;

public class CopyChildSprites : MonoBehaviour
{
    public static CopyChildSprites Instance;  // ★ 单例

    [Header("源Image（读取它们的子物体Sprite）")]
    [SerializeField] private Image[] sourceImages = new Image[6];

    [Header("目标Image（接收复制的Sprite）")]
    [SerializeField] private Image[] targetImages = new Image[6];

    void Awake()
    {
        Instance = this;  // ★ 初始化单例
    }

    /// <summary>
    /// 从源Image的子物体读取Sprite，赋值给对应的目标Image
    /// </summary>
    public void CopySprites()
    {
        if (sourceImages == null || targetImages == null) return;

        int count = Mathf.Min(sourceImages.Length, targetImages.Length);

        for (int i = 0; i < count; i++)
        {
            if (sourceImages[i] == null || targetImages[i] == null) continue;

            // 获取源Image的子物体
            Transform child = sourceImages[i].transform.childCount > 0
                ? sourceImages[i].transform.GetChild(0)
                : null;

            if (child != null)
            {
                // 先尝试SpriteRenderer
                SpriteRenderer sr = child.GetComponent<SpriteRenderer>();
                if (sr != null && sr.sprite != null)
                {
                    targetImages[i].sprite = sr.sprite;
                    continue;
                }

                // 再尝试子物体的Image
                Image childImage = child.GetComponent<Image>();
                if (childImage != null && childImage.sprite != null)
                {
                    targetImages[i].sprite = childImage.sprite;
                }
            }
            // 没有子物体：跳过，目标Image保持不变
        }
    }
}