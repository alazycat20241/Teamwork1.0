using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class BagUI : MonoBehaviour
{
    public static BagUI Instance { get; private set; }

    [Header("背包面板")]
    public SlidePanel bagPanel;

    [Header("UI文本")]
    public TextMeshProUGUI resourceText;

    [Header("地图内的按钮")]
    [SerializeField] private Button OpenButton;
    [SerializeField] private Button BackButton;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }
    void OnEnable()
    {
        // 绑定按钮事件
        OpenButton.onClick.AddListener(OpenBag);
        BackButton.onClick.AddListener(CloseBag);
    }
   

    public void OpenBag()
    {
        bagPanel.Open();
        RefreshBag();
    }

    public void CloseBag()
    {
        bagPanel.Close();
    }

    public void RefreshBag()
    {
        var inv = PlayerInventory.Instance;
      

        resourceText.text =
            $"Seed: {inv.seedCount}\n" +
            $"Gold: {inv.playerGold}\n" +
            $"Fertilizer: {inv.soulStones}";
    }
}