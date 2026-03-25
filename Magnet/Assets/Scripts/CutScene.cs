using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class RobotAsyncSceneLoad : MonoBehaviour
{
    // 目标场景的Build索引
    public int targetSceneIndex = 1;
    private AsyncOperation operation;
    private float timer = 0;
    private bool isLoading = false;
    //播放的过场动画
    public Animator CutAnimator;
    public Animator RobotAnimator;

    // 鼠标点击Robot时触发
    private void OnMouseDown()
    {
        if (!isLoading)
        {
            RobotAnimator.SetTrigger("R");
            isLoading = true;
            // 开始异步加载场景，但先不激活
            StartCoroutine(LoadSceneAsync());
        }
    }

    // 异步加载场景协程
    IEnumerator LoadSceneAsync()
    {
        // 异步加载场景，不自动激活
        operation = SceneManager.LoadSceneAsync(targetSceneIndex);
        operation.allowSceneActivation = false;
        CutAnimator.SetTrigger("R");
        yield return operation;
    }

    private void Update()
    {
        // 只有在加载中才计时
        if (isLoading && operation != null)
        {
            // 打印加载进度（0~0.9，0.9代表加载完成）
            Debug.Log("加载进度: " + operation.progress);

            timer += Time.deltaTime;
            // 1秒后激活场景
            if (timer > 1.5f)
            {
                operation.allowSceneActivation = true;
                isLoading = false;
                timer = 0;
            }
        }
    }
}