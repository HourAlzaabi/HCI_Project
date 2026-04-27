using TMPro;
using UnityEngine;

public class InventoryUI : MonoBehaviour
{
    public TMP_Text inventoryText;

    private void Start()
    {
        if (InventorySystem.Instance != null)
        {
            InventorySystem.Instance.OnInventoryChanged += RefreshUI;
        }

        RefreshUI();
    }

    private void OnDestroy()
    {
        if (InventorySystem.Instance != null)
        {
            InventorySystem.Instance.OnInventoryChanged -= RefreshUI;
        }
    }

    public void RefreshUI()
    {
        if (inventoryText == null || InventorySystem.Instance == null) return;

        string text = "Inventory:\n";

        foreach (string item in InventorySystem.Instance.items)
        {
            text += "- " + item + "\n";
        }

        inventoryText.text = text;
    }
}