using System.Collections.Generic;
using UnityEngine;

public class DollPlay : MonoBehaviour
{
    public static DollPlay Instance;

    public int DCount = 1;
    public GameObject dollPrefab;
    public Transform dollParent;
    public Transform dollSpawnPoint;

    private List<GameObject> activeDolls = new List<GameObject>();
    public bool ischange = false;

    void Awake()
    {
        Instance = this;   // 每次加载场景重新赋值
    }

    void Start()
    {
        SpawnDolls(DCount);
    }

    public void SpawnDolls(int count)
    {
        Debug.Log($"SpawnDolls 被调用: count={count}, prefab={dollPrefab}, spawnPoint={dollSpawnPoint}");

        ClearAllDolls();
        DCount = count;

        if (dollPrefab == null)
        {
            Debug.LogError("dollPrefab 为空！请在 Inspector 拖拽预制体");
            return;
        }

        if (dollSpawnPoint == null) dollSpawnPoint = transform;

        for (int i = 0; i < count; i++)
        {
            Vector3 offset = new Vector3(i * 0.5f, 0, 0);
            GameObject doll = Instantiate(dollPrefab, dollSpawnPoint.position + offset, Quaternion.identity);
            Debug.Log($"生成玩偶 {i}: {doll.name}");

            if (dollParent != null)
                doll.transform.SetParent(dollParent);

            activeDolls.Add(doll);
        }

        Debug.Log($"共生成 {activeDolls.Count} 个玩偶");
    }

    public void AddDoll()
    {
        DCount++;

        if (dollSpawnPoint == null) dollSpawnPoint = transform;

        Vector3 offset = new Vector3((DCount - 1) * 0.5f, 0, 0);
        GameObject doll = Instantiate(dollPrefab, dollSpawnPoint.position + offset, Quaternion.identity);

        if (dollParent != null)
            doll.transform.SetParent(dollParent);

        activeDolls.Add(doll);
    }

    void ClearAllDolls()
    {
        foreach (var doll in activeDolls)
        {
            if (doll != null) Destroy(doll);
        }
        activeDolls.Clear();
    }
}