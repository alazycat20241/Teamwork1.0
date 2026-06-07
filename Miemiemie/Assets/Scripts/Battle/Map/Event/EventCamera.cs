using UnityEngine;

public class EventCamera : MonoBehaviour
{
    public Canvas canvas;
    void Awake()
    {
        if (canvas == null) canvas = GetComponent<Canvas>();
        if (canvas != null)
        {
            canvas.renderMode = RenderMode.ScreenSpaceCamera;
            canvas.worldCamera = Camera.main;
        }
    }
}