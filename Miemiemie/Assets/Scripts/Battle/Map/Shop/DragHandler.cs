using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/// <summary>
/// 道具拖拽处理
/// 拖拽时放大并跟随鼠标，松手后若未放入槽位则回到原位
/// 鼠标悬浮时显示描边效果
/// 挂载前提：Image 组件的 Material 使用 UI/ImageOutline Shader 的材质
/// </summary>
public class DragHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerEnterHandler, IPointerExitHandler
{
    // ==================== 组件引用 ====================
    private CanvasGroup canvasGroup;          // 控制拖拽时射线是否穿透
    private RectTransform rectTransform;      // 控制位置移动
    private Image image;                      // Image 组件，用于获取材质
    private Material outlineMat;              // 当前物体的描边材质实例

    // ==================== 原始状态记录 ====================
    private Vector3 originalScale;            // 原始缩放大小，拖拽结束后恢复
    private Vector3 originalLocalPos;         // 原始本地坐标，回弹用
    private Transform originalParent;         // 原始父物体，用于判断是否被槽位接收

    // ==================== 描边参数（面板可调） ====================
    [Header("悬浮描边设置")]
    [Tooltip("描边颜色")]
    [ColorUsage(true, true)]  // (showAlpha, showEyeDropper, showAlpha, isHDR)
    public Color outlineColor = Color.yellow;

    [Tooltip("描边粗细，范围 0 ~ 0.2")]
    [Range(0, 0.2f)]
    public float outlineWidth = 0.05f;

    // Shader 属性 ID，避免字符串查找，提高性能
    private static readonly int OutlineColorID = Shader.PropertyToID("_OutlineColor");
    private static readonly int OutlineWidthID = Shader.PropertyToID("_OutlineWidth");

    // ==================== 初始化 ====================
    void Awake()
    {
        // 获取必要组件
        canvasGroup = GetComponent<CanvasGroup>();
        rectTransform = GetComponent<RectTransform>();
        image = GetComponent<Image>();

        // 记录原始缩放，拖拽时会放大到 1.3 倍
        originalScale = transform.localScale;

        // 获取 Image 上挂载的材质实例
        // 注意：需要先在 Image 的 Material 栏挂上 UI/ImageOutline 材质
        if (image != null && image.material != null)
        {
            // ✅ ——实例化一份独立材质，互不影响
            outlineMat = Instantiate(image.material);
            image.material = outlineMat;
        }

        // 初始化时关闭描边
        DisableOutline();
    }

    // ==================== 鼠标悬浮 ====================

    /// <summary>
    /// 鼠标进入：开启描边
    /// </summary>
    public void OnPointerEnter(PointerEventData eventData)
    {
        EnableOutline();
    }

    /// <summary>
    /// 鼠标离开：关闭描边
    /// </summary>
    public void OnPointerExit(PointerEventData eventData)
    {
        DisableOutline();
    }

    // ==================== 拖拽 ====================

    /// <summary>
    /// 开始拖拽：记录原位、放大、关闭射线阻挡、关闭描边
    /// </summary>
    public void OnBeginDrag(PointerEventData eventData)
    {
        // 记录原始父物体和本地坐标，用于松手后判断是否回弹
        originalParent = transform.parent;
        originalLocalPos = rectTransform.localPosition;

        // 关闭射线阻挡，让鼠标射线能穿透到下方槽位
        canvasGroup.blocksRaycasts = false;

        // 拖拽时放大到 1.3 倍，视觉上有"拿起"的感觉
        transform.localScale = originalScale * 1.3f;

        // 拖拽时关闭描边，避免干扰
        DisableOutline();
    }

    /// <summary>
    /// 拖拽中：跟随鼠标位置移动
    /// </summary>
    public void OnDrag(PointerEventData eventData)
    {
        // eventData.position 是屏幕坐标，直接赋值给 RectTransform.position 即可跟随
        rectTransform.position = eventData.position;
    }

    /// <summary>
    /// 结束拖拽：恢复射线阻挡、恢复大小、若未被槽位接收则回到原位
    /// </summary>
    public void OnEndDrag(PointerEventData eventData)
    {
        // 恢复射线阻挡
        canvasGroup.blocksRaycasts = true;

        // 恢复原始大小
        transform.localScale = originalScale;

        // 判断是否被槽位接收：
        // 如果父物体没变，说明没有被任何槽位吸纳，回到原位
        if (transform.parent == originalParent)
        {
            rectTransform.localPosition = originalLocalPos;
        }
        // 如果父物体变了，说明已经被槽位接收，不需要回弹
        // 注意：此时 originalParent 还是旧值，下一帧 OnBeginDrag 会更新
    }

    // ==================== 描边辅助方法 ====================

    /// <summary>
    /// 开启描边：设置颜色和宽度
    /// </summary>
    private void EnableOutline()
    {
        if (outlineMat != null)
        {
            //outlineMat.SetColor(OutlineColorID, outlineColor);
            // ✅ SetVector 保留原始 float 值，HDR 发光生效
            outlineMat.SetVector(OutlineColorID, outlineColor);

            outlineMat.SetFloat(OutlineWidthID, outlineWidth);
        }
    }

    /// <summary>
    /// 关闭描边：颜色透明 + 宽度归零
    /// </summary>
    private void DisableOutline()
    {
        if (outlineMat != null)
        {
            // Alpha 设为 0，描边完全透明
            outlineMat.SetColor(OutlineColorID, new Color(0, 0, 0, 0));
            outlineMat.SetFloat(OutlineWidthID, 0);
        }
    }
}