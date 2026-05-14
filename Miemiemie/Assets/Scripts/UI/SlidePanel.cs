using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// 通用面板滑入滑出组件
/// 挂到任意面板上即可实现从下方（或右方）滑入/滑出动画
/// 使用方法：panel.Open() 打开，panel.Close() 关闭
/// </summary>
public class SlidePanel : MonoBehaviour
{
    [Header("动画设置")]
    [SerializeField] private float slideDuration = 0.3f;      // 滑动动画时长（秒）
    [SerializeField] private float slideDistance = 800f;      // 滑动距离（屏幕下方的距离，根据分辨率调整）
    [SerializeField] private bool slideFromBottom = true;     // true=从下方滑入，false=从右方滑入

    private RectTransform rectTransform;                       // 面板的RectTransform，用于控制位置
    private CanvasGroup canvasGroup;                           // 控制面板的透明度、交互和射线遮挡
    private Coroutine currentAnimation;                        // 当前正在播放的动画协程引用
    private bool isOpen = false;                               // 面板当前是否处于打开状态

    // 事件：动画完成时触发，供外部订阅
    public event Action OnOpenComplete;                        // 打开动画播放完毕
    public event Action OnCloseComplete;                       // 关闭动画播放完毕

    // 公开属性
    public bool IsOpen => isOpen;                              // 是否打开中
    public bool IsAnimating => currentAnimation != null;       // 是否正在播放动画

    void Awake()
    {
        // 获取必要组件
        rectTransform = GetComponent<RectTransform>();

        // 尝试获取CanvasGroup，没有则自动添加（用于控制透明度和交互）
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }

        // 初始化：隐藏面板并移到屏幕外
        SetToClosedState();
    }

    /// <summary>
    /// 直接设置为关闭状态（无动画），用于初始化
    /// </summary>
    private void SetToClosedState()
    {
        // 根据滑动方向确定关闭位置
        Vector2 closedPos;
        if (slideFromBottom)
        {
            closedPos = new Vector2(0, -slideDistance);   // 屏幕下方外
        }
        else
        {
            closedPos = new Vector2(slideDistance, 0);    // 屏幕右方外
        }

        // 设置位置、透明度、交互状态
        rectTransform.anchoredPosition = closedPos;
        canvasGroup.alpha = 0f;                            // 完全透明
        canvasGroup.interactable = false;                  // 不可交互
        canvasGroup.blocksRaycasts = false;                // 不阻挡射线
        gameObject.SetActive(false);                       // 隐藏面板
        isOpen = false;
    }

    /// <summary>
    /// 打开面板（从屏幕外滑入）
    /// </summary>
    public void Open()
    {
        // 已打开且无动画播放，忽略
        if (isOpen && currentAnimation == null) return;

        // 如果有正在播放的动画，先停掉
        if (currentAnimation != null)
        {
            StopCoroutine(currentAnimation);
            currentAnimation = null;
        }

        // 激活面板，开始滑入动画
        gameObject.SetActive(true);
        currentAnimation = StartCoroutine(SlideAnimation(GetClosedPosition(), Vector2.zero, true));
    }

    /// <summary>
    /// 关闭面板（滑出屏幕）
    /// </summary>
    public void Close()
    {
        // 已关闭且无动画播放，忽略
        if (!isOpen && currentAnimation == null) return;

        // 如果有正在播放的动画，先停掉
        if (currentAnimation != null)
        {
            StopCoroutine(currentAnimation);
            currentAnimation = null;
        }

        // 开始滑出动画
        currentAnimation = StartCoroutine(SlideAnimation(Vector2.zero, GetClosedPosition(), false));
    }

    /// <summary>
    /// 切换面板开关状态（打开↔关闭）
    /// </summary>
    public void Toggle()
    {
        if (isOpen)
            Close();
        else
            Open();
    }

    /// <summary>
    /// 打开面板（带回调版本）
    /// </summary>
    /// <param name="onComplete">打开动画完成后的回调</param>
    public void Open(Action onComplete)
    {
        OnOpenComplete += onComplete;
        Open();
    }

    /// <summary>
    /// 关闭面板（带回调版本）
    /// </summary>
    /// <param name="onComplete">关闭动画完成后的回调</param>
    public void Close(Action onComplete)
    {
        OnCloseComplete += onComplete;
        Close();
    }

    /// <summary>
    /// 获取面板关闭时的屏幕外位置
    /// </summary>
    /// <returns>关闭位置坐标</returns>
    private Vector2 GetClosedPosition()
    {
        if (slideFromBottom)
        {
            return new Vector2(0, -slideDistance);   // 屏幕下方
        }
        else
        {
            return new Vector2(slideDistance, 0);    // 屏幕右方
        }
    }

    /// <summary>
    /// 滑动动画协程
    /// </summary>
    /// <param name="from">起始位置</param>
    /// <param name="to">目标位置</param>
    /// <param name="isOpening">true=打开动画，false=关闭动画</param>
    IEnumerator SlideAnimation(Vector2 from, Vector2 to, bool isOpening)
    {
        float elapsed = 0f;

        // 设置起始位置
        rectTransform.anchoredPosition = from;

        // 打开时：允许交互和射线遮挡
        // 关闭时：禁止交互和射线遮挡（避免面板在屏幕外时还能被点到）
        canvasGroup.interactable = isOpening;
        canvasGroup.blocksRaycasts = isOpening;

        while (elapsed < slideDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / slideDuration;

            // 使用SmoothStep缓动曲线，让动画有"减速"效果，更自然
            t = Mathf.SmoothStep(0f, 1f, t);

            // 同时插值位置和透明度
            rectTransform.anchoredPosition = Vector2.Lerp(from, to, t);
            canvasGroup.alpha = isOpening ? t : (1f - t);  // 打开时0→1，关闭时1→0

            yield return null;
        }

        // 确保最终状态准确（防止插值误差）
        rectTransform.anchoredPosition = to;
        canvasGroup.alpha = isOpening ? 1f : 0f;

        // 动画结束，清理状态
        currentAnimation = null;
        isOpen = isOpening;

        // 关闭后隐藏面板，节省性能
        if (!isOpening)
        {
            gameObject.SetActive(false);
            // 触发关闭完成事件
            OnCloseComplete?.Invoke();
            OnCloseComplete = null;  // 清除一次性回调，避免内存泄漏
        }
        else
        {
            // 触发打开完成事件
            OnOpenComplete?.Invoke();
            OnOpenComplete = null;
        }
    }
}