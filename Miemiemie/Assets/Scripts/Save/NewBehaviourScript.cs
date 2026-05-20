using UnityEngine;
using UnityEngine.UI;

public class OpenSavePanelButton : MonoBehaviour
{
    public GameObject saveLoadPanel;  // 拖入存档界面面板

    void Start()
    {
        GetComponent<Button>().onClick.AddListener(() =>
        {
            saveLoadPanel.SetActive(true);
        });
    }
}