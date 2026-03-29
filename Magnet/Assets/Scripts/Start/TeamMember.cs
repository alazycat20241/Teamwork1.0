using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class TeamMenber: MonoBehaviour
{
    [Header("UI设置")]
    public GameObject targetUI;        // 要显示的UI对象
    public float displayDuration = 3f;  // 显示时长（秒）

    private Button btn;

    void Awake()
    {
        btn = GetComponent<Button>();
        btn.onClick.AddListener(OnClick);
    }

    void OnClick()
    {
        StartCoroutine(ShowAndHide());
    }

    IEnumerator ShowAndHide()
    {
        if (targetUI != null)
            targetUI.SetActive(true);

        yield return new WaitForSeconds(displayDuration);

        if (targetUI != null)
            targetUI.SetActive(false);
    }
}