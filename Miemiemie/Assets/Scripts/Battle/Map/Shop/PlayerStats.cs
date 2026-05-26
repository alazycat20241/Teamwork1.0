using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    public static PlayerStats Instance { get; private set; }

    public float attackBonus = 0f;
    public float rangeBonus = 0f;
    public float speedBonus = 0f;
    public float panicChance = 0f;
    public float panicDuration = 0f;
    public float stoneChance = 0f;
    public float stoneDuration = 0f;

    void Awake()
    {
        Instance = this;
    }
}