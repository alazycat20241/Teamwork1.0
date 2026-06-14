using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 音量设置UI - 挂在有Slider的Canvas上
/// </summary>
public class VolumeSettings : MonoBehaviour
{
    [Header("音量滑块")]
    [Tooltip("主音量滑块")]
    public Slider masterSlider;

    [Tooltip("音效音量滑块")]
    public Slider sfxSlider;

    [Tooltip("背景音乐音量滑块")]
    public Slider bgmSlider;

    // PlayerPrefs的键名常量
    private const string MASTER_VOLUME_KEY = "MasterVolume";
    private const string SFX_VOLUME_KEY = "SFXVolume";
    private const string BGM_VOLUME_KEY = "BGMVolume";

    void Start()
    {
        // ========== 加载存档的音量设置 ==========
        // 如果没有存档过，默认值为1（最大音量）
        float savedMaster = PlayerPrefs.GetFloat(MASTER_VOLUME_KEY, 1f);
        float savedSFX = PlayerPrefs.GetFloat(SFX_VOLUME_KEY, 1f);
        float savedBGM = PlayerPrefs.GetFloat(BGM_VOLUME_KEY, 1f);

        // 设置Slider的初始值
        if (masterSlider != null) masterSlider.value = savedMaster;
        if (sfxSlider != null) sfxSlider.value = savedSFX;
        if (bgmSlider != null) bgmSlider.value = savedBGM;

        // ========== 绑定Slider值改变事件 ==========
        if (masterSlider != null)
        {
            // 添加监听器：当Slider值改变时，调用OnMasterVolumeChanged
            masterSlider.onValueChanged.AddListener(OnMasterVolumeChanged);
        }

        if (sfxSlider != null)
        {
            sfxSlider.onValueChanged.AddListener(OnSFXVolumeChanged);
        }

        if (bgmSlider != null)
        {
            bgmSlider.onValueChanged.AddListener(OnBGMVolumeChanged);
        }

        // ========== 立即应用存档的音量 ==========
        ApplySavedVolumes();
    }

    /// <summary>
    /// 应用存档的音量设置到AudioManager
    /// </summary>
    private void ApplySavedVolumes()
    {
        if (AudioManager.Instance == null) return;

        float savedMaster = PlayerPrefs.GetFloat(MASTER_VOLUME_KEY, 1f);
        float savedSFX = PlayerPrefs.GetFloat(SFX_VOLUME_KEY, 1f);
        float savedBGM = PlayerPrefs.GetFloat(BGM_VOLUME_KEY, 1f);

        AudioManager.Instance.SetMasterVolume(savedMaster);
        AudioManager.Instance.SetSFXVolume(savedSFX);
        AudioManager.Instance.SetBGMVolume(savedBGM);
    }

    // ========== Slider回调函数 ==========

    /// <summary>
    /// 主音量Slider改变时调用
    /// </summary>
    /// <param name="value">Slider当前值 0-1</param>
    private void OnMasterVolumeChanged(float value)
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetMasterVolume(value);
        }
        // 保存到PlayerPrefs，下次启动时读取
        PlayerPrefs.SetFloat(MASTER_VOLUME_KEY, value);
        PlayerPrefs.Save();  // 立即写入磁盘
    }

    /// <summary>
    /// 音效音量Slider改变时调用
    /// </summary>
    private void OnSFXVolumeChanged(float value)
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetSFXVolume(value);
        }
        PlayerPrefs.SetFloat(SFX_VOLUME_KEY, value);
        PlayerPrefs.Save();
    }

    /// <summary>
    /// 背景音乐音量Slider改变时调用
    /// </summary>
    private void OnBGMVolumeChanged(float value)
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetBGMVolume(value);
        }
        PlayerPrefs.SetFloat(BGM_VOLUME_KEY, value);
        PlayerPrefs.Save();
    }
}