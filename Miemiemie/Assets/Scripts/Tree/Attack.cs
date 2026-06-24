using UnityEngine;
using UnityEngine.UI;
using static SaveData;

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

    public Sprite defaultSprite;      

    /// <summary>
    /// 按钮上的图片组件
    /// </summary>
    private Image img;

    public bool IsPurchased { get; private set; }
    public string TechID => GetFullPath();

    /// <summary>
    /// 初始化组件，注册按钮点击事件
    /// </summary>
    private void Awake()
    {
        // 获取当前物体上的Image组件
        img = GetComponent<Image>();
        if(img == null )gameObject.AddComponent<Image>();

        // 如果按钮已绑定，则添加点击事件监听
        if (L != null)
            L.onClick.AddListener(click);
    }

    private void Update()
    {
        if (IsPurchased)
        {
            // 将按钮图片切换为已升级状态
            if (img != null && Select1Sprite != null)
                img.sprite = Select1Sprite;
        }
    }
    /// <summary>
    /// 按钮点击处理逻辑
    /// 检查玩家资源是否足够，执行升级操作
    /// </summary>
    void click()
    {
        if (IsPurchased) return;

        // 检查玩家金币和灵魂石是否满足升级要求
        if (PlayerInventory.Instance.playerGold >= GCOUNT &&
            PlayerInventory.Instance.soulStones >= SCOUNT)
        {
            IsPurchased = true;                                    // 新增

            // 将按钮图片切换为已升级状态
            if (img != null && Select1Sprite != null)
                img.sprite = Select1Sprite;

            // 扣除升级所需资源
            PlayerInventory.Instance.SpendGold(GCOUNT);      // 扣除金币
            PlayerInventory.Instance.SpendSoulStones(SCOUNT);      // 扣除灵魂石

            // 永久增加玩家攻击力
            PlayerStats.Instance.AddPermanentAttack(addCount);

            // ========== 检测子物体里有没有 Unlock 脚本 ==========
            Unlock unlock = GetComponentInChildren<Unlock>();
            if (unlock != null)
            {
                // 调用 ForceUnlock() 解锁功能
                unlock.ForceUnlock();
            }
        }
    }

    public void LoadFromSave(bool purchased)
    {
        if (purchased)
        {
            IsPurchased = true;
            if (img != null && Select1Sprite != null)
                img.sprite = Select1Sprite;
            //if (L != null)
            //    L.interactable = false;
        }
    }

    public TechData GetSaveData()
    {
        return new TechData { techID = TechID, isUnlocked = true, isPurchased = IsPurchased };
    }

    string GetFullPath()
    {
        string path = gameObject.name;
        Transform t = transform.parent;
        while (t != null) { path = t.name + "/" + path; t = t.parent; }
        return path;
    }

    public void ResetState()
    {
        IsPurchased = false;
        if (img != null && defaultSprite != null) img.sprite = defaultSprite;        // 恢复默认图
        if (L != null) L.interactable = true;
    }
}