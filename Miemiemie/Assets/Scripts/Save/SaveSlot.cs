using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SaveSlot : MonoBehaviour
{
    public TextMeshProUGUI slotLabel;   // 显示"存档1"、"存档2"...
    public TextMeshProUGUI infoText;    // 显示天数、时间 或 "空"
    public Button slotButton;           // 点击存档/读档

    private int slotIndex;              // 当前槽位编号
    private SaveLoadPanel panel;        // 母面板引用

    [Header("确认弹窗")]
    public GameObject confirmPanel;      // 确认面板（默认隐藏）
    public Button confirmYesButton;      // 确认按钮
    public Button confirmNoButton;       // 取消按钮

    public bool isSave = false;

    private Vector3 originalScale;
    public float hoverScale = 1.1f;
    private void Awake()
    {
        confirmYesButton.onClick.AddListener(OnConfirmYes);
        confirmNoButton.onClick.AddListener(OnConfirmNo);
        confirmPanel.SetActive(false);

        originalScale = transform.localScale;
    }
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

        isSave = isSaveMode;

        // 点击槽位 → 弹出确认窗口，不直接执行
        slotButton.onClick.RemoveAllListeners();
        slotButton.onClick.AddListener(OnSlotClicked);
    }

    void OnSlotClicked()
    {
        confirmPanel.SetActive(true);
    }

    void OnConfirmYes()
    {
        confirmPanel.SetActive(false);

        if (isSave)
        {
            // 存档模式 → 保存
            SaveManager.Instance.SaveToSlot(slotIndex);
        }
        else
        {
            // 读档模式 → 读取
            SaveManager.Instance.LoadFromSlot(slotIndex);
        }

        // 刷新面板
        if (panel != null)
            panel.RefreshAllSlots();
    }

    void OnConfirmNo()
    {
        confirmPanel.SetActive(false);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        transform.localScale = originalScale * hoverScale;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        transform.localScale = originalScale;
    }
}