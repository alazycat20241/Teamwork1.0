using System;
using UnityEngine;
using System.Collections;

public class Health : MonoBehaviour,IDamageable
{
    [Header("血量设置")]
    public float maxHealth = 100f;
    public float currentHealth;

    [Header("无敌设置")]
    [SerializeField] private float invincibilityDuration = 0.5f; // 受伤后无敌时间
    private float invincibilityTimer = 0f;
    private bool isInvincible = false;

    [Header("受击闪烁")]
    [SerializeField] private bool enableHitFlash = true;        // 是否启用闪烁
    [SerializeField] private Color enemyFlashColor = Color.white; // 敌人闪烁颜色
    [SerializeField] private float flashDuration = 0.1f;        // 闪烁持续时间

    // 闪烁相关组件
    private SpriteRenderer spriteRenderer;
    private MaterialPropertyBlock propertyBlock;
    private Coroutine flashCoroutine;

    public event Action OnDeath;      // 死亡事件
    public event Action<float> OnDamaged; // 受伤事件（可播放动画、音效）

    public float CurrentHealth => currentHealth;
    public float MaxHealth => maxHealth;
    public bool IsDead => currentHealth <= 0f;

    void Awake()
    {
        currentHealth = maxHealth;

        // 初始化闪烁组件
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null && enableHitFlash)
        {
            propertyBlock = new MaterialPropertyBlock();
            spriteRenderer.GetPropertyBlock(propertyBlock);
        }
    }

    void Update()
    {
        // 无敌倒计时
        if (isInvincible)
        {
            invincibilityTimer -= Time.deltaTime;
            if (invincibilityTimer <= 0f)
            {
                isInvincible = false;
            }
        }
    }

    public void TakeDamage(float damage)
    {
        if (IsDead) return;

        if (enableHitFlash)
        {
            if (flashCoroutine != null)
                StopCoroutine(flashCoroutine);
            flashCoroutine = StartCoroutine(HitFlash());
        }

        currentHealth -= damage;
        OnDamaged?.Invoke(damage);

        if (currentHealth <= 0f)
        {
            currentHealth = 0f;
            Die();
        }
    }

    private void Die()
    {
        OnDeath?.Invoke();
        // 根据对象不同，可以在此处追加逻辑（播放动画、掉落等）
        // 例如敌人可以 Destroy(gameObject); 玩家可以触发复活或游戏结束
        // 判断是不是玩家
        if (gameObject.CompareTag("Player"))
        {
            FixedRoomManager.Instance.ReturnToHome(false);
        }
        // 敌人死亡时播放特效
        if (gameObject.CompareTag("Enemy") && EffectPool.Instance != null)
        {
            EffectPool.Instance.PlayAt("EnemyDeath", transform.position);
        }
        // ========== 如果是敌人，清理激光特效 ==========
        EnemyLaser laser = GetComponent<EnemyLaser>();
        if (laser != null)
        {
            laser.CleanupLaser();
        }
        gameObject.SetActive(false);
    }

    // 闪烁协程
    private IEnumerator HitFlash()
    {
        // 设置闪烁颜色和强度
        propertyBlock.SetColor("_FlashColor", enemyFlashColor);
        propertyBlock.SetFloat("_FlashAmount", 1f);
        spriteRenderer.SetPropertyBlock(propertyBlock);

        // 等待闪烁时间
        yield return new WaitForSeconds(flashDuration);

        // 恢复原样
        propertyBlock.SetFloat("_FlashAmount", 0f);
        spriteRenderer.SetPropertyBlock(propertyBlock);
    }
}
