using UnityEngine.UI;
using UnityEngine;

public class UI_End : MonoBehaviour
{
    void Start()
    {
        GetComponent<Button>().onClick.AddListener(End);
    }

    void End()
    {
        Application.Quit();

        // 额外加一段：让 Unity 编辑器里也能退出运行模式
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}
