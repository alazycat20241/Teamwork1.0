using UnityEngine;

// 必须挂在你的退出按键物体上
public class ExitButton : MonoBehaviour
{
    private void OnMouseDown()
    {
        // 退出打包后的游戏
        Application.Quit();

        // 额外加一段：让 Unity 编辑器里也能退出运行模式
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}