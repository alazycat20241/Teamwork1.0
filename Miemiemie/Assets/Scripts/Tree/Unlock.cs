using UnityEngine;
using UnityEngine.UI;
using static SaveData;

/// <summary>
/// 解锁功能组件
/// 点击后隐藏锁定图标，显示可用的功能按钮
/// </summary>
public class Unlock : MonoBehaviour
{
    [SerializeField] private Image img1;    // 锁定图标1
    [SerializeField] private Image img2;    // 锁定图标2
    [SerializeField] private Image img3;    // 锁定图标3
    [SerializeField] private Button butt;   // 解锁后显示的功能按钮1
    [SerializeField] private Button buttt;  // 解锁后显示的功能按钮2
    [SerializeField] private Button butttt; // 解锁后显示的功能按钮3

    private Button buttonn;                 // 当前物体上的解锁按钮


    [Header("解锁消耗")]
    public int goldCost = 0;        // 金币消耗
    public int stoneCost = 0;       // 灵魂石消耗

    // ===== 新增 =====
    public bool IsUnlocked { get; private set; }
    public string TechID => GetFullPath();

    /// <summary>
    /// 初始化组件，注册按钮点击事件
    /// </summary>
    void Awake()
    {
        // 获取当前物体上的Button组件
        buttonn = GetComponent<Button>();

        // 如果按钮存在，则添加点击事件监听
        if (buttonn != null)
            buttonn.onClick.AddListener(onnclick);
    }

    /// <summary>
    /// 解锁按钮点击处理逻辑
    /// 隐藏所有锁定图标，显示所有功能按钮
    /// </summary>
    void onnclick()
    {
        if (PlayerInventory.Instance.playerGold < goldCost ||
            PlayerInventory.Instance.soulStones < stoneCost)
        {
            return;
        }

        IsUnlocked = true;                                        // 新增

        // 隐藏锁定图标，表示该功能已解锁
        img1.gameObject.SetActive(false);
        img2.gameObject.SetActive(false);
        img3.gameObject.SetActive(false);

        // 显示功能按钮，允许玩家使用这些功能
        butt.gameObject.SetActive(true);
        buttt.gameObject.SetActive(true);
        butttt.gameObject.SetActive(true);
    }

    // ===== 新增方法 =====
    public void LoadFromSave(bool unlocked)
    {
        if (unlocked)
        {
            IsUnlocked = true;
            img1.gameObject.SetActive(false);
            img2.gameObject.SetActive(false);
            img3.gameObject.SetActive(false);
            butt.gameObject.SetActive(true);
            buttt.gameObject.SetActive(true);
            butttt.gameObject.SetActive(true);
        }
    }

    public TechData GetSaveData()
    {
        return new TechData { techID = TechID, isUnlocked = IsUnlocked, isPurchased = false };
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
        IsUnlocked = false;
        img1.gameObject.SetActive(true);
        img2.gameObject.SetActive(true);
        img3.gameObject.SetActive(true);
        butt.gameObject.SetActive(false);
        buttt.gameObject.SetActive(false);
        butttt.gameObject.SetActive(false);
    }
}