using UnityEngine;

public class PickupItem : MonoBehaviour
{
    public string itemName = "Wire Cutters";

    public void PickUp()
    {
        InventorySystem.Instance.AddItem(itemName);
        Destroy(gameObject);
    }
}
