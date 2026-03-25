using UnityEngine.UI;
using UnityEngine;

public class UI_Continue : MonoBehaviour
{
    public GameObject targetUI;

    void Start()
    {
        GetComponent<Button>().onClick.AddListener(ResumeGame);
    }

    void ResumeGame()
    {
        targetUI.SetActive(false);
        Time.timeScale = 1; // ¼ÌÐø
    }
}
