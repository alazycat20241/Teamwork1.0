using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Button))]
public class SelectableButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    [Header("悬浮/选中外观")]
    [SerializeField] private Sprite hoverSprite;           // 悬浮时的图片（带描边的版本）
    [SerializeField] private float hoverScale = 1.1f;      // 悬浮时放大倍数
    [SerializeField] private float animDuration = 0.15f;   // 动画过渡时间

    [Header("选中位移")]
    [SerializeField] private Vector2 slideOffset = new Vector2(0f, -10f);  // 悬浮时往下滑的像素距离

    [Header("按下效果")]
    [SerializeField] private float pressScale = 0.95f;     // 按下时缩小倍数

    // 私有变量
    private Button button;
    private Image image;
    private Sprite normalSprite;            // 原始图片
    private Vector3 normalScale;            // 原始缩放
    private Vector2 normalPosition;         // 原始位置
    private RectTransform rectTransform;
    private bool isHovered = false;         // 鼠标是否悬浮
    private bool isPressed = false;         // 鼠标是否按下
    private Coroutine currentAnimation;     // 当前动画协程

    void Awake()
    {
        // 获取组件
        button = GetComponent<Button>();
        image = GetComponent<Image>();
        rectTransform = GetComponent<RectTransform>();

        // 记录原始状态
        normalSprite = image.sprite;
        normalScale = transform.localScale;
        normalPosition = rectTransform.anchoredPosition;
    }

    // ==================== 鼠标悬浮 ====================

    // 鼠标进入：变大、换图、往下滑
    public void OnPointerEnter(PointerEventData eventData)
    {
        isHovered = true;

        // 停止当前动画，开始悬浮动画
        if (currentAnimation != null)
            StopCoroutine(currentAnimation);

        currentAnimation = StartCoroutine(AnimateToHover());
    }

    // 鼠标离开：恢复原始大小、原始图片、滑回原位
    public void OnPointerExit(PointerEventData eventData)
    {
        isHovered = false;
        isPressed = false;  // 离开时也重置按下状态

        if (currentAnimation != null)
            StopCoroutine(currentAnimation);

        currentAnimation = StartCoroutine(AnimateToNormal());
    }

    // ==================== 鼠标按下/松开 ====================

    // 鼠标按下：缩小
    public void OnPointerDown(PointerEventData eventData)
    {
        isPressed = true;

        if (currentAnimation != null)
            StopCoroutine(currentAnimation);

        currentAnimation = StartCoroutine(AnimateToPress());
    }

    // 鼠标松开：恢复到悬浮状态（如果还在按钮上）
    public void OnPointerUp(PointerEventData eventData)
    {
        isPressed = false;

        if (currentAnimation != null)
            StopCoroutine(currentAnimation);

        // 如果鼠标还在按钮上，恢复到悬浮状态
        if (isHovered)
            currentAnimation = StartCoroutine(AnimateToHover());
        else
            currentAnimation = StartCoroutine(AnimateToNormal());
    }

    // ==================== 动画协程 ====================

    // 动画：到悬浮状态（变大 + 换图 + 下滑）
    private System.Collections.IEnumerator AnimateToHover()
    {
        float elapsed = 0f;

        Vector3 startScale = transform.localScale;
        Vector3 targetScale = normalScale * hoverScale;

        Vector2 startPos = rectTransform.anchoredPosition;
        Vector2 targetPos = normalPosition + slideOffset;

        // 换图
        if (hoverSprite != null)
            image.sprite = hoverSprite;

        while (elapsed < animDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / animDuration);

            transform.localScale = Vector3.Lerp(startScale, targetScale, t);
            rectTransform.anchoredPosition = Vector2.Lerp(startPos, targetPos, t);

            yield return null;
        }

        // 确保最终值
        transform.localScale = targetScale;
        rectTransform.anchoredPosition = targetPos;
    }

    // 动画：到按下状态（缩小，但保持位置和图片不变）
    private System.Collections.IEnumerator AnimateToPress()
    {
        float elapsed = 0f;

        Vector3 startScale = transform.localScale;
        Vector3 targetScale = normalScale * pressScale;

        while (elapsed < animDuration * 0.5f)  // 按下动画快一点
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / (animDuration * 0.5f));

            transform.localScale = Vector3.Lerp(startScale, targetScale, t);

            yield return null;
        }

        transform.localScale = targetScale;
    }

    // 动画：到正常状态（原始大小 + 原始图 + 滑回原位）
    private System.Collections.IEnumerator AnimateToNormal()
    {
        float elapsed = 0f;

        Vector3 startScale = transform.localScale;
        Vector3 targetScale = normalScale;

        Vector2 startPos = rectTransform.anchoredPosition;
        Vector2 targetPos = normalPosition;

        while (elapsed < animDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / animDuration);

            transform.localScale = Vector3.Lerp(startScale, targetScale, t);
            rectTransform.anchoredPosition = Vector2.Lerp(startPos, targetPos, t);

            yield return null;
        }

        // 恢复原图
        image.sprite = normalSprite;

        // 确保最终值
        transform.localScale = targetScale;
        rectTransform.anchoredPosition = targetPos;
    }
}