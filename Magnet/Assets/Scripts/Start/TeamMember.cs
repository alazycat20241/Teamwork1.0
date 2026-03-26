using UnityEngine;
using System.Collections;

public class TeamMenber: MonoBehaviour
{
    [Header("UI设置")]
    public GameObject targetUI;        // 要显示的UI对象
    public float displayDuration = 3f;  // 显示时长（秒）

    private void OnMouseDown()
    {
        StartCoroutine(ShowAndHide());
    }

    IEnumerator ShowAndHide()
    {
        // 显示UI
        if (targetUI != null)
        {
            targetUI.SetActive(true);
        }

        // 等待指定时长
        yield return new WaitForSeconds(displayDuration);

        // 隐藏UI
        if (targetUI != null)
        {
            targetUI.SetActive(false);
        }
    }
}