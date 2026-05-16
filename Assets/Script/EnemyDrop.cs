using System.Collections.Generic;
using UnityEngine;

public class EnemyDrop : MonoBehaviour
{
    [System.Serializable]
    public struct DropItem
    {
        public GameObject itemPrefab;
        [Range(0f, 100f)]
        public float dropChance;
    }

    [Header("Loot Settings")]
    public List<DropItem> lootTable;

    // This is called by EnemyHealth right before the enemy is removed
    public void HandleDeath()
    {
        CalculateDrop();
        // Removed Destroy(gameObject) from here to let EnemyHealth handle it
    }

    private void CalculateDrop()
    {
        if (lootTable == null || lootTable.Count == 0) return;

        float roll = Random.Range(0f, 100f);

        foreach (DropItem item in lootTable)
        {
            // SAFETY FIX: If a prefab slot is blank in the inspector, skip it!
            // Otherwise, Unity crashes the code here and the enemy stays standing forever.
            if (item.itemPrefab == null) continue;

            if (roll <= item.dropChance)
            {
                // Spawns the loot at the enemy's feet
                Instantiate(item.itemPrefab, transform.position, transform.rotation);

                // Keep "break;" if you only want ONE item from the list to drop
                break;
            }
        }
    }
}