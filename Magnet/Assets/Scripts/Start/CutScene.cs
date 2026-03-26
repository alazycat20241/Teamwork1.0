using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using UnityEngine.UI;

public class CutScene : MonoBehaviour
{
    private bool isLoading = false;

    // 播放的过场动画
    public Animator RobotAnimator;

    // 公开的按钮引用（可在Inspector中手动绑定）
    public Button targetButton;

    void Start()
    {
        if (targetButton != null)
        {
            targetButton.onClick.AddListener(OnButtonClick);
        }
    }

    // 按钮点击时触发
    public void OnButtonClick()
    {
        if (!isLoading)
        {
            // 播放动画
            if (RobotAnimator != null)
            {
                RobotAnimator.SetTrigger("R");
            }

            isLoading = true;

            // 异步加载场景
            LoadAni.SwitchToScene(1);
        }
    }

    void OnDestroy()
    {
        // 移除监听器，避免内存泄漏
        if (targetButton != null)
        {
            targetButton.onClick.RemoveListener(OnButtonClick);
        }
    }
}