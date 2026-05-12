using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Try : MonoBehaviour
{
    [SerializeField] private float chargeTime = 2f;    // 蓄力时间

    private SporePool sporePool;                       // 孢子池引用
    private float chargeTimer = 0f;                    // 蓄力计时
    private bool isCharging = false;                   // 是否正在蓄力

    void Start()
    {
        sporePool = FindObjectOfType<SporePool>();
    }

    void Update()
    {
        // 按下空格开始蓄力
        if (Input.GetKeyDown(KeyCode.Space))
        {
            isCharging = true;
            chargeTimer = 0f;
        }

        // 按住空格蓄力
        if (Input.GetKey(KeyCode.Space) && isCharging)
        {
            chargeTimer += Time.deltaTime;

            // 蓄力满了，爆发！
            if (chargeTimer >= chargeTime)
            {
                sporePool.BurstSpores(transform.position);
                isCharging = false;
                chargeTimer = 0f;
            }
        }

        // 提前松手取消蓄力
        if (Input.GetKeyUp(KeyCode.Space))
        {
            isCharging = false;
            chargeTimer = 0f;
        }
    }
}
