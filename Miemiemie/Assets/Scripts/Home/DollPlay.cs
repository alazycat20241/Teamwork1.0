using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DollPlay : MonoBehaviour
{
    public static DollPlay Instance;
    public bool ischange = false;
    public GameObject DollPrefab;
    public Transform DPoint;

    [Header("生成设置")]
    [SerializeField] private float spawnRadius = 1f; // 散布半径
    // Start is called before the first frame update
    void Awake()
    {
        Instance = this;
    }


    // Update is called once per frame
    void Update()
    {
        if (ischange)
        {
            Doll(PlayerInventory.Instance.DollCount-1);
            ischange = false;
        } 
    }

    void Doll(int DollCounts)
    {
        if (DollPrefab == null) return;

        Vector3 basePos = DPoint.position + Vector3.up * 0.5f;

        for (int i = 0; i < DollCounts; i++)
        {
            // 在水平面（XZ）随机偏移，Y保持基准高度
            Vector2 randomOffset = Random.insideUnitCircle * spawnRadius;
            Vector3 spawnPos = basePos + new Vector3(randomOffset.x, randomOffset.y, 0f);

            Instantiate(DollPrefab, spawnPos, Quaternion.identity);
        }
    }
}
