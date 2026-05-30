using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class ButtonSmoothScale : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    [Header("缩放设置")]
    public float normalScale = 1f;
    public float hoverScale = 1.1f;    // 悬浮放大
    public float pressedScale = 0.9f;  // 按下缩小
    public float transitionSpeed = 8f; // 过渡速度（越大越快，8-12比较舒服）

    private Button button;
    private bool isHovered = false;
    private bool isPressed = false;
    private float targetScale;

    void Start()
    {
        button = GetComponent<Button>();
        targetScale = normalScale;
    }

    void Update()
    {
        // 判断目标大小
        if (!button.interactable)
            targetScale = normalScale;
        else if (isPressed && isHovered)
            targetScale = pressedScale;
        else if (isHovered)
            targetScale = hoverScale;
        else
            targetScale = normalScale;

        // 丝滑过渡
        transform.localScale = Vector3.Lerp(transform.localScale, Vector3.one * targetScale, Time.deltaTime * transitionSpeed);
    }

    public void OnPointerEnter(PointerEventData eventData) => isHovered = true;
    public void OnPointerExit(PointerEventData eventData) => isHovered = false;
    public void OnPointerDown(PointerEventData eventData) => isPressed = true;
    public void OnPointerUp(PointerEventData eventData) => isPressed = false;
}