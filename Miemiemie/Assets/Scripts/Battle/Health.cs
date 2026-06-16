using Spine.Unity;
using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// 通用血量组件
/// 支持 SpriteRenderer 和 MeshRenderer（含 Spine）的受击闪烁
/// 实现 IDamageable 接口
/// </summary>
public class Health : MonoBehaviour, IDamageable
{
    [Header("血量设置")]
    public float maxHealth = 100f;
    public float currentHealth;
    public bool isStoned = false;  // 石化中，不受伤害

    [Header("受击闪烁")]
    [SerializeField] private bool enableHitFlash = true;        // 是否启用闪烁
    [SerializeField] private Color enemyFlashColor = Color.white; // 闪烁颜色
    [SerializeField] private float flashDuration = 0.1f;        // 闪烁持续时间

    [Header("受伤音效")]
    [SerializeField] private AudioClip hurtSound;               // 受伤音效文件

    // 闪烁相关组件
    private SpriteRenderer spriteRenderer;
    private MaterialPropertyBlock propertyBlock;
    private MaterialPropertyBlock meshMpb;                     // MeshRenderer 用的 PropertyBlock
    private Coroutine flashCoroutine;                           // 闪烁协程引用（避免重复启动）

    public event Action OnDeath;          // 死亡事件
    public event Action<float> OnDamaged; // 受伤事件（传递伤害值）

    public float CurrentHealth => currentHealth;
    public float MaxHealth => maxHealth;
    public bool IsDead => currentHealth <= 0f;

    public bool isNextDamageImmune = false;  // 下次伤害免疫标记

    void Awake()
    {
        currentHealth = maxHealth;

        // 初始化 SpriteRenderer 闪烁组件
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null && enableHitFlash)
        {
            propertyBlock = new MaterialPropertyBlock();
            spriteRenderer.GetPropertyBlock(propertyBlock);
        }

        // 初始化 MeshRenderer 闪烁组件（Spine 等）
        meshMpb = new MaterialPropertyBlock();
    }

    /// <summary>
    /// 受到伤害
    /// </summary>
    public void TakeDamage(float damage)
    {
        // 已死亡，不再受伤
        if (IsDead) return;

        // 下次受伤免疫（格挡等）
        if (isNextDamageImmune)
        {
            isNextDamageImmune = false;
            Debug.Log("下次受伤免疫触发，伤害被抵挡");
            return;
        }

        // 石化中的敌人不受伤害
        if (isStoned && CompareTag("Enemy")) return;

        // 触发受击闪烁（自动覆盖旧闪烁，避免材质卡住）
        if (enableHitFlash)
        {
            if (flashCoroutine != null)
                StopCoroutine(flashCoroutine);
            flashCoroutine = StartCoroutine(HitFlash());
        }

        // 玩家受伤处理：播放音效 + 暗角
        if (CompareTag("Player"))
        {
            if (AudioManager.Instance != null && hurtSound != null)
            {
                AudioManager.Instance.PlaySound(hurtSound);
            }

            if (DamageVignette.Instance != null)
            {
                DamageVignette.Instance.TriggerDamageVignette();
            }
        }

        // 触发受伤事件（外部可监听播放动画等）
        OnDamaged?.Invoke(damage);

        // 扣血
        currentHealth -= damage;

        // 检查死亡
        if (currentHealth <= 0f)
        {
            // 护身符：锁血到 5
            if (PropManager.Instance != null && PropManager.Instance.TryUseHuShenFu())
            {
                currentHealth = 5;
                return;
            }

            currentHealth = 0f;
            Die();
        }
    }

    /// <summary>
    /// 死亡处理
    /// </summary>
    private void Die()
    {
        OnDeath?.Invoke();

        // 玩家死亡：返回家园
        if (gameObject.CompareTag("Player"))
        {
            FixedRoomManager.Instance.ReturnToHome(false);
        }

        // 敌人死亡：播放特效
        if (gameObject.CompareTag("Enemy") && EffectPool.Instance != null)
        {
            EffectPool.Instance.PlayAt("EnemyDeath", transform.position);
        }

        // 敌人死亡：清理激光
        EnemyLaser laser = GetComponent<EnemyLaser>();
        if (laser != null)
        {
            laser.CleanupLaser();
        }

        // 禁用物体（而非销毁，方便对象池复用）
        gameObject.SetActive(false);
    }

    /// <summary>
    /// 受击闪烁协程
    /// 同时处理 SpriteRenderer（旧）和 MeshRenderer（Spine/3D）
    /// 每次新受伤会中断旧闪烁重新开始，避免材质卡在闪烁状态
    /// </summary>
    private IEnumerator HitFlash()
    {
        // ============================================
        // 闪烁阶段：设置 _FlashAmount = 1
        // ============================================

        // SpriteRenderer 闪烁（旧版敌人/玩家用）
        if (spriteRenderer != null)
        {
            propertyBlock.SetColor("_FlashColor", enemyFlashColor);
            propertyBlock.SetFloat("_FlashAmount", 1f);
            spriteRenderer.SetPropertyBlock(propertyBlock);
        }

        // MeshRenderer 闪烁（Spine 动画 / 3D 模型用）
        // ★ Spine 闪烁：通过 SkeletonAnimation.Skeleton.Color 改色
        var allSkeletonAnimations = GetComponentsInChildren<SkeletonAnimation>(true);
        foreach (var sa in allSkeletonAnimations)
        {
            if (sa != null)
            {
                sa.Skeleton.SetColor(enemyFlashColor);
            }
        }

        yield return new WaitForSeconds(flashDuration);

        // ============================================
        // 恢复阶段：设置 _FlashAmount = 0
        // ============================================

        // 恢复 SpriteRenderer
        if (spriteRenderer != null)
        {
            propertyBlock.SetFloat("_FlashAmount", 0f);
            spriteRenderer.SetPropertyBlock(propertyBlock);
        }

        // ★ 恢复 Spine 颜色
        foreach (var sa in allSkeletonAnimations)
        {
            if (sa != null)
            {
                sa.Skeleton.SetColor(Color.white);
            }
        }

        // 协程结束，清空引用
        flashCoroutine = null;
    }
}