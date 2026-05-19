using UnityEngine;

public class ProgressionGate : MonoBehaviour
{
    [Header("Gate Configuration")]
    [Tooltip("The EXACT array index slot (0, 1, 2...) the player must own in their book to make this object appear.")]
    [SerializeField] private int requiredPageIndex = 0;

    private Renderer[] childRenderers;
    private Collider[] childColliders;
    private bool isInitialized = false;

    void Awake()
    {
        CacheComponents();
        // Start completely invisible on frame 1
        SetItemVisibility(false);
    }

    void Start()
    {
        EvaluateGate();
    }

    private void CacheComponents()
    {
        if (isInitialized) return;
        childRenderers = GetComponentsInChildren<Renderer>(true);
        childColliders = GetComponentsInChildren<Collider>(true);
        isInitialized = true;
    }

    public void EvaluateGate()
    {
        CacheComponents();

        if (BookManager.Instance == null) return;

        // FIXED: Directly checks the exact index you type in the inspector! No more "- 1" subtraction.
        if (BookManager.Instance.IsPageUnlocked(requiredPageIndex))
        {
            SetItemVisibility(true);
            Debug.Log($"[ProgressionGate] Player owns index {requiredPageIndex}! Revealing world item: {gameObject.name}");
        }
        else
        {
            SetItemVisibility(false);
        }
    }

    private void SetItemVisibility(bool visible)
    {
        if (childRenderers != null)
        {
            foreach (Renderer r in childRenderers) if (r != null) r.enabled = visible;
        }
        if (childColliders != null)
        {
            foreach (Collider c in childColliders) if (c != null) c.enabled = visible;
        }
    }
}