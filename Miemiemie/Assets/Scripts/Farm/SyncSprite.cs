using UnityEngine;

[ExecuteAlways]
public class SyncSprite : MonoBehaviour
{
    public SpriteRenderer parentRenderer;

    private SpriteRenderer selfRenderer;

    void Awake()
    {
        selfRenderer = GetComponent<SpriteRenderer>();
        if (parentRenderer == null)
            parentRenderer = GetComponentInParent<SpriteRenderer>();
    }

    void LateUpdate()
    {
        if (parentRenderer && selfRenderer)
            selfRenderer.sprite = parentRenderer.sprite;
    }
}