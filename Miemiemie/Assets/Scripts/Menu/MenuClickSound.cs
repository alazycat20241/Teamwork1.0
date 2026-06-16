using UnityEngine;

/// <summary>
/// 鼠标点击时播放音效（挂在一个常驻物体上，比如 Canvas）
/// </summary>
public class MenuClickSound : MonoBehaviour
{
    [SerializeField] private AudioClip clickSound;
    [SerializeField] private float volume = 1f;

    private AudioSource audioSource;

    void Awake()
    {
        audioSource = gameObject.GetComponent<AudioSource>();
        audioSource.playOnAwake = false;
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            audioSource.PlayOneShot(clickSound, volume);
        }
    }
}