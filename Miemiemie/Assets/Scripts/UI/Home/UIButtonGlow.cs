using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIButtonGlow : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public Graphic glowImage;
    private bool hasInit;

    void Update()
    {
        if (!hasInit && glowImage != null)
        {
            Color c = glowImage.color;
            c.a = 0f;
            glowImage.color = c;
            hasInit = true;
        }
    }

    public void OnPointerEnter(PointerEventData d)
    {
        if (glowImage != null)
        {
            Color c = glowImage.color;
            c.a = 1f;
            glowImage.color = c;
        }
    }

    public void OnPointerExit(PointerEventData d)
    {
        if (glowImage != null)
        {
            Color c = glowImage.color;
            c.a = 0f;
            glowImage.color = c;
        }
    }
}