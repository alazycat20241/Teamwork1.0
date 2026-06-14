using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DollLevelUp : MonoBehaviour
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
            LUp.onClick.AddListener(LevelUp);
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
    public void LevelUp()
    {
        if (PlayerInventory.Instance.soulStones>2) {
            if (ActionPointManager.Instance.UseActionPoints(1))
            {
                PlayerInventory.Instance.soulStones -= 3;
                PlayerInventory.Instance.DollCount += 1;
                StoneText.text = "" + PlayerInventory.Instance.soulStones;
                DollText.text = "" + PlayerInventory.Instance.DollCount;
            }
        }
    }
}
