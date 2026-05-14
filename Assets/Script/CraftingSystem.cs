using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CraftingSystem : MonoBehaviour
{
    public GameObject toolsScreenUI;
    public GameObject craftingScreenUI;
    public List<string> inventoryItemList = new List<string>();

    // Buttons
    private Button toolsBTN;
    private Button backBTN;

    // --- Axe UI ---
    private Button craftAxeBTN;
    private TextMeshProUGUI AxeReq1, AxeReq2;
    public Blueprint AxeBLP = new Blueprint("Axe", 2, "Stone", 3, "Stick", 3);

    // --- Arrow UI ---
    private Button craftArrowBTN;
    private TextMeshProUGUI ArrowReq1, ArrowReq2;
    // Changed "Arrow" to "ArrowUI" to match your Resources folder
    public Blueprint ArrowBLP = new Blueprint("ArrowUI", 2, "Stone", 1, "Stick", 1);

    public bool isOpen;
    public static CraftingSystem Instance { get; set; }

    private void Awake()
    {
        if (Instance != null && Instance != this) Destroy(gameObject);
        else Instance = this;
    }

    void Start()
    {
        isOpen = false;

        // 1. Setup Main Category Button
        toolsBTN = craftingScreenUI.transform.Find("ToolsButton").GetComponent<Button>();
        toolsBTN.onClick.AddListener(delegate { OpenToolsCategory(); });

        // 2. Setup Back Button
        backBTN = toolsScreenUI.transform.Find("BackButton").GetComponent<Button>();
        backBTN.onClick.AddListener(delegate { CloseToolsCategory(); });

        // 3. Setup Axe UI References (Looking for GameObject named "Axe")
        Transform axeTransform = toolsScreenUI.transform.Find("Axe");
        AxeReq1 = axeTransform.Find("req1").GetComponent<TextMeshProUGUI>();
        AxeReq2 = axeTransform.Find("req2").GetComponent<TextMeshProUGUI>();
        craftAxeBTN = axeTransform.Find("CraftButton").GetComponent<Button>();
        craftAxeBTN.onClick.AddListener(delegate { CraftAnyItem(AxeBLP); });

        // 4. Setup Arrow UI References (Looking for GameObject named "Arrow")
        Transform arrowTransform = toolsScreenUI.transform.Find("Arrow");
        ArrowReq1 = arrowTransform.Find("req1").GetComponent<TextMeshProUGUI>();
        ArrowReq2 = arrowTransform.Find("req2").GetComponent<TextMeshProUGUI>();
        craftArrowBTN = arrowTransform.Find("CraftButton").GetComponent<Button>();
        craftArrowBTN.onClick.AddListener(delegate { CraftAnyItem(ArrowBLP); });
    }

    void OpenToolsCategory()
    {
        craftingScreenUI.SetActive(false);
        toolsScreenUI.SetActive(true);
    }

    void CloseToolsCategory()
    {
        toolsScreenUI.SetActive(false);
        craftingScreenUI.SetActive(true);
    }

    void CraftAnyItem(Blueprint blueprintToCraft)
    {
        // 1. Determine how many items to give.
        // If it's ArrowUI, give 10. Otherwise, give 1.
        int amountToGive = (blueprintToCraft.itemName == "ArrowUI") ? 10 : 1;

        // 2. Add to inventory using the new amountToGive variable
        InventorySystem.Instance.AddToInventory(blueprintToCraft.itemName, amountToGive);

        // 3. Remove the requirements from the inventory
        if (blueprintToCraft.numOfRequirements == 1)
        {
            InventorySystem.Instance.RemoveItem(blueprintToCraft.Req1, blueprintToCraft.Req1amount);
        }
        else if (blueprintToCraft.numOfRequirements == 2)
        {
            InventorySystem.Instance.RemoveItem(blueprintToCraft.Req1, blueprintToCraft.Req1amount);
            InventorySystem.Instance.RemoveItem(blueprintToCraft.Req2, blueprintToCraft.Req2amount);
        }

        // 4. Refresh everything
        InventorySystem.Instance.ReCalculateList();
        RefreshNeededItems();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.C) && !BookManager.Instance.isBookOpen)
        {
            if (!isOpen) OpenCrafting();
            else CloseCrafting();
        }

        if (isOpen) RefreshNeededItems();
    }

    private void OpenCrafting()
    {
        craftingScreenUI.SetActive(true);
        isOpen = true;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        if (SelectionManager.Instance != null) SelectionManager.Instance.DisableSelection();
    }

    private void CloseCrafting()
    {
        craftingScreenUI.SetActive(false);
        toolsScreenUI.SetActive(false);
        isOpen = false;

        if (!InventorySystem.Instance.isOpen)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            if (SelectionManager.Instance != null) SelectionManager.Instance.EnableSelection();
        }
    }

    private void RefreshNeededItems()
    {
        InventorySystem.Instance.ReCalculateList();

        int stone_count = 0;
        int stick_count = 0;
        bool hasAxe = false; // New variable to track the Axe

        foreach (string itemName in InventorySystem.Instance.itemList)
        {
            if (itemName == "Stone") stone_count++;
            else if (itemName == "Stick") stick_count++;

            // Check if the list contains the Axe
            if (itemName == "Axe") hasAxe = true;
        }

        // --- Axe Refresh ---
        AxeReq1.text = "3 Stone [" + stone_count + "]";
        AxeReq2.text = "3 Stick [" + stick_count + "]";

        // Logic: Only show the button if you DON'T have an axe AND you have the materials
        if (hasAxe)
        {
            craftAxeBTN.gameObject.SetActive(false);
            // Optional: Change the text to show it's already owned
            AxeReq1.text = "ALREADY";
            AxeReq2.text = "OWNED";
        }
        else
        {
            // Show button only if materials are met
            craftAxeBTN.gameObject.SetActive(stone_count >= 3 && stick_count >= 3);
        }

        // --- Arrow Refresh ---
        ArrowReq1.text = "1 Stone [" + stone_count + "]";
        ArrowReq2.text = "1 Stick [" + stick_count + "]";
        craftArrowBTN.gameObject.SetActive(stone_count >= 1 && stick_count >= 1);
    }
}