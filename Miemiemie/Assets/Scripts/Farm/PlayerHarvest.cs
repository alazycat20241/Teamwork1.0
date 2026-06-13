using TMPro;
using UnityEngine;

public class PlayerHarvest : MonoBehaviour
{
    
    void OnTriggerEnter2D(Collider2D other)
    {
        // 判断是否是 Harvest
        if (other.CompareTag("Harvest"))
        {
            PlayerInventory.Instance.soulStones += 2;
            PlayerInventory.Instance.UpdateStone();
            barn();
            Destroy(other.gameObject);
        }
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
            PlayerInventory.Instance.UpdateStone();
        }
    }
}