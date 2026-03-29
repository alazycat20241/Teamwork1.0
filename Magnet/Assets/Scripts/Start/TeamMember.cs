using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class TeamMenber: MonoBehaviour
{
    [Header("UI设置")]
    public GameObject targetUI;        // 要显示的UI对象
   
    private Button btn;
    private bool isActive = false;

    void Awake()
    {
        btn = GetComponent<Button>();
        btn.onClick.AddListener(OnClick);
    }

    void OnClick()
    {
        if (isActive)
        {
            targetUI.SetActive(false);
            isActive = false;
        }
        else
        {
            targetUI.SetActive(true);
            isActive = true;
        }
    }

}