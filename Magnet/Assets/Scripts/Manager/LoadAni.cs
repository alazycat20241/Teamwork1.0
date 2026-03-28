using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadAni : MonoBehaviour
{
    [Header("动画设置")]
    public Animator transitionAnimator;      // 淡入淡出动画的 Animator
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
        transitionAnimator.SetBool("FadeOut",true);

        // 2. 等待淡出动画完成
        yield return new WaitForSeconds(fadeOutDuration);

        transitionAnimator.SetBool("FadeOut", false);
        // 3. 异步加载新场景
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneIndex);

        // 等待场景加载完成
        while (!asyncLoad.isDone)
        {
            yield return null;
        }
    }
}
