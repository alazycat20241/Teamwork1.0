using UnityEngine;

public class Collectible : MonoBehaviour
{
    public enum CollectibleType { Gold, SoulStone, halfLove,fullLove,Seed }

    [SerializeField] private CollectibleType type;
    [SerializeField] private int amount = 1;

    [SerializeField] private AudioClip collectSound;
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        if (collectSound != null && AudioManager.Instance != null)//音效
        {
            AudioManager.Instance.PlaySound(collectSound);
        }

        Health health = other.GetComponent<Health>();
        if (PlayerInventory.Instance == null) return;

        switch (type)
        {
            case CollectibleType.Gold:
                PlayerInventory.Instance.AddGold(amount);
                // 记录本次地图收集的金币
                FixedRoomManager.Instance?.AddCollectedGold(amount);
                break;
            case CollectibleType.SoulStone:
                PlayerInventory.Instance.AddStone(amount); 
                PlayerInventory.Instance.UpdateStone();
                // 记录本次地图收集的灵魂石
                FixedRoomManager.Instance?.AddCollectedSoulStone(amount);
                break;
            case CollectibleType.halfLove:
                if(health!=null)health.currentHealth += 5;
                break;
            case CollectibleType.fullLove:
                if(health != null)health.currentHealth += 10;
                break;
            case CollectibleType.Seed:
                PlayerInventory.Instance.seedCount += amount;
                PlayerInventory.Instance.UpdateStone();
                break;
        }
        gameObject.SetActive(false);
    }
}