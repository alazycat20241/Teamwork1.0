using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 攻击力升级按钮组件
/// 玩家花费金币和灵魂石来永久提升攻击力
/// </summary>
public class Attack : MonoBehaviour
{
    /// <summary>
    /// 攻击力提升的数值
    /// </summary>
    public float addCount;

    /// <summary>
    /// 升级所需的金币数量
    /// </summary>
    public int GCOUNT;

    /// <summary>
    /// 升级所需的灵魂石数量
    /// </summary>
    public int SCOUNT;

    /// <summary>
    /// 绑定的升级按钮
    /// </summary>
    public Button L;

    /// <summary>
    /// 升级后显示的图片（已升级状态）
    /// </summary>
    public Sprite Select1Sprite;

    /// <summary>
    /// 按钮上的图片组件
    /// </summary>
    private Image img;

    /// <summary>
    /// 初始化组件，注册按钮点击事件
    /// </summary>
    private void Awake()
    {
        // 获取当前物体上的Image组件
        img = GetComponent<Image>();

        // 如果按钮已绑定，则添加点击事件监听
        if (L != null)
            L.onClick.AddListener(click);
    }

    /// <summary>
    /// 按钮点击处理逻辑
    /// 检查玩家资源是否足够，执行升级操作
    /// </summary>
    void click()
    {
        // 检查玩家金币和灵魂石是否满足升级要求
        if (PlayerInventory.Instance.playerGold > GCOUNT &&
            PlayerInventory.Instance.soulStones > SCOUNT)
        {
            // 将按钮图片切换为已升级状态
            img.sprite = Select1Sprite;

            // 扣除升级所需资源
            PlayerInventory.Instance.playerGold -= GCOUNT;      // 扣除金币
            PlayerInventory.Instance.soulStones -= SCOUNT;      // 扣除灵魂石

            // 永久增加玩家攻击力
            PlayerStats.Instance.AddPermanentAttack(addCount);
        }
        // 如果资源不足，不做任何操作（无法升级）
    }
}