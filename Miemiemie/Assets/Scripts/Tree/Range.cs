using UnityEngine.UI;
using UnityEngine;

public class Range : MonoBehaviour
{
    public float addCount;
    public int GCOUNT;
    public int SCOUNT;
    public Button r;
    
    public Sprite Select1Sprite; 
    private Image img;

    private void Awake()
    {
        img = GetComponent<Image>();
        if (r != null)
            r.onClick.AddListener(click);
    }
    void click()
    {
        
        if (PlayerInventory.Instance.playerGold > GCOUNT &&
            PlayerInventory.Instance.soulStones > SCOUNT)
        {
            
            img.sprite = Select1Sprite;
            PlayerInventory.Instance.playerGold -= GCOUNT;
            PlayerInventory.Instance.soulStones -= SCOUNT;
            PlayerStats.Instance.AddPermanentRange(addCount);
        }
    }
}
