using UnityEngine;

public class TeamMenber: MonoBehaviour
{
    private Animator TeamAnimator;
  


    private void Awake()
    {
        TeamAnimator = GetComponent<Animator>();
    }

    private void OnMouseDown()
    {
        
            TeamAnimator.SetTrigger("R");
         
        

    }
}