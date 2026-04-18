using System;
using System.Collections;
using System.Collections.Generic;
<<<<<<< HEAD
<<<<<<< HEAD:INSIDETHEBOOK/Assets/Script/InventroySystem.cs
using System.Diagnostics.Tracing;
using UnityEditor.Animations;
using UnityEditor.Build.Content;

=======
>>>>>>> b857efda77977d2f6f8190f3b7f33a6c89e5e36d:Assets/Script/InventroySystem.cs
=======
>>>>>>> b857efda77977d2f6f8190f3b7f33a6c89e5e36d
using UnityEngine;

public class InventorySystem : MonoBehaviour
{
    public static InventorySystem Instance { get; set; }

    public GameObject inventoryScreenUI;
    public List<GameObject> slotList = new List<GameObject>();
    public List<string> itemList = new List<string>();

    public bool isOpen;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    void Start()
    {
        isOpen = false;
        PopulateSlotList();
        Cursor.visible = false;
<<<<<<< HEAD
<<<<<<< HEAD:INSIDETHEBOOK/Assets/Script/InventroySystem.cs

        
=======
>>>>>>> b857efda77977d2f6f8190f3b7f33a6c89e5e36d:Assets/Script/InventroySystem.cs
=======
>>>>>>> b857efda77977d2f6f8190f3b7f33a6c89e5e36d
    }

    private void PopulateSlotList()
    {
        slotList.Clear();
        foreach (Transform child in inventoryScreenUI.transform)
        {
            if (child.CompareTag("Slot"))
            {
                slotList.Add(child.gameObject);
            }
        }
    }

<<<<<<< HEAD
<<<<<<< HEAD:INSIDETHEBOOK/Assets/Script/InventroySystem.cs


void Update()
{
    if (Input.GetKeyDown(KeyCode.I) && !isOpen)
    {
        Debug.Log("i is pressed");
        inventoryScreenUI.SetActive(true);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        SelectionManager.Instance.DisableSelection();
        SelectionManager.Instance.GetComponent<SelectionManager>().enabled = false;

        isOpen = true;
=======
=======
>>>>>>> b857efda77977d2f6f8190f3b7f33a6c89e5e36d
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.I) && !isOpen)
        {
            inventoryScreenUI.SetActive(true);
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            SelectionManager.Instance.DisableSelection();
            SelectionManager.Instance.enabled = false; 

            isOpen = true;
        }
        else if (Input.GetKeyDown(KeyCode.I) && isOpen)
        {
            inventoryScreenUI.SetActive(false);
            isOpen = false;

            if (!CraftingSystem.Instance.isOpen)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;

                SelectionManager.Instance.EnableSelection();
                SelectionManager.Instance.enabled = true; // Crucial fix
            }
        }
>>>>>>> b857efda77977d2f6f8190f3b7f33a6c89e5e36d:Assets/Script/InventroySystem.cs
    }
    else if (Input.GetKeyDown(KeyCode.I) && isOpen)
    {
        inventoryScreenUI.SetActive(false);

<<<<<<< HEAD
        if (!CraftingSystem.Instance.isOpen)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            SelectionManager.Instance.EnableSelection();
            SelectionManager.Instance.GetComponent<SelectionManager>().enabled = false;
        }

        isOpen = false;
    }
}

=======
>>>>>>> b857efda77977d2f6f8190f3b7f33a6c89e5e36d
    public void AddToInventory(string itemName)
    {
        GameObject whatSlotToEquip = FindNextEmptySlot();
        
        if (whatSlotToEquip != null)
        {
            GameObject itemToAdd = Instantiate(Resources.Load<GameObject>(itemName), whatSlotToEquip.transform.position, whatSlotToEquip.transform.rotation);
            itemToAdd.transform.SetParent(whatSlotToEquip.transform);
            itemList.Add(itemName);
        }
    }

    private GameObject FindNextEmptySlot()
    {
        foreach (GameObject slot in slotList)
        {
            if (slot.transform.childCount == 0)
            {
                return slot;
            }
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

    public void RemoveItem(string nameToRemove, int amountToRemove)
    {
        int counter = amountToRemove;

        for (var i = slotList.Count - 1; i >= 0; i--)
        {
            if (slotList[i].transform.childCount > 0)
            {
                if (slotList[i].transform.GetChild(0).name == nameToRemove + "(Clone)" && counter > 0)
                {
                    Destroy(slotList[i].transform.GetChild(0).gameObject);
                    counter -= 1;
                }
            }
        }
    }

    public void ReCalculateList()
    {
        itemList.Clear();
        foreach (GameObject slot in slotList)
        {
            if (slot.transform.childCount > 0)
            {
                string name = slot.transform.GetChild(0).name;
                string result = name.Replace("(Clone)", "");
                itemList.Add(result);
            }
        }
    }
}
