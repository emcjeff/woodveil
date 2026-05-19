using UnityEngine;
using TMPro;

public class ArrowUIDisplay : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Drag your Text component here. If left empty, it will try to find it on this GameObject.")]
    [SerializeField] private TextMeshProUGUI arrowCountText;

    [Tooltip("Drag the parent panel/root container of the Arrow HUD here. If left empty, it will use this GameObject.")]
    [SerializeField] private GameObject uiContainer;

    [Header("Settings")]
    [Tooltip("The exact item name used inside your inventory slots data for the arrows")]
    [SerializeField] private string arrowItemName = "ArrowUI";

    private bool isSubscribed = false;

    private void Start()
    {
        if (arrowCountText == null)
        {
            arrowCountText = GetComponent<TextMeshProUGUI>();
        }

        if (uiContainer == null)
        {
            uiContainer = this.gameObject;
        }

        TrySubscribe();
    }

    private void Update()
    {
        // Keep attempting to subscribe if the InventorySystem wasn't ready on frame 1
        if (!isSubscribed && InventorySystem.Instance != null)
        {
            TrySubscribe();
        }

        EvaluateVisibilityConditions();
    }

    private void TrySubscribe()
    {
        if (InventorySystem.Instance != null)
        {
            InventorySystem.Instance.OnInventoryChanged += RefreshArrowCounter;
            isSubscribed = true;
            RefreshArrowCounter();
        }
    }

    private void OnDestroy()
    {
        if (isSubscribed && InventorySystem.Instance != null)
        {
            InventorySystem.Instance.OnInventoryChanged -= RefreshArrowCounter;
        }
    }

    private void EvaluateVisibilityConditions()
    {
        // If the main system doesn't exist yet, keep it hidden
        if (InventorySystem.Instance == null)
        {
            SetUIElementActive(false);
            return;
        }

        // FIXED: Removed the BookManager condition entirely!

        // Check Bow condition (Checks BOTH "Bow" and "BowUI" so it never breaks!)
        int bowCount = InventorySystem.Instance.GetTotalItemCount("Bow");
        int bowUiCount = InventorySystem.Instance.GetTotalItemCount("BowUI");
        bool hasBowWeapon = (bowCount > 0 || bowUiCount > 0);

        // The UI should show ONLY if the player has the bow weapon
        bool shouldBeVisible = hasBowWeapon;

        SetUIElementActive(shouldBeVisible);
    }

    private void SetUIElementActive(bool state)
    {
        // If we are turning off the script's OWN GameObject, its Update() loop stops running!
        // To fix this, we turn off the Text component renderer instead if it's pointing to itself.
        if (uiContainer == this.gameObject)
        {
            if (arrowCountText != null && arrowCountText.enabled != state)
            {
                arrowCountText.enabled = state;
            }
        }
        else
        {
            // If it's a separate container panel, we can safely flip its active state
            if (uiContainer != null && uiContainer.activeSelf != state)
            {
                uiContainer.SetActive(state);
            }
        }
    }

    public void RefreshArrowCounter()
    {
        if (InventorySystem.Instance == null || arrowCountText == null) return;

        int totalArrows = InventorySystem.Instance.GetTotalItemCount(arrowItemName);
        arrowCountText.text = totalArrows.ToString();
    }
}