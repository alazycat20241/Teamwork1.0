using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadAni : MonoBehaviour
{
    [Header("动画设置")]
    public Animator transitionAnimator;      // 淡入淡出动画的 Animator
    public string fadeOutTrigger = "FadeOut"; // 淡出动画的触发参数名
    public float fadeOutDuration;      // 淡出动画时长

    private static LoadAni instance;

    void Awake()
    {
        // 单例模式
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// 场景切换（淡出 → 加载新场景 → 淡入）
    /// </summary>
    /// <param name="sceneIndex">场景的 Build Index</param>
    public static void SwitchToScene(int sceneIndex)
    {
        if (instance != null)
        {
            instance.StartCoroutine(instance.SwitchSceneCoroutine(sceneIndex));
        }
        else
        {
            // 降级方案：直接加载场景
            SceneManager.LoadScene(sceneIndex);
        }
    }

    private IEnumerator SwitchSceneCoroutine(int sceneIndex)
    {
        // 1. 播放淡出动画
        if (transitionAnimator != null)
        {
            transitionAnimator.SetTrigger(fadeOutTrigger);
        }

        // 2. 等待淡出动画完成
        yield return new WaitForSeconds(fadeOutDuration);

        // 3. 异步加载新场景
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneIndex);

        // 等待场景加载完成
        while (!asyncLoad.isDone)
        {
            yield return null;
        }
    }

    /// <summary>
    /// 重载当前场景（淡出 → 重载 → 淡入）
    /// </summary>
    public static void ReloadCurrentScene()
    {
        if (instance != null)
        {
            instance.StartCoroutine(instance.ReloadSceneCoroutine());
        }
        else
        {
            // 降级方案：直接重载场景
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }

    private IEnumerator ReloadSceneCoroutine()
    {
        // 1. 播放淡出动画
        if (transitionAnimator != null)
        {
            transitionAnimator.SetTrigger(fadeOutTrigger);
        }

        // 2. 等待淡出动画完成
        yield return new WaitForSeconds(fadeOutDuration);

        // 3. 获取当前场景索引并重载
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(currentSceneIndex);

        // 等待场景重载完成
        while (!asyncLoad.isDone)
        {
            yield return null;
        }
    }
}
