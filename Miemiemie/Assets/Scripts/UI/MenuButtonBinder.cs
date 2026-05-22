using UnityEngine;
using UnityEngine.UI;

public class MenuButtonBinder : MonoBehaviour
{
    void Start()
    {
        GetComponent<Button>().onClick.AddListener(() =>
        {
            if (MenuUIManager.Instance != null)
                MenuUIManager.Instance.OpenMenu();
        });
    }
}