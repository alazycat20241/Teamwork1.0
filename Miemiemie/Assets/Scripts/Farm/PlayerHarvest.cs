using TMPro;
using UnityEngine;

public class PlayerHarvest : MonoBehaviour
{
    //作物储存
    public int HarvestCount = 0; //作物数量
    [SerializeField] private TextMeshProUGUI HarvestText;  //UI显示

    void OnTriggerEnter2D(Collider2D other)
    {
        // 判断是否是 Harvest
        if (other.CompareTag("Harvest"))
        {
            HarvestCount += 1;
            UpdateUI();
            barn();
            Destroy(other.gameObject);
        }
    }
    void UpdateUI()
    {
        HarvestText.text = "Harvest:" + HarvestCount;
    }

    /*void Harvest(GameObject target)
    {
        // 增加种子
        if (PlayerInventory.Instance != null)
        {
            PlayerInventory.Instance.AddSeed(SeedGainPerHarvest);
        }
    }*/

    //判断是否超过仓库最大容量20
    void barn()
    {
        if (HarvestCount > 20)
        {
            HarvestCount = 20;
            HarvestText.text = "Harvest:" + HarvestCount + "   Warning! The barn is full.";
        }
    }
}