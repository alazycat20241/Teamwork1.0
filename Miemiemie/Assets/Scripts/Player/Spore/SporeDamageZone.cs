using UnityEngine;

public class SporeDamageZone : MonoBehaviour
{
    [SerializeField] private LayerMask targetLayer;  // 目标层

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (((1 << other.gameObject.layer) & targetLayer) == 0) return;

        Health health = other.GetComponent<Health>();
        if (health != null)
        {
            SporeDamageManager.Instance?.RegisterTarget(health);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (((1 << other.gameObject.layer) & targetLayer) == 0) return;

        Health health = other.GetComponent<Health>();
        if (health != null)
        {
            SporeDamageManager.Instance?.UnregisterTarget(health);
        }
    }
}