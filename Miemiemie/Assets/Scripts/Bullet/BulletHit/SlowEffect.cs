using UnityEngine;

public class SlowEffect : MonoBehaviour
{
    private float timer;
    private float slowMultiplier;      // 0.75 = 减速25%
    private float originalMoveSpeed;   // 原始速度（只记录一次）
    private bool isSlowed;

    public void ApplySlow(float slowPercent, float duration)
    {
        slowMultiplier = 1f - slowPercent;
        timer = duration;

        if (!isSlowed)
        {
            // ★ 只在第一次记录原始速度
            IMovable movableComp = GetComponent<IMovable>();
            if (movableComp != null)
            {
                originalMoveSpeed = movableComp.GetMoveSpeed();
            }
            isSlowed = true;
        }
    }

    void Update()
    {
        if (timer <= 0)
        {
            if (isSlowed)
            {
                // 恢复速度
                IMovable movableComp = GetComponent<IMovable>();
                if (movableComp != null)
                {
                    movableComp.SetMoveSpeed(originalMoveSpeed);
                }
                isSlowed = false;
                Destroy(this);
            }
            return;
        }

        timer -= Time.deltaTime;

        // ★ 持续减速：用记录的原始速度，不要再读当前速度
        IMovable movable = GetComponent<IMovable>();
        if (movable != null)
        {
            movable.SetMoveSpeed(originalMoveSpeed * slowMultiplier);  // 始终用原始速度
        }
    }

    void OnDestroy()
    {
        if (isSlowed)
        {
            IMovable movableComp = GetComponent<IMovable>();
            if (movableComp != null)
            {
                movableComp.SetMoveSpeed(originalMoveSpeed);
            }
        }
    }
}