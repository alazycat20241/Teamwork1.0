using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// 道具拖拽处理（支持 Canvas Screen Space - Camera 模式）
/// 拖拽时放大并跟随鼠标，松手后若未放入槽位则回到原位
/// 鼠标悬浮时显示 HDR 发光描边效果
/// 挂载前提：Image 组件的 Material 使用 UI/ImageOutline Shader 的材质
/// </summary>
public class DragHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerEnterHandler, IPointerExitHandler
{
    // ==================== 组件引用 ====================
    private CanvasGroup canvasGroup;
    private RectTransform rectTransform;
    private Image image;
    private Material outlineMat;
    private Canvas parentCanvas;              // 父级 Canvas，用于坐标转换

    // ==================== 原始状态记录 ====================
    private Vector3 originalScale;
    private Vector3 originalLocalPos;
    private Transform originalParent;

    // ==================== 描边参数 ====================
    [Header("悬浮 HDR 描边设置")]
    [ColorUsage(true, true)]//支持 HDR 和透明度
    public Color outlineColor = Color.yellow; 

    [Tooltip("描边粗细，范围 0 ~ 0.2")]
    [Range(0, 0.2f)]
    public float outlineWidth = 0.05f;

    // Shader 属性 ID
    private static readonly int OutlineColorID = Shader.PropertyToID("_OutlineColor");
    private static readonly int OutlineWidthID = Shader.PropertyToID("_OutlineWidth");

    [Header("悬浮提示")]
    public PropData propData;
    public GameObject tooltipPanel;      // 拖入子物体的提示面板
    public TextMeshProUGUI nameText;     // 拖入名称文本
    public TextMeshProUGUI descText;     // 拖入描述文本

    // ==================== 初始化 ====================
    void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        rectTransform = GetComponent<RectTransform>();
        image = GetComponent<Image>();

        // 获取父级 Canvas（用于屏幕坐标 → 世界坐标转换）
        parentCanvas = GetComponentInParent<Canvas>();

        originalScale = transform.localScale;

        if (image != null && image.material != null)
        {
            outlineMat = Instantiate(image.material);
            image.material = outlineMat;
        }

        DisableOutline();
        tooltipPanel.SetActive(false);
    }

    // ==================== 鼠标悬浮 ====================
    public void OnPointerEnter(PointerEventData eventData)
    {
        EnableOutline();
        if (propData != null)
        {
            nameText.text = propData.propName;
            descText.text = propData.description;
            tooltipPanel.SetActive(true);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        DisableOutline();
        tooltipPanel.SetActive(false);
    }

    // ==================== 拖拽 ====================
    public void OnBeginDrag(PointerEventData eventData)
    {
        tooltipPanel.SetActive(false);

        originalParent = transform.parent;
        originalLocalPos = rectTransform.localPosition;

        // 关闭射线阻挡
        canvasGroup.blocksRaycasts = false;

        // 放大
        transform.localScale = originalScale * 1.3f;

        // 拖拽时关闭描边
        DisableOutline();

        // 拖拽时移到最上层，防止被其他 UI 遮挡
        transform.SetAsLastSibling();
    }

    public void OnDrag(PointerEventData eventData)
    {
        // Screen Space - Camera 模式：将屏幕坐标转为 Canvas 的世界坐标
        if (parentCanvas != null && parentCanvas.worldCamera != null)
        {
            RectTransformUtility.ScreenPointToWorldPointInRectangle(
                rectTransform,                    // 参考矩形
                eventData.position,               // 屏幕坐标
                parentCanvas.worldCamera,         // Canvas 的渲染相机
                out Vector3 worldPoint            // 输出世界坐标
            );
            rectTransform.position = worldPoint;
        }
        else
        {
            // Overlay 模式
            rectTransform.position = eventData.position;
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.blocksRaycasts = true;
        transform.localScale = originalScale*0.8f;

        if (transform.parent == originalParent)
        {
            rectTransform.localPosition = originalLocalPos;
        }
    }

    // ==================== 描边控制 ====================
    private void EnableOutline()
    {
        if (outlineMat != null)
        {
            outlineMat.SetVector(OutlineColorID, outlineColor);
            outlineMat.SetFloat(OutlineWidthID, outlineWidth);
        }
    }

    private void DisableOutline()
    {
        if (outlineMat != null)
        {
            outlineMat.SetVector(OutlineColorID, new Vector4(0, 0, 0, 0));
            outlineMat.SetFloat(OutlineWidthID, 0);
        }
    }
}