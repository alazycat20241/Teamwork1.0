using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(RectTransform))]
public class UIButtonHoverScale : MonoBehaviour,
    IPointerEnterHandler,
    IPointerExitHandler
{
    public float hoverScale = 1.2f;
    public float duration = 0.15f;

    private Vector3 originScale;
    private Coroutine scaleRoutine;

    void Awake()
    {
        originScale = transform.localScale;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        StartScale(originScale * hoverScale);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        StartScale(originScale);
    }

    private void StartScale(Vector3 target)
    {
        if (scaleRoutine != null)
            StopCoroutine(scaleRoutine);

        scaleRoutine = StartCoroutine(ScaleTo(target));
    }

    private System.Collections.IEnumerator ScaleTo(Vector3 target)
    {
        Vector3 start = transform.localScale;
        float t = 0f;

        while (t < 1f)
        {
            t += Time.unscaledDeltaTime / duration;
            transform.localScale = Vector3.Lerp(start, target, t);
            yield return null;
        }

        transform.localScale = target;
    }
}