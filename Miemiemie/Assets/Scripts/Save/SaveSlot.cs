using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SaveSlot : MonoBehaviour
{
    public TextMeshProUGUI slotLabel;   // 显示"存档1"、"存档2"...
    public TextMeshProUGUI infoText;    // 显示天数、时间 或 "空"
    public Button slotButton;           // 点击存档/读档

    private int slotIndex;              // 当前槽位编号
    private SaveLoadPanel panel;        // 父面板引用

    // 刷新槽位显示
    public void Refresh(int index, bool isSaveMode, SaveLoadPanel parentPanel)
    {
        slotIndex = index;
        panel = parentPanel;

        slotLabel.text = $"Save {index + 1}";

        // 有存档：显示信息；无存档：显示"空"
        if (SaveManager.Instance.SlotHasSave(index))
        {
            SaveData data = SaveManager.Instance.GetSlotInfo(index);
            infoText.text = $"Day{data.currentDay} | {data.saveTime}";

            // 读档模式：有存档才能点
            slotButton.interactable = true;
        }
        else
        {
            infoText.text = "Null";

            // 存档模式：空槽位可点；读档模式：不可点
            slotButton.interactable = true;
        }

        // 绑定点击事件
        slotButton.onClick.RemoveAllListeners();
        slotButton.onClick.AddListener(() => panel.OnSlotClicked(slotIndex));
    }
}