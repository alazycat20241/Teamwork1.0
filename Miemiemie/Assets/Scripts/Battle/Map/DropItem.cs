using UnityEngine;

public class Collectible : MonoBehaviour
{
    public enum CollectibleType { Gold, SoulStone, halfLove,fullLove,Seed }

    [SerializeField] private CollectibleType type;
    [SerializeField] private int amount = 1;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        Health health = other.GetComponent<Health>();
        if (PlayerInventory.Instance == null) return;

        switch (type)
        {
            case CollectibleType.Gold:
                PlayerInventory.Instance.AddGold(amount);
                break;
            case CollectibleType.SoulStone:
                PlayerInventory.Instance.soulStones += amount;
                PlayerInventory.Instance.UpdateStone();
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

        Destroy(gameObject);
    }
}