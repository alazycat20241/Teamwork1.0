using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SpikeTrap : MonoBehaviour
{
    [Header("动画设置")]
    public Animator animator;
    public float appearInterval = 3f;
    public float upDuration = 2f;

    [Header("伤害设置")]
    public int damage = 1;

    private bool canDamage = false;
    private HashSet<GameObject> damagedPlayers = new HashSet<GameObject>();

    void Start()
    {
        if (animator == null)
            animator = GetComponent<Animator>();

        animator.SetBool("IsUp", false);

        StartCoroutine(SpikeCycle());
    }

    IEnumerator SpikeCycle()
    {
        while (true)
        {
            yield return new WaitForSeconds(appearInterval);

            // 升起，清空伤害记录
            animator.SetBool("IsUp", true);
            canDamage = false;
            damagedPlayers.Clear();

            yield return new WaitForSeconds(upDuration);

            // 落下
            animator.SetBool("IsUp", false);
            canDamage = false;
        }
    }

    // 动画第3帧的Event调用
    public void EnableDamage()
    {
        canDamage = true;
        // 开启伤害时立刻检测已经在范围内的玩家
        TryDamagePlayersInRange();
    }

    // Fall动画第1帧的Event调用
    public void DisableDamage()
    {
        canDamage = false;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!canDamage) return;
        TryDamagePlayer(other.gameObject);
    }

    void OnTriggerStay2D(Collider2D other)
    {
        if (!canDamage) return;
        TryDamagePlayer(other.gameObject);
    }

    void TryDamagePlayer(GameObject player)
    {
        if (!player.CompareTag("Player")) return;
        if (damagedPlayers.Contains(player)) return;

        var health = player.GetComponent<Health>();
        if (health != null)
        {
            health.TakeDamage(damage);
            damagedPlayers.Add(player);
            Debug.Log("尖刺造成伤害");
        }
    }

    void TryDamagePlayersInRange()
    {
        // 获取所有在触发器内的碰撞体
        Collider2D[] colliders = Physics2D.OverlapBoxAll(
            transform.position,
            GetComponent<BoxCollider2D>().size,
            0
        );

        foreach (Collider2D col in colliders)
        {
            TryDamagePlayer(col.gameObject);
        }
    }
}