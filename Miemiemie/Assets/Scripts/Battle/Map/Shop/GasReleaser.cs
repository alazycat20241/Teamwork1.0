using System.Collections;
using UnityEngine;

/// <summary>
/// 毒气释放器
/// 挂玩家身上，道具06破损时调用
/// </summary>
public class GasReleaser : MonoBehaviour
{
    [SerializeField] private float gasRadius = 3f;
    [SerializeField] private float gasDuration = 2f;
    [SerializeField] private float gasDamage = 4f;  // 总共4伤，分4跳，每跳1

    public void TriggerGas(Vector3 position)
    {
        StartCoroutine(GasCoroutine(position));
    }

    IEnumerator GasCoroutine(Vector3 center)
    {
        float elapsed = 0f;
        float interval = 0.5f;  // 每0.5秒一跳

        while (elapsed < gasDuration)
        {
            Collider2D[] hits = Physics2D.OverlapCircleAll(center, gasRadius);
            foreach (var hit in hits)
            {
                if (hit.CompareTag("Enemy"))
                {
                    Health health = hit.GetComponent<Health>();
                    health?.TakeDamage(gasDamage / (gasDuration / interval));  // 每跳伤害
                }
            }
            elapsed += interval;
            yield return new WaitForSeconds(interval);
        }
    }
}