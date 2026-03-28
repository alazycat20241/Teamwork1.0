using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Collider : MonoBehaviour
{

    [Header("检测设置")]
    public LayerMask wallLayer;           // 墙壁层
    public bool debugMode = true;

    [Header("事件")]
    public UnityEvent OnWallEnter;
    public UnityEvent OnWallExit;

    private bool isTouchingWall;
    private int wallContactCount = 0;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (IsWall(other))
        {
            wallContactCount++;
            if (wallContactCount == 1)
            {
                isTouchingWall = true;
                OnWallEnter?.Invoke();

                if (debugMode)
                    Debug.Log("接触墙壁");
            }
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (IsWall(other))
        {
            wallContactCount--;
            if (wallContactCount == 0)
            {
                isTouchingWall = false;
                OnWallExit?.Invoke();

                if (debugMode)
                    Debug.Log("离开墙壁");
            }
        }
    }

    bool IsWall(Collider2D collider)
    {
        return ((1 << collider.gameObject.layer) & wallLayer) != 0;
    }

    public bool IsTouchingWall()
    {
        return isTouchingWall;
    }
}
