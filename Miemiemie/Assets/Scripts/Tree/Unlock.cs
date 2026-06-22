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

    public bool IsUnlocked { get; private set; }
    public string TechID => GetFullPath();

    /// <summary>
    /// 执行解锁逻辑（由外部调用，跳过扣费）
    /// </summary>
    public void ForceUnlock()
    {
        if (IsUnlocked) return;
        DoUnlock();
    }

    /// <summary>
    /// 实际执行解锁
    /// </summary>
    private void DoUnlock()
    {
        IsUnlocked = true;

        // 隐藏锁定图标，表示该功能已解锁
        if (img1 != null) img1.gameObject.SetActive(false);
        if (img2 != null) img2.gameObject.SetActive(false);
        if (img3 != null) img3.gameObject.SetActive(false);

        // 显示功能按钮，允许玩家使用这些功能
        if (butt != null) butt.gameObject.SetActive(true);
        if (buttt != null) buttt.gameObject.SetActive(true);
        if (butttt != null) butttt.gameObject.SetActive(true);
    }

    // ===== 新增方法 =====
    public void LoadFromSave(bool unlocked)
    {
        if (unlocked)
        {
            IsUnlocked = true;
            if (img1 != null) img1.gameObject.SetActive(false);
            if (img2 != null) img2.gameObject.SetActive(false);
            if (img3 != null) img3.gameObject.SetActive(false);
            if (butt != null) butt.gameObject.SetActive(true);
            if (buttt != null) buttt.gameObject.SetActive(true);
            if (butttt != null) butttt.gameObject.SetActive(true);
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