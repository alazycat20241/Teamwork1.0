using UnityEngine.UI;
using UnityEngine;
using static Unity.Collections.AllocatorManager;

public class TreePanel : MonoBehaviour
{
    public Button oBtn;
    public Button cBtn;
    public SlidePanel slidePanel;

    
    void Awake()
    {
        cBtn.onClick.AddListener(Close);
        oBtn.onClick.AddListener(Open);
    }

    public void Open()
    {
        slidePanel.Open();
    }
    public void Close()
    {
        slidePanel.Close();
    }
}
