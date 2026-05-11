using UnityEngine;
using System.Collections.Generic;

public class EquipManager : MonoBehaviour
{
    public static EquipManager Instance { get; set; }

    [Header("Weapon Models in Hand")]
    public GameObject axeInHand;
    public GameObject bowInHand;

    private void Awake()
    {
        if (Instance != null && Instance != this) Destroy(gameObject);
        else Instance = this;
    }

    void Update()
    {
        // Press '1' for Axe, '2' for Bow
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            EquipWeapon("Axe");
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            EquipWeapon("BowUI");
        }
    }

    public void EquipWeapon(string name)
    {
        if (InventorySystem.Instance.itemList.Contains(name))
        {
            if (name == "Axe")
            {
                axeInHand.SetActive(true);
                bowInHand.SetActive(false);

                // FIX: Search children for the AxeController (since it's on the Hitbox)
                AxeController axeScript = axeInHand.GetComponentInChildren<AxeController>();
                if (axeScript != null) axeScript.enabled = true;

                if (BowController.Instance != null) BowController.Instance.enabled = false;
            }
            else if (name == "BowUI")
            {
                axeInHand.SetActive(false);
                bowInHand.SetActive(true);

                // FIX: Search children to disable the AxeController
                AxeController axeScript = axeInHand.GetComponentInChildren<AxeController>();
                if (axeScript != null) axeScript.enabled = false;

                if (BowController.Instance != null) BowController.Instance.enabled = true;
            }
        }
    }
}