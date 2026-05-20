using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class ArrowUIDisplay : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Drag your Text component here. If left empty, it will try to find it on this GameObject.")]
    [SerializeField] private TextMeshProUGUI arrowCountText;

    [Tooltip("Drag the parent panel/root container of the Arrow HUD here. If left empty, it will use this GameObject.")]
    [SerializeField] private GameObject uiContainer;

    [Header("Settings")]
    [Tooltip("The default fallback item name inside your inventory slots data")]
    [SerializeField] private string arrowItemName = "ArrowUI";

    private bool isSubscribed = false;

    private void Awake()
    {
        // Hook into the scene manager event system to heal data tracks on winning, retrying, or scene loading
        SceneManager.sceneLoaded += OnSceneLoadedLayoutHeal;
    }

    private void OnDestroy()
    {
        // Clean up event footprints out of engine memory
        SceneManager.sceneLoaded -= OnSceneLoadedLayoutHeal;
        UnsubscribeFromInventory();
    }

    private void Start()
    {
        FindLocalReferences();
        TrySubscribe();
    }

    private void OnSceneLoadedLayoutHeal(Scene scene, LoadSceneMode mode)
    {
        string sceneName = scene.name;

        // If we hit menu, game over, or win screens, cleanly clear tracking states
        if (sceneName == "MainMenu" || sceneName == "GameOver" || sceneName == "Win")
        {
            UnsubscribeFromInventory();
            SetUIElementActive(false);
            return;
        }

        // Re-locate components in the new scene layout context
        FindLocalReferences();

        // Force a clean resubscription to the brand new active Inventory instance frame
        UnsubscribeFromInventory();
        TrySubscribe();
    }

    private void FindLocalReferences()
    {
        if (arrowCountText == null)
        {
            arrowCountText = GetComponent<TextMeshProUGUI>();
        }

        // Fallback search if the reference was destroyed during a scene transition swap
        if (uiContainer == null)
        {
            // First try to look for the explicit ArrowAmmo container object in the scene hierarchy
            uiContainer = GameObject.Find("ArrowAmmo");

            if (uiContainer == null)
            {
                uiContainer = this.gameObject;
            }
        }
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
        if (InventorySystem.Instance != null && !isSubscribed)
        {
            InventorySystem.Instance.OnInventoryChanged += RefreshArrowCounter;
            isSubscribed = true;
            RefreshArrowCounter();
        }
    }

    private void UnsubscribeFromInventory()
    {
        if (isSubscribed && InventorySystem.Instance != null)
        {
            InventorySystem.Instance.OnInventoryChanged -= RefreshArrowCounter;
        }
        isSubscribed = false;
    }

    private void EvaluateVisibilityConditions()
    {
        // If the main system doesn't exist yet, keep it hidden
        if (InventorySystem.Instance == null)
        {
            SetUIElementActive(false);
            return;
        }

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
        if (uiContainer == this.gameObject)
        {
            if (arrowCountText != null && arrowCountText.enabled != state)
            {
                arrowCountText.enabled = state;
            }
        }
        else
        {
            if (uiContainer != null && uiContainer.activeSelf != state)
            {
                uiContainer.SetActive(state);
            }
        }
    }

    public void RefreshArrowCounter()
    {
        if (InventorySystem.Instance == null || arrowCountText == null) return;

        // BULLETPROOF CHECK: Scan for BOTH naming conventions ("Arrow" vs "ArrowUI")
        int defaultArrows = InventorySystem.Instance.GetTotalItemCount(arrowItemName);
        int fallbackArrows = InventorySystem.Instance.GetTotalItemCount("Arrow");

        // Sum them up or choose the maximum found value
        int totalArrows = Mathf.Max(defaultArrows, fallbackArrows);

        arrowCountText.text = totalArrows.ToString();
    }
}