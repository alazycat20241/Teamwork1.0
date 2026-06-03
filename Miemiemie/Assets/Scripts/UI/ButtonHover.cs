using UnityEngine;

public class ButtonHover : MonoBehaviour
{
    public GameObject hoverIcon;
    //设置选择界面的选择图标
    public void ShowIcon()
    {
        hoverIcon.SetActive(true);
    }

    public void HideIcon()
    {
        hoverIcon.SetActive(false);
    }
}