using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneTransition : MonoBehaviour
{
    public static SceneTransition Instance;

    [SerializeField] private Image fadeImage;
    [SerializeField] private float fadeDuration = 0.5f;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
        DontDestroyOnLoad(gameObject);
    }

    public void LoadScene(string sceneName)
    {
        StartCoroutine(Transition(sceneName));
    }

    private IEnumerator Transition(string sceneName)
    {
        // 黑屏期间阻挡点击
        fadeImage.raycastTarget = true;

        // 淡出（画面变黑）
        yield return StartCoroutine(Fade(1f));

        // 异步加载场景
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
        asyncLoad.allowSceneActivation = false;

        while (asyncLoad.progress < 0.9f)
        {
            yield return null;
        }

        // 场景准备好后，再等一小会儿
        yield return new WaitForSeconds(0.1f);

        asyncLoad.allowSceneActivation = true;

        // 淡入（画面恢复）
        yield return StartCoroutine(Fade(0f));
        // 亮屏后恢复
        fadeImage.raycastTarget = false;
    }

    private IEnumerator Fade(float targetAlpha)
    {
        float startAlpha = fadeImage.color.a;
        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(startAlpha, targetAlpha, elapsed / fadeDuration);
            fadeImage.color = new Color(0, 0, 0, alpha);
            yield return null;
        }

        fadeImage.color = new Color(0, 0, 0, targetAlpha);
    }

    /// <summary>
    /// 黑屏（不切换场景）
    /// </summary>
    public void FadeOut()
    {
        StartCoroutine(Fade(1f));
    }

    /// <summary>
    /// 亮屏（不切换场景）
    /// </summary>
    public void FadeIn()
    {
        StartCoroutine(Fade(0f));
    }

    /// <summary>
    /// 黑屏+亮屏（中间无回调）
    /// </summary>
    public void FadeOutIn()
    {
        StartCoroutine(FadeOutInRoutine());
    }

    private IEnumerator FadeOutInRoutine()
    {
        // 黑屏期间阻挡点击
        fadeImage.raycastTarget = true;

        yield return StartCoroutine(Fade(1f));
        yield return StartCoroutine(Fade(0f));

        // 亮屏后恢复
        fadeImage.raycastTarget = false;
    }
}