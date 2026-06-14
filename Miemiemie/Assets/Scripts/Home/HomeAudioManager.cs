using UnityEngine;

/// <summary>
/// 场景级音频管理器（精简版）
/// 负责在地图和商店之间切换BGM
/// </summary>
public class HomeAudioManager : MonoBehaviour
{
    public static HomeAudioManager Instance { get; private set; }

    [Header("音频源")]
    [SerializeField] private AudioSource bgmSource;

    [Header("背景音乐")]
    [SerializeField] private AudioClip mapBGM;      // 地图BGM
    [SerializeField] private AudioClip shopBGM;     // 商店BGM

    [Header("过渡设置")]
    [SerializeField] private float fadeDuration = 0.5f;

    void Awake()
    {
        Instance = this;

        if (bgmSource == null)
        {
            bgmSource = GetComponent<AudioSource>();
            if (bgmSource == null)
                bgmSource = gameObject.AddComponent<AudioSource>();
        }

        bgmSource.loop = true;

        // ★ 注册到全局AudioManager，让Slider控制音量
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.RegisterBGM(bgmSource);
        }
    }

    void Start()
    {
        PlayMapBGM();
    }

    void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    /// <summary>
    /// 播放地图BGM
    /// </summary>
    public void PlayMapBGM()
    {
        if (mapBGM == null || bgmSource.clip == mapBGM) return;
        StartCoroutine(FadeToBGM(mapBGM));
    }

    /// <summary>
    /// 播放商店BGM
    /// </summary>
    public void PlayShopBGM()
    {
        if (shopBGM == null || bgmSource.clip == shopBGM) return;
        StartCoroutine(FadeToBGM(shopBGM));
    }

    private System.Collections.IEnumerator FadeToBGM(AudioClip newClip)
    {
        float startVolume = bgmSource.volume;
        float timer = 0f;

        // 淡出
        while (timer < fadeDuration)
        {
            timer += Time.unscaledDeltaTime;
            bgmSource.volume = Mathf.Lerp(startVolume, 0f, timer / fadeDuration);
            yield return null;
        }

        // 切换
        bgmSource.clip = newClip;
        bgmSource.Play();

        // 淡入
        timer = 0f;
        while (timer < fadeDuration)
        {
            timer += Time.unscaledDeltaTime;
            bgmSource.volume = Mathf.Lerp(0f, startVolume, timer / fadeDuration);
            yield return null;
        }

        bgmSource.volume = startVolume;
    }
}