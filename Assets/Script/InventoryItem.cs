using UnityEngine;
using TMPro;

public class InventoryItem : MonoBehaviour
{
    public string itemName; // Set this to "Stone" or "Stick" or "ArrowUI" in the Inspector!
    public int amount = 1;
    public int maxAmount = 99;
    public bool isStackable = true;
    public TextMeshProUGUI amountText;

    void Start()
    {
        // If you forgot to set the name in the inspector, this grabs the prefab name
        if (string.IsNullOrEmpty(itemName))
        {
            itemName = gameObject.name.Replace("(Clone)", "");
        }
        UpdateSlotText();
    }

    public void UpdateSlotText()
    {
        if (amountText != null)
        {
            amountText.text = amount > 1 ? amount.ToString() : "";
        }
    }
}