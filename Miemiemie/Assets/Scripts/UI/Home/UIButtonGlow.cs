using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIButtonGlow : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public Graphic glowImage; // Õœ»Î GlowOutline(Image)

    void Start()
    {
        if (glowImage) glowImage.canvasRenderer.SetAlpha(0);
    }

    public void OnPointerEnter(PointerEventData d)
    {
        if (glowImage) glowImage.CrossFadeAlpha(1f, 0.15f, true);
    }

    public void OnPointerExit(PointerEventData d)
    {
        if (glowImage) glowImage.CrossFadeAlpha(0f, 0.15f, true);
    }
}