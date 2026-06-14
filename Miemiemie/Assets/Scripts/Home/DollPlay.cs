using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DollPlay : MonoBehaviour
{
    public static DollPlay Instance;
    public bool ischange = false;
    public GameObject DollPrefab;
    public Transform DPoint;
    public int DCount = 0;
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
            Doll(DCount);
            ischange = false;
            DCount = 0;
        }
    }

    void Doll(int DollCounts)
    {
        if (DollPrefab == null) return;

        Vector3 basePos = DPoint.position + Vector3.up * 0.5f;

        for (int i = 0; i < DollCounts; i++)
        {
            Instantiate(DollPrefab, basePos, Quaternion.identity);
        }
    }
}

