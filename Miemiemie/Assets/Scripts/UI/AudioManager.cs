using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 音频管理器 - 统一管理所有音效和BGM的音量
/// </summary>
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("音量设置（0-1）")]
    [SerializeField] private float masterVolume = 1f;  // 主音量（总控制）
    [SerializeField] private float sfxVolume = 1f;     // 音效音量
    [SerializeField] private float bgmVolume = 1f;     // 背景音乐音量

    [Header("短音效播放器")]
    [SerializeField] private AudioSource sfxPlayer;    // 用于播放一次性短音效的AudioSource

    // 分别存储SFX和BGM的AudioSource列表
    // 用List而不是数组，因为预制体会动态生成和销毁
    private List<AudioSource> sfxSources = new List<AudioSource>();  // 所有音效源
    private List<AudioSource> bgmSources = new List<AudioSource>();  // 所有背景音乐源

    [Header("UI音效")]
    [SerializeField] private AudioClip clickSound;  // 鼠标点击音效
    void Awake()
    {
        // 单例初始化
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);  // 跨场景保留，让音乐不断
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // 短音效播放器
        sfxPlayer = gameObject.GetComponent<AudioSource>();
        sfxPlayer.playOnAwake = false;  // 不要自动播放
        sfxPlayer.loop = false;         // 短音效不循环
    }

    void Update()
    {
        // 只在非Menu场景播放
        if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name != "Menu")
        {
            if (Input.GetMouseButtonDown(0))
            {
                PlaySFX(clickSound);
            }
        }
    }

    // ===================================================
    // 注册与注销（预制体生成/销毁时调用）
    // ===================================================

    /// <summary>
    /// 注册音效源 - 预制体生成时调用
    /// </summary>
    /// <param name="source">预制体上的AudioSource组件</param>
    public void RegisterSFX(AudioSource source)
    {
        // 避免重复注册同一个AudioSource
        if (!sfxSources.Contains(source))
        {
            sfxSources.Add(source);
            // 立即应用当前音量设置
            source.volume = sfxVolume * masterVolume;
        }
    }

    /// <summary>
    /// 注册背景音乐源 - 预制体生成时调用
    /// </summary>
    /// <param name="source">预制体上的AudioSource组件</param>
    public void RegisterBGM(AudioSource source)
    {
        // 避免重复注册
        if (!bgmSources.Contains(source))
        {
            bgmSources.Add(source);
            // 立即应用当前音量设置
            source.volume = bgmVolume * masterVolume;
        }
    }

    /// <summary>
    /// 注销音效源 - 预制体销毁时调用
    /// </summary>
    public void UnregisterSFX(AudioSource source)
    {
        sfxSources.Remove(source);
    }

    /// <summary>
    /// 注销背景音乐源 - 预制体销毁时调用
    /// </summary>
    public void UnregisterBGM(AudioSource source)
    {
        bgmSources.Remove(source);
    }

    // ===================================================
    // 音量控制（UI Slider调用）
    // ===================================================

    /// <summary>
    /// 设置主音量 - Master Slider调用
    /// </summary>
    /// <param name="vol">音量值 0-1</param>
    public void SetMasterVolume(float vol)
    {
        masterVolume = vol;
        // 主音量改变，需要更新所有AudioSource的音量
        UpdateAllVolumes();
    }

    /// <summary>
    /// 设置音效音量 - SFX Slider调用
    /// </summary>
    /// <param name="vol">音量值 0-1</param>
    public void SetSFXVolume(float vol)
    {
        sfxVolume = vol;
        // 移除已经被销毁的空引用（防御性清理）
        sfxSources.RemoveAll(s => s == null);

        // 更新所有音效源的音量
        // 实际音量 = 音效音量 × 主音量
        foreach (AudioSource source in sfxSources)
        {
            if (source != null)
            {
                source.volume = sfxVolume * masterVolume;
            }
        }
    }

    /// <summary>
    /// 设置背景音乐音量 - BGM Slider调用
    /// </summary>
    /// <param name="vol">音量值 0-1</param>
    public void SetBGMVolume(float vol)
    {
        bgmVolume = vol;
        // 移除已经被销毁的空引用
        bgmSources.RemoveAll(s => s == null);

        // 更新所有BGM源的音量
        // 实际音量 = BGM音量 × 主音量
        foreach (AudioSource source in bgmSources)
        {
            if (source != null)
            {
                source.volume = bgmVolume * masterVolume;
            }
        }
    }

    /// <summary>
    /// 更新所有AudioSource的音量（主音量改变时调用）
    /// </summary>
    private void UpdateAllVolumes()
    {
        // 清理空引用
        sfxSources.RemoveAll(s => s == null);
        bgmSources.RemoveAll(s => s == null);

        // 更新所有音效源
        foreach (AudioSource source in sfxSources)
        {
            if (source != null)
            {
                source.volume = sfxVolume * masterVolume;
            }
        }

        // 更新所有BGM源
        foreach (AudioSource source in bgmSources)
        {
            if (source != null)
            {
                source.volume = bgmVolume * masterVolume;
            }
        }
    }

    // ===================================================
    // 播放短音效（翻页音效、UI点击音效等一次性音效）
    // ===================================================

    /// <summary>
    /// 播放一次性短音效
    /// </summary>
    /// <param name="clip">音效片段</param>
    /// <param name="volumeScale">额外音量倍率 0-1，默认为1</param>
    public void PlaySFX(AudioClip clip, float volumeScale = 1f)
    {
        if (clip == null) return;

        // PlayOneShot不干扰当前正在播放的音效，可以叠加播放
        sfxPlayer.PlayOneShot(clip, sfxVolume * masterVolume * volumeScale);
    }

    /// <summary>
    /// 手动清理空引用（可选，定期调用以防止内存泄漏）
    /// </summary>
    public void CleanupNullReferences()
    {
        sfxSources.RemoveAll(s => s == null);
        bgmSources.RemoveAll(s => s == null);
    }

    /// <summary>
    /// 播放一次性短音效（你原来用的方法）
    /// </summary>
    /// <param name="clip">音效片段</param>
    public void PlaySound(AudioClip clip)
    {
        if (clip == null) return;

        // PlayOneShot不干扰当前正在播放的音效，可以叠加播放
        sfxPlayer.PlayOneShot(clip, sfxVolume * masterVolume);
    }
}