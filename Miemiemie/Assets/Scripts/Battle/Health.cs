using System;
using UnityEngine;

public class Health : MonoBehaviour,IDamageable
{
    [Header("血量设置")]
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float currentHealth;

    [Header("无敌设置")]
    [SerializeField] private float invincibilityDuration = 0.5f; // 受伤后无敌时间
    private float invincibilityTimer = 0f;
    private bool isInvincible = false;

    public event Action OnDeath;      // 死亡事件
    public event Action<float> OnDamaged; // 受伤事件（可播放动画、音效）

    public float CurrentHealth => currentHealth;
    public float MaxHealth => maxHealth;
    public bool IsDead => currentHealth <= 0f;

    void Awake()
    {
        currentHealth = maxHealth;
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
        if (isInvincible || IsDead) return;

        currentHealth -= damage;
        OnDamaged?.Invoke(damage);

        if (currentHealth <= 0f)
        {
            currentHealth = 0f;
            Die();
        }
        else
        {
            // 开启无敌
            isInvincible = true;
            invincibilityTimer = invincibilityDuration;
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
            FixedRoomManager.Instance.ReturnToHome();
        }
        gameObject.SetActive(false);
    }
}
