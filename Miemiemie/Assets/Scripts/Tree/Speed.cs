using UnityEngine.UI;
using UnityEngine;

public class Speed : MonoBehaviour
{
    public float addCount;
    public int GCOUNT;
    public int SCOUNT;
    public Button L;
    
    public Sprite Select1Sprite;
    private Image img;

    private void Awake()
    {
        img = GetComponent<Image>();
        if (L != null)
            L.onClick.AddListener(click);
    }
    void click()
    {
       
        if (PlayerInventory.Instance.playerGold > GCOUNT &&
            PlayerInventory.Instance.soulStones > SCOUNT)
        {
           
            img.sprite = Select1Sprite;
            PlayerInventory.Instance.playerGold -= GCOUNT;
            PlayerInventory.Instance.soulStones -= SCOUNT;
            PlayerStats.Instance.AddPermanentSpeed(addCount);
        }
    }
}
