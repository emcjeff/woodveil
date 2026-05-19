using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class InventorySystem : MonoBehaviour
{
    public static InventorySystem Instance { get; private set; }

    [Header("UI General")]
    public GameObject inventoryScreenUI;
    public List<GameObject> slotList = new List<GameObject>();
    public List<string> itemList = new List<string>();

    [HideInInspector] public bool isOpen;

    // Simple event delegate hook to alert HUD overlays when counts shift
    public event Action OnInventoryChanged;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Track when scenes change to heal UI associations automatically
        SceneManager.sceneLoaded += OnSceneChangedLayout;

        ResetInventoryDataState();
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneChangedLayout;
    }

    private void Start()
    {
        InitializeSceneUIAssociation(SceneManager.GetActiveScene().name);
    }

    private void OnSceneChangedLayout(Scene scene, LoadSceneMode mode)
    {
        InitializeSceneUIAssociation(scene.name);
    }

    /// <summary>
    /// Looks for the new scene UI assets to link manager trackers to active visual elements.
    /// </summary>
    private void InitializeSceneUIAssociation(string sceneName)
    {
        isOpen = false;

        // If we load back to menus or screens where tracking isn't needed, completely clear old references
        if (sceneName == "MainMenu" || sceneName == "GameOver" || sceneName == "Win")
        {
            ResetInventoryDataState();
            return;
        }

        // Search for the newly generated scene canvas
        GameObject canvasObj = GameObject.Find("Canvas");
        if (canvasObj != null)
        {
            // Looks for the Inventory Panel layout setup
            Transform targetPanel = canvasObj.transform.Find("InventoryScreen");
            if (targetPanel != null)
            {
                inventoryScreenUI = targetPanel.gameObject;
                inventoryScreenUI.SetActive(false);
                PopulateSlotList();
                Debug.Log("[Inventory Manager] Successfully linked to the new scene's UI layout.");
            }
            else
            {
                // Fallback scan: look through structural root setups
                inventoryScreenUI = GameObject.Find("InventoryScreen");
                if (inventoryScreenUI != null)
                {
                    inventoryScreenUI.SetActive(false);
                    PopulateSlotList();
                }
            }
        }
    }

    /// <summary>
    /// Safely cleans up data, drops lingering event connections, and handles scene resets.
    /// </summary>
    public void ResetInventoryDataState()
    {
        isOpen = false;
        if (itemList != null) itemList.Clear();
        if (slotList != null) slotList.Clear();

        inventoryScreenUI = null;
        OnInventoryChanged = null;

        Debug.Log("[Inventory Manager] Data structures completely cleared for scene session reset.");
    }

    private void PopulateSlotList()
    {
        slotList.Clear();
        if (inventoryScreenUI == null) return;

        foreach (Transform child in inventoryScreenUI.transform)
        {
            if (child.CompareTag("Slot")) slotList.Add(child.gameObject);
        }
    }

    void Update()
    {
        // Block processing if the current layout environment has no operational UI attached
        if (inventoryScreenUI == null) return;

        bool bookOpen = (BookManager.Instance != null && BookManager.Instance.isBookOpen);

        if (Input.GetKeyDown(KeyCode.I) && !bookOpen)
        {
            if (!isOpen) OpenInventory();
            else CloseInventory();
        }
    }

    public void AddToInventory(string itemName, int quantity = 1)
    {
        if (slotList == null || slotList.Count == 0) return;

        // 1. Try to find an existing stack that is NOT FULL
        foreach (GameObject slot in slotList)
        {
            if (slot != null && slot.transform.childCount > 0)
            {
                InventoryItem itemInSlot = slot.transform.GetChild(0).GetComponent<InventoryItem>();

                if (itemInSlot != null &&
                    itemInSlot.itemName == itemName &&
                    itemInSlot.isStackable &&
                    itemInSlot.amount < itemInSlot.maxAmount)
                {
                    int spaceLeft = itemInSlot.maxAmount - itemInSlot.amount;
                    int amountToAdd = Math.Min(quantity, spaceLeft);

                    itemInSlot.amount += amountToAdd;
                    itemInSlot.UpdateSlotText();

                    quantity -= amountToAdd;

                    if (quantity <= 0)
                    {
                        ReCalculateList();
                        return;
                    }
                }
            }
        }

        // 2. Create a NEW stack in an empty slot
        while (quantity > 0)
        {
            GameObject whatSlotToEquip = FindNextEmptySlot();

            if (whatSlotToEquip != null)
            {
                GameObject prefab = Resources.Load<GameObject>(itemName);
                if (prefab == null)
                {
                    Debug.LogError("Prefab not found in Resources: " + itemName);
                    return;
                }

                GameObject itemToAdd = Instantiate(prefab);
                itemToAdd.transform.SetParent(whatSlotToEquip.transform, false);

                InventoryItem newItem = itemToAdd.GetComponent<InventoryItem>();
                if (newItem != null)
                {
                    newItem.itemName = itemName;
                    int amountForThisSlot = Math.Min(quantity, newItem.maxAmount);
                    newItem.amount = amountForThisSlot;
                    newItem.UpdateSlotText();

                    quantity -= amountForThisSlot;
                }
            }
            else
            {
                Debug.Log("Inventory Full!");
                break;
            }
        }

        ReCalculateList();
    }

    private GameObject FindNextEmptySlot()
    {
        foreach (GameObject slot in slotList)
        {
            if (slot != null && slot.transform.childCount == 0) return slot;
        }
        return null;
    }

    public bool CheckIfFull()
    {
        foreach (GameObject slot in slotList)
        {
            if (slot != null && slot.transform.childCount == 0) return false;
        }
        return true;
    }

    public int GetTotalItemCount(string targetItemName)
    {
        int total = 0;
        if (slotList == null || slotList.Count == 0) return 0;

        foreach (GameObject slot in slotList)
        {
            if (slot != null && slot.transform.childCount > 0)
            {
                // Safety guard check against empty tracking slots inside loop
                if (slot.transform.GetChild(0) == null) continue;

                InventoryItem item = slot.transform.GetChild(0).GetComponent<InventoryItem>();
                if (item != null && string.Equals(item.itemName, targetItemName, StringComparison.OrdinalIgnoreCase))
                {
                    total += item.amount;
                }
            }
        }
        return total;
    }

    public void RemoveItem(string nameToRemove, int amountToRemove)
    {
        int counter = amountToRemove;
        if (slotList == null) return;

        for (var i = slotList.Count - 1; i >= 0; i--)
        {
            if (slotList[i] != null && slotList[i].transform.childCount > 0)
            {
                InventoryItem item = slotList[i].transform.GetChild(0).GetComponent<InventoryItem>();
                if (item != null && item.itemName == nameToRemove && counter > 0)
                {
                    if (item.amount > counter)
                    {
                        item.amount -= counter;
                        counter = 0;
                        item.UpdateSlotText();
                    }
                    else
                    {
                        counter -= item.amount;
                        Destroy(slotList[i].transform.GetChild(0).gameObject);
                    }
                }
            }
        }
        ReCalculateList();
    }

    public void ReCalculateList()
    {
        itemList.Clear();
        if (slotList == null) return;

        foreach (GameObject slot in slotList)
        {
            if (slot != null && slot.transform.childCount > 0)
            {
                InventoryItem item = slot.transform.GetChild(0).GetComponent<InventoryItem>();
                if (item != null)
                {
                    for (int i = 0; i < item.amount; i++)
                    {
                        itemList.Add(item.itemName);
                    }
                }
            }
        }

        OnInventoryChanged?.Invoke();
    }

    private void OpenInventory()
    {
        if (inventoryScreenUI != null) inventoryScreenUI.SetActive(true);
        isOpen = true;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        if (SelectionManager.Instance != null) SelectionManager.Instance.DisableSelection();
    }

    private void CloseInventory()
    {
        if (inventoryScreenUI != null) inventoryScreenUI.SetActive(false);
        isOpen = false;

        if (CraftingSystem.Instance != null && !CraftingSystem.Instance.isOpen)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            if (SelectionManager.Instance != null) SelectionManager.Instance.EnableSelection();
        }
    }
}