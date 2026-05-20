using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SaveLoadPanel : MonoBehaviour
{
    public Button closeBtn;             // 关闭按钮
    public SaveSlot[] slots;            // 5个存档槽

    private bool isSaveMode = true;     // true=存档模式，false=读档模式

    public SlidePanel slidePanel;  // 自己的滑动面板

    void Awake()
    {
        closeBtn.onClick.AddListener(() => Close());
    }

    /// <summary>
    /// 打开面板
    /// </summary>
    public void Open()
    {
        slidePanel.Open();
    }

    /// <summary>
    /// 关闭面板（带回调）
    /// </summary>
    public void Close(System.Action onComplete = null)
    {
        slidePanel.Close(onComplete);
    }

    // 切换存档/读档模式
    public void SetMode(bool saveMode)
    {
        isSaveMode = saveMode;
        RefreshAllSlots();
    }

    // 刷新所有槽位
    void RefreshAllSlots()
    {
        for (int i = 0; i < slots.Length; i++)
        {
            slots[i].Refresh(i, isSaveMode, this);
        }
    }

    // 槽位被点击时调用
    public void OnSlotClicked(int index)
    {
        if (isSaveMode)
        {
            // 存档模式：保存并刷新显示
            SaveManager.Instance.SaveToSlot(index);
            RefreshAllSlots();
        }
        else
        {
            // 读档模式：读取并关闭面板
            if (SaveManager.Instance.SlotHasSave(index))
            {
                SaveManager.Instance.LoadFromSlot(index);
                Close();
            }
        }
    }
}