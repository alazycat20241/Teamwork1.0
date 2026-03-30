using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TeamExit : MonoBehaviour
{
    [Header("UI设置")]
    public GameObject targetUI;        // 要显示的UI对象

    private Button btn;

    void Awake()
    {
        btn = GetComponent<Button>();
        btn.onClick.AddListener(OnClick);
    }

    void OnClick()
    {
        targetUI.SetActive(false);
    }

}
