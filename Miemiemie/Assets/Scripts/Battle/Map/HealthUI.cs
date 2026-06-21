using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 血量UI显示（心形固定位置，每帧轮询血量变化）
/// 1心 = 10HP，满心/半心/空心各有两种样式，奇数位用A，偶数位用B
/// </summary>
public class HealthUI : MonoBehaviour
{
    [Header("心形图片（两种样式交替）")]
    [SerializeField] private Sprite fullHeartA;     // 满心样式A（第1、3、5...颗）
    [SerializeField] private Sprite fullHeartB;     // 满心样式B（第2、4、6...颗）
    [SerializeField] private Sprite halfHeartA;     // 半心样式A
    [SerializeField] private Sprite halfHeartB;     // 半心样式B
    [SerializeField] private Sprite emptyHeartA;    // 空心样式A
    [SerializeField] private Sprite emptyHeartB;    // 空心样式B

    [Header("心形Image列表")]
    [SerializeField] private Image[] hearts;

    private Health targetHealth;   // 玩家血量组件
    private float lastHP;          // 上一帧的血量，用于检测变化
    private bool isReady = false;  // 是否已找到玩家

    void Start()
    {
        // 玩家可能还未生成，协程等待
        StartCoroutine(WaitForPlayer());
    }

    /// <summary>循环等待玩家生成后绑定</summary>
    IEnumerator WaitForPlayer()
    {
        while (targetHealth == null)
        {
            var player = FixedRoomManager.Instance?.GetPlayer();
            if (player != null)
            {
                targetHealth = player.GetComponent<Health>();
                if (targetHealth != null)
                {
                    break;
                }
            }
            yield return new WaitForSeconds(0.2f);  // 每0.2秒检查一次
        }

        isReady = true;
        lastHP = targetHealth.CurrentHealth;
        UpdateHearts();  // 首次刷新
    }

    void Update()
    {
        if (!isReady || targetHealth == null) return;

        // 只有血量变化时才刷新UI
        if (targetHealth.CurrentHealth != lastHP)
        {
            lastHP = targetHealth.CurrentHealth;
            UpdateHearts();
        }
    }

    /// <summary>根据当前血量刷新所有心形图标</summary>
    public void UpdateHearts()
    {
        if (targetHealth == null) return;

        float hp = targetHealth.CurrentHealth;

        for (int i = 0; i < hearts.Length; i++)
        {
            // 奇数位（0、2、4...）用样式A，偶数位（1、3、5...）用样式B
            bool useStyleA = (i % 2 == 0);

            // 第i颗心完全填满需要(i+1)*10点血量
            float heartThreshold = (i + 1) * 10f;

            if (hp >= heartThreshold)
            {
                // 血量足够 → 满心
                hearts[i].sprite = useStyleA ? fullHeartA : fullHeartB;
            }
            else if (hp >= heartThreshold - 5f)
            {
                // 差5HP以内 → 半心
                hearts[i].sprite = useStyleA ? halfHeartA : halfHeartB;
            }
            else
            {
                // 不够 → 空心
                hearts[i].sprite = useStyleA ? emptyHeartA : emptyHeartB;
            }
        }
    }
}