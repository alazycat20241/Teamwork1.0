using UnityEngine;

public class InventoryUI : MonoBehaviour
{
    public static InventoryUI Instance { get; private set; }
    [SerializeField] private DropHandler[] slots;

    void Awake() { Instance = this; }

}