using UnityEngine;

public class PersistentCanvas : MonoBehaviour
{
    private static PersistentCanvas instance;

    void Awake()
    {
        // 如果已有实例存在，销毁当前对象
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        // 否则设为实例并保留
        instance = this;
        DontDestroyOnLoad(gameObject);
    }
}