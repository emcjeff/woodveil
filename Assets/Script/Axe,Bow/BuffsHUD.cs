using UnityEngine;

public class BuffDisplayHUD : MonoBehaviour
{
    [Header("UI Icon GameObjects")]
    [Tooltip("The image icon panel that displays when damage boost is unlocked.")]
    [SerializeField] private GameObject damageBoostIcon;

    [Tooltip("The image icon panel that displays when double shot is unlocked.")]
    [SerializeField] private GameObject doubleShotIcon;

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
        // RULE: If the player isn't holding the bow (or it's inactive/hidden), hide icons instantly
        if (BowController.Instance == null || !BowController.Instance.gameObject.activeInHierarchy)
        {
            SetAllIconsActive(false);
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

    private void SetAllIconsActive(bool state)
    {
        if (damageBoostIcon != null) damageBoostIcon.SetActive(state);
        if (doubleShotIcon != null) doubleShotIcon.SetActive(state);
    }
}