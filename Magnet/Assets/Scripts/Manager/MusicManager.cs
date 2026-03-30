using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance;

    [Header("BGM 音频源")]
    public AudioSource bgmAudioSource;

    [Header("设置音乐")]
    public AudioClip mainMenuBGM;    // 场景0：主界面BGM
    public AudioClip levelBGM;       // 场景1+：所有关卡通用BGM

    private AudioClip currentBGM;

    void Awake()
    {
        // 单例 + 跨场景不销毁
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // 获取AudioSource
        bgmAudioSource = GetComponent<AudioSource>();
        bgmAudioSource.loop = true;
    }

    void Start()
    {
        // 第一次进入游戏时自动播放对应BGM
        PlayBGMBySceneIndex(SceneManager.GetActiveScene().buildIndex);
    }

    // 注册场景切换监听
    void OnEnable() { SceneManager.sceneLoaded += OnSceneLoaded; }
    void OnDisable() { SceneManager.sceneLoaded -= OnSceneLoaded; }

    // 场景加载完成后自动切换BGM
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        PlayBGMBySceneIndex(scene.buildIndex);
    }

    /// <summary>
    /// 根据场景索引自动播放BGM
    /// 0 = 主界面
    /// 非0 = 关卡
    /// </summary>
    void PlayBGMBySceneIndex(int sceneIndex)
    {
        if (sceneIndex == 0)
        {
            PlayBGM(mainMenuBGM);
        }
        else
        {
            PlayBGM(levelBGM);
        }
    }

    /// <summary>
    /// 播放BGM（自动替换旧音乐）
    /// </summary>
    public void PlayBGM(AudioClip newBGM)
    {
        if (newBGM == null || newBGM == currentBGM)
            return;

        currentBGM = newBGM;
        bgmAudioSource.clip = newBGM;
        bgmAudioSource.Play();
    }

    // 停止BGM
    public void StopBGM()
    {
        bgmAudioSource.Stop();
        currentBGM = null;
    }
}
