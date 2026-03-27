using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SecneChange : MonoBehaviour
{
    private int currentSceneIndex;

    void Start()
    {
        // 获取当前场景索引
        currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        int nextSceneIndex = currentSceneIndex + 1;

        // 如果下一个场景索引超出范围，则回到第一个场景（索引0）
        if (nextSceneIndex >= SceneManager.sceneCountInBuildSettings)
        {
            nextSceneIndex = 0;
        }

        LoadAni.SwitchToScene(nextSceneIndex);
    }
}
