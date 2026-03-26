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
        if (other.gameObject.CompareTag("Player"))
        {
            LoadAni.SwitchToScene(currentSceneIndex + 1);
        }
    }
}
