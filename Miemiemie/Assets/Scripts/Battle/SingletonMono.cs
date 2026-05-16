using UnityEngine;

/// <summary>
/// MonoBehaviour单例基类（用于需要挂载到场景物体上的类）
/// </summary>
public class SingletonMono<T> : MonoBehaviour where T : SingletonMono<T>
{
    private static T instance;
    public static T Instance
    {
        get { return instance; }
    }

    protected virtual void Awake()
    {
        if (instance != null)
            Destroy(gameObject);
        else
            instance = (T)this;
        DontDestroyOnLoad(gameObject);
    }

    public static bool isInit
    {
        get { return instance != null; }
    }

    protected virtual void OnDestroy()
    {
        if (instance == this)
        {
            instance = null;
        }
    }
}