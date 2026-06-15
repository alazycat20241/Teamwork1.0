using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SleepButton : MonoBehaviour
{
    private Button button;
    void Start()
    {
        button = gameObject.GetComponent<Button>();
        button.onClick.AddListener(NextDay);
    }

    void NextDay()
    {
        ActionPointManager.Instance.AdvanceDay();
    }
}
