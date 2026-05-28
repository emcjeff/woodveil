using UnityEngine;
using System.Collections;

public class UniqueItemPersistence : MonoBehaviour
{
    public enum UniqueItemType { Book, Headlamp, Bow, Page }

    [Header("Unique Item Settings")]
    [Tooltip("Select what kind of unique item this specific object is.")]
    public UniqueItemType uniqueType;

    [Header("Page Specific Settings")]
    [Tooltip("Only used if Unique Type is set to 'Page'. Match this exactly with your InteractableObject page index!")]
    public int pageIndex;

    private void OnEnable()
    {
        StartCoroutine(CheckPersistenceRoutine());
    }

    private IEnumerator CheckPersistenceRoutine()
    {
        // Wait until the very end of the frame. 
        // This guarantees all Managers have fully initialized their data structures!
        yield return new WaitForEndOfFrame();

        switch (uniqueType)
        {
            case UniqueItemType.Book:
                if (BookManager.Instance != null && BookManager.Instance.hasBook)
                {
                    Debug.Log("[Persistence] Player already collected the Book. Removing duplicate.");
                    Destroy(gameObject);
                }
                break;

            case UniqueItemType.Headlamp:
                if (FlashlightController.Instance != null && FlashlightController.Instance.hasHelmet)
                {
                    Debug.Log("[Persistence] Player already collected the Headlamp. Removing duplicate.");
                    Destroy(gameObject);
                }
                break;

            case UniqueItemType.Bow:
                if (InventorySystem.Instance != null)
                {
                    // FIXED: Checks for both "Bow" and "BowUI" case-insensitively!
                    bool carriesBow = InventorySystem.Instance.itemList.Exists(item =>
                        string.Equals(item, "Bow", System.StringComparison.OrdinalIgnoreCase) ||
                        string.Equals(item, "BowUI", System.StringComparison.OrdinalIgnoreCase));

                    if (carriesBow)
                    {
                        Debug.Log("[Persistence] Bow/BowUI data found in inventory array list. Removing GroundBow.");
                        Destroy(gameObject);
                    }
                }
                break;

            case UniqueItemType.Page:
                if (BookManager.Instance != null && BookManager.Instance.IsPageUnlocked(pageIndex))
                {
                    Debug.Log($"[Persistence] Page {pageIndex + 1} is already unlocked globally. Removing scene duplicate.");
                    Destroy(gameObject);
                }
                break;
        }
    }
}