using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LevelUp : MonoBehaviour
{
    [Header("面板引用")]
    public SlidePanel DollPanel;        // 菜单面板

    [SerializeField] private TextMeshProUGUI DollText;  //UI显示
    [SerializeField] private TextMeshProUGUI StoneText;  //UI显示

    [Header("按钮")]
    public Button OLU;//打开面板
    public Button CLU;//关闭面板
    public Button LUp;      // 合成玩偶按钮



    // Start is called before the first frame update
    private void Awake()
    {
        if (LUp != null)
            LUp.onClick.AddListener(LevelU);
        if (OLU != null)
            OLU.onClick.AddListener(OpenL);
        if (CLU != null)
            CLU.onClick.AddListener(CloseL);
    }

    public void OpenL()
    {
        DollPanel.Open();
        StoneText.text = "" + PlayerInventory.Instance.soulStones;
        DollText.text = "" + PlayerInventory.Instance.DollCount;

    }
    public void CloseL()
    {
        DollPanel.Close();
        PlayerInventory.Instance.UpdateStone();
        DollPlay.Instance.ischange = true;
    }
    public void LevelU()
    {
        Debug.Log("=== LevelU 被调用 ===");
        Debug.Log($"灵魂石: {PlayerInventory.Instance.soulStones}, 行动点: {ActionPointManager.Instance.GetCurrentPoints()}");
        Debug.Log($"DollPlay.Instance: {(DollPlay.Instance != null ? "存在" : "为空")}");

        if (PlayerInventory.Instance.soulStones >= 3)
        {
            Debug.Log("灵魂石够3个");

            if (ActionPointManager.Instance.UseActionPoints(1))
            {
                Debug.Log("行动点消耗成功，开始合成");

                PlayerInventory.Instance.soulStones -= 3;
                PlayerInventory.Instance.DollCount += 1;

                DollPlay.Instance.AddDoll();

                StoneText.text = PlayerInventory.Instance.soulStones.ToString();
                DollText.text = PlayerInventory.Instance.DollCount.ToString();

                Debug.Log($"合成完成，玩偶数量: {PlayerInventory.Instance.DollCount}");
            }
            else
            {
                Debug.Log("行动点不足！");
            }
        }
        else
        {
            Debug.Log($"灵魂石不足！当前: {PlayerInventory.Instance.soulStones}");
        }
    }
}
