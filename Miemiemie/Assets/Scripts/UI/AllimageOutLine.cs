using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class AllimageOutline : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    [Header("缩放")]
    public float hoverScale = 1.1f;
    public float pressScale = 0.95f;
    private Vector3 originalScale;

    [Header("描边")]
    [ColorUsage(true, true)]
    public Color outlineColor = Color.yellow;
    [Range(0, 0.2f)]
    public float outlineWidth = 0.05f;

    private Material outlineMat;
    private static readonly int OutlineColorID = Shader.PropertyToID("_OutlineColor");
    private static readonly int OutlineWidthID = Shader.PropertyToID("_OutlineWidth");

    private bool isHovering = false;
    private bool isPressing = false;

    void Awake()
    {
        originalScale = transform.localScale;

        Image image = GetComponent<Image>();
        if (image != null && image.material != null)
        {
            outlineMat = Instantiate(image.material);
            image.material = outlineMat;
        }

        DisableOutline();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        isHovering = true;
        UpdateScale();
        EnableOutline();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isHovering = false;
        isPressing = false;
        UpdateScale();
        DisableOutline();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        isPressing = true;
        UpdateScale();
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        isPressing = false;
        UpdateScale();
    }

    void UpdateScale()
    {
        if (isPressing)
            transform.localScale = originalScale * pressScale;
        else if (isHovering)
            transform.localScale = originalScale * hoverScale;
        else
            transform.localScale = originalScale;
    }

    void EnableOutline()
    {
        if (outlineMat != null)
        {
            outlineMat.SetVector(OutlineColorID, outlineColor);
            outlineMat.SetFloat(OutlineWidthID, outlineWidth);
        }
    }

    void DisableOutline()
    {
        if (outlineMat != null)
        {
            outlineMat.SetVector(OutlineColorID, Color.clear);
            outlineMat.SetFloat(OutlineWidthID, 0);
        }
    }
}