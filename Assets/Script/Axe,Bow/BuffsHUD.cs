using UnityEngine;

public class BuffDisplayHUD : MonoBehaviour
{
    [Header("UI Icon GameObjects")]
    [Tooltip("The image icon panel that displays when damage boost is unlocked.")]
    [SerializeField] private GameObject damageBoostIcon;

    [Tooltip("The image icon panel that displays when double shot is unlocked.")]
    [SerializeField] private GameObject doubleShotIcon;

    [Tooltip("The image icon panel that displays when the player gets a speed boost from the Axe.")]
    [SerializeField] private GameObject axeSpeedBuffIcon;

    void Start()
    {
        RefreshBuffDisplay();
    }

    void Update()
    {
        // Keep visual layout states synced directly with active triggers
        RefreshBuffDisplay();
    }

    private void RefreshBuffDisplay()
    {
        // 1. --- AXE SPEED BUFF LOGIC ---
        if (axeSpeedBuffIcon != null)
        {
            // The icon lights up automatically if EquipManager says the Axe is in your hands
            bool isAxeActive = (EquipManager.Instance != null && EquipManager.Instance.IsAxeEquipped());
            axeSpeedBuffIcon.SetActive(isAxeActive);
        }

        // 2. --- BOW BUFFS LOGIC ---
        // RULE: If the player isn't holding the bow (or it's inactive/hidden), hide bow icons instantly
        if (BowController.Instance == null || !BowController.Instance.gameObject.activeInHierarchy)
        {
            SetBowIconsActive(false);
            return;
        }

        // If the player IS holding the bow, sync layout to its unlocked quest states
        if (damageBoostIcon != null)
        {
            damageBoostIcon.SetActive(BowController.Instance.isDamageBoostUnlocked);
        }

        if (doubleShotIcon != null)
        {
            doubleShotIcon.SetActive(BowController.Instance.isDoubleShotUnlocked);
        }
    }

    private void SetBowIconsActive(bool state)
    {
        if (damageBoostIcon != null) damageBoostIcon.SetActive(state);
        if (doubleShotIcon != null) doubleShotIcon.SetActive(state);
    }
}