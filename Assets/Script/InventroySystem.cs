using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InventorySystem : MonoBehaviour
{
    public static InventorySystem Instance { get; set; }

    [Header("UI General")]
    public GameObject inventoryScreenUI;
    public List<GameObject> slotList = new List<GameObject>();
    public List<string> itemList = new List<string>();

    public bool isOpen;

    // ADDED: Simple event delegate hook to instantly alert HUD overlays when counts shift
    public event Action OnInventoryChanged;

    private void Awake()
    {
        if (Instance != null && Instance != this) Destroy(gameObject);
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    void Start()
    {
        isOpen = false;
        PopulateSlotList();

        if (inventoryScreenUI != null) inventoryScreenUI.SetActive(false);
        Cursor.visible = false;
    }

    private void PopulateSlotList()
    {
        slotList.Clear();
        foreach (Transform child in inventoryScreenUI.transform)
        {
            if (child.CompareTag("Slot")) slotList.Add(child.gameObject);
        }
    }

    void Update()
    {
        bool bookOpen = (BookManager.Instance != null && BookManager.Instance.isBookOpen);

        if (Input.GetKeyDown(KeyCode.I) && !bookOpen)
        {
            if (!isOpen) OpenInventory();
            else CloseInventory();
        }
    }

    public void AddToInventory(string itemName, int quantity = 1)
    {
        // 1. Try to find an existing stack that is NOT FULL
        foreach (GameObject slot in slotList)
        {
            if (slot.transform.childCount > 0)
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
            if (slot.transform.childCount == 0) return slot;
        }
        return null;
    }

    public bool CheckIfFull()
    {
        foreach (GameObject slot in slotList)
        {
            if (slot.transform.childCount == 0) return false;
        }
        return true;
    }

    public int GetTotalItemCount(string targetItemName)
    {
        int total = 0;
        foreach (GameObject slot in slotList)
        {
            if (slot.transform.childCount > 0)
            {
                InventoryItem item = slot.transform.GetChild(0).GetComponent<InventoryItem>();
                // Added case-insensitive comparison safety check so "Arrow" vs "arrow" won't break matches
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

        for (var i = slotList.Count - 1; i >= 0; i--)
        {
            if (slotList[i].transform.childCount > 0)
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
        foreach (GameObject slot in slotList)
        {
            if (slot.transform.childCount > 0)
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

        // ADDED: Signal all listening custom UI scripts that they need to repaint their values!
        OnInventoryChanged?.Invoke();
    }

    private void OpenInventory()
    {
        inventoryScreenUI.SetActive(true);
        isOpen = true;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        if (SelectionManager.Instance != null) SelectionManager.Instance.DisableSelection();
    }

    private void CloseInventory()
    {
        inventoryScreenUI.SetActive(false);
        isOpen = false;

        if (CraftingSystem.Instance != null && !CraftingSystem.Instance.isOpen)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            if (SelectionManager.Instance != null) SelectionManager.Instance.EnableSelection();
        }
    }
}