using UnityEngine;
using Spine;
using Spine.Unity;
using System.Collections;
public class Trap : MonoBehaviour
{
    [Header("伤害")]
    [SerializeField] private float damage = 10f;

    [Header("Spine 动画")]
    [SerializeField] private SkeletonAnimation skeletonAnimation;
    [SpineAnimation]
    [SerializeField] private string appearAnimation = "xianjing";  // 动画名

    private void Awake()
    {
        // ★ 先设为透明
        if (skeletonAnimation != null)
            skeletonAnimation.Skeleton.SetColor(new Color(1, 1, 1, 0));

        // 下一帧恢复不透明
        StartCoroutine(ShowFirstFrame());
    }
    void Start()
    {
        // 播放一次，不循环
        if (skeletonAnimation != null)
            skeletonAnimation.AnimationState.SetAnimation(0, appearAnimation, false);
    }

    IEnumerator ShowFirstFrame()
    {
        yield return new WaitForSeconds(0.3f);
        if (skeletonAnimation != null)
            skeletonAnimation.Skeleton.SetColor(Color.white);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Health health = other.GetComponent<Health>();
            if (health != null)
                health.TakeDamage(damage);

            Destroy(gameObject);
        }
    }
}