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
            PlayerInventory.Instance.soulStones += 1;
            UpdateUI();
            barn();
            Destroy(other.gameObject);
        }
    }
    void UpdateUI()
    {
        HarvestText.text = "soulStones:" + PlayerInventory.Instance.soulStones;
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
        if (PlayerInventory.Instance.soulStones > 20)
        {
            PlayerInventory.Instance.soulStones = 20;
            HarvestText.text = "soulStones:" + PlayerInventory.Instance.soulStones + "   Warning! The barn is full.";
        }
    }
}