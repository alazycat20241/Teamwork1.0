using UnityEngine;

public class EffectAutoRelease : MonoBehaviour
{
    [SerializeField] private string effectKey;
    [SerializeField] private float lifetime = 0.5f;

    private void OnEnable()
    {
        if (EffectPool.Instance != null)
            Invoke(nameof(Release), lifetime);
        else
            Destroy(gameObject, lifetime);
    }

    private void Release()
    {
        EffectPool.Instance?.Release(effectKey, gameObject);
    }
}