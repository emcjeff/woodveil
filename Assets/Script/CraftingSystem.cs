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
        int amountToGive = (blueprintToCraft.itemName == "ArrowUI") ? 10 : 1;

        // 2. Add to inventory using the amountToGive variable
        InventorySystem.Instance.AddToInventory(blueprintToCraft.itemName, amountToGive);

        // --- CONNECT TO BOOKMANAGER FOR LINE 7 (CRAFT AXE) ---
        if (blueprintToCraft.itemName == "Axe" && BookManager.Instance != null)
        {
            BookManager.Instance.CompleteObjective(7); // Triggers Line 7 Cross-out!
            Debug.Log("Quest Complete: Crafted a primitive Axe!");
        }

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
        // Safe check to avoid null reference crashes if BookManager isn't spawned yet
        bool isBookOpen = (BookManager.Instance != null) && BookManager.Instance.isBookOpen;

        if (Input.GetKeyDown(KeyCode.C) && !isBookOpen)
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
        bool hasAxe = false;

        foreach (string itemName in InventorySystem.Instance.itemList)
        {
            if (itemName == "Stone") stone_count++;
            else if (itemName == "Stick") stick_count++;

            if (itemName == "Axe") hasAxe = true;
        }

        // --- Axe Refresh ---
        AxeReq1.text = "3 Stone [" + stone_count + "]";
        AxeReq2.text = "3 Stick [" + stick_count + "]";

        if (hasAxe)
        {
            craftAxeBTN.gameObject.SetActive(false);
            AxeReq1.text = "ALREADY";
            AxeReq2.text = "OWNED";
        }
        else
        {
            craftAxeBTN.gameObject.SetActive(stone_count >= 3 && stick_count >= 3);
        }

        // --- Arrow Refresh ---
        ArrowReq1.text = "1 Stone [" + stone_count + "]";
        ArrowReq2.text = "1 Stick [" + stick_count + "]";
        craftArrowBTN.gameObject.SetActive(stone_count >= 1 && stick_count >= 1);
    }
}