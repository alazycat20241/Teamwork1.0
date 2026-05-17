using TMPro;
using UnityEngine;

public class PlayerHarvest : MonoBehaviour
{

    //作物储存
    
    [SerializeField] private TextMeshProUGUI HarvestText;  //UI显示
    


    void OnTriggerEnter2D(Collider2D other)
    {
        
        // 判断是否是 Harvest
        if (other.CompareTag("Harvest"))
        {
            PlayerInventory.Instance.HarvestCount += 1;
            UpdateUI();
            barn();
            Destroy(other.gameObject);
        }
    }
    void UpdateUI()
    {
        HarvestText.text = "Harvest:" + PlayerInventory.Instance.HarvestCount;
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
        if (PlayerInventory.Instance.HarvestCount > 20)
        {
            PlayerInventory.Instance.HarvestCount = 20;
            HarvestText.text = "Harvest:" + PlayerInventory.Instance.HarvestCount + "   Warning! The barn is full.";
        }
    }
}