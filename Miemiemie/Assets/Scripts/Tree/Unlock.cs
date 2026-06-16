using UnityEngine.UI;
using UnityEngine;

public class Unlock : MonoBehaviour
{
    [SerializeField] private Image img1;
    [SerializeField] private Image img2;
    [SerializeField] private Image img3;
    [SerializeField] private Button butt;
    [SerializeField] private Button buttt;
    [SerializeField] private Button butttt;


    private Button buttonn;
    void Awake()
    {
        buttonn=GetComponent<Button>();
        if (buttonn != null)
            buttonn.onClick.AddListener(onnclick);
    }
    void onnclick()
    {
        img1.gameObject.SetActive(false);
        img2.gameObject.SetActive(false);
        img3.gameObject.SetActive(false);
        butt.gameObject.SetActive(true);
        buttt.gameObject.SetActive(true);
        butttt.gameObject.SetActive(true);
    }
}
