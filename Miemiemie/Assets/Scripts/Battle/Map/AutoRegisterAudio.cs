using UnityEngine;

/// <summary>
/// 自动注册音频源 - 挂在预制体上，生成/销毁时自动注册/注销AudioSource
/// </summary>
public class AutoRegisterAudio : MonoBehaviour
{
    // 音频类型枚举
    public enum AudioType
    {
        SFX,  // 音效（受SFX Slider控制）
        BGM   // 背景音乐（受BGM Slider控制）
    }

    [Header("音频类型")]
    public AudioType audioType = AudioType.BGM;  // 默认为BGM

    // 缓存AudioSource组件引用
    private AudioSource audioSource;

    void Awake()
    {
        // 获取自身AudioSource组件（预制体自带的那个）
        audioSource = GetComponent<AudioSource>();
    }

    void Start()
    {
        // 安全检查
        if (audioSource == null)
        {
            return;
        }

        if (AudioManager.Instance == null)
        {
            return;
        }

        // 根据类型注册到对应的列表
        if (audioType == AudioType.BGM)
        {
            AudioManager.Instance.RegisterBGM(audioSource);
        }
        else
        {
            AudioManager.Instance.RegisterSFX(audioSource);
        }
    }

    void OnDestroy()
    {
        // 预制体销毁时，从AudioManager的列表中移除
        // 防止空引用留在列表中造成内存泄漏
        if (audioSource == null) return;
        if (AudioManager.Instance == null) return;

        if (audioType == AudioType.BGM)
        {
            AudioManager.Instance.UnregisterBGM(audioSource);
        }
        else
        {
            AudioManager.Instance.UnregisterSFX(audioSource);
        }
    }
}