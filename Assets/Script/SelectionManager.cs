using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SelectionManager : MonoBehaviour
{
    public static SelectionManager Instance { get; private set; }

    public bool onTarget;
    public GameObject selectedObject;

    public GameObject Interaction_Info_UI;
    TextMeshProUGUI interaction_text;

    public Image centerDotImage;
    public Image handIcon;

    public float damage = 20f; // Damage for attacks

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
        onTarget = false;

        if (Interaction_Info_UI != null)
        {
            interaction_text = Interaction_Info_UI.GetComponent<TextMeshProUGUI>();
        }
        else
        {
            Debug.LogError("Interaction_Info_UI is NOT assigned!");
        }
    }

    void Update()
    {
        // 1. HANDLE SELECTION (RAYCAST)
        Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            var selectionTransform = hit.transform;
            InteractableObject interactable = selectionTransform.GetComponent<InteractableObject>();

            if (interactable && interactable.playerInRange)
            {
                onTarget = true;
                selectedObject = interactable.gameObject;
                interaction_text.text = interactable.GetItemName();
                Interaction_Info_UI.SetActive(true);

                if (interactable.CompareTag("pickable"))
                {
                    centerDotImage.gameObject.SetActive(false);
                    handIcon.gameObject.SetActive(true);
                }
                else
                {
                    handIcon.gameObject.SetActive(false);
                    centerDotImage.gameObject.SetActive(true);
                }

                // 2. HANDLE INTERACTION (PICKUP)
                if (Input.GetMouseButtonDown(0))
                {
                    if (BowController.Instance != null && !BowController.Instance.IsBusy())
                    {
                        interactable.PickUp();
                    }
                }
            }
            else
            {
                // Also check if we are hitting an Enemy that isn't an "InteractableObject"
                EnemyHealth enemy = selectionTransform.GetComponent<EnemyHealth>();
                if(enemy != null)
                {
                    selectedObject = enemy.gameObject;
                }
                else
                {
                    selectedObject = null;
                }
                
                ResetSelectionUI();
            }
        }
        else
        {
            selectedObject = null;
            ResetSelectionUI();
        }

        // 3. HANDLE COMBAT (TAKE DAMAGE)
    // Change "GetButtonDown" to "GetMouseButtonUp" to match the bow release
    if (Input.GetMouseButtonUp(0)) 
    {
    if (selectedObject != null)
    {
        // Now this will be true the exact same frame the arrow is created
        if (BowController.Instance != null && BowController.Instance.IsFired()) 
        {
            EnemyHealth health = selectedObject.GetComponent<EnemyHealth>();
            if (health != null)
            {
                health.TakeDamage(damage);
            }
        }
    }
}
    } // This bracket CLOSES the Update method

    // These functions must be OUTSIDE of Update, but INSIDE the class
    private void ResetSelectionUI()
    {
        onTarget = false;
        Interaction_Info_UI.SetActive(false);
        handIcon.gameObject.SetActive(false);
        centerDotImage.gameObject.SetActive(true);
    }

    public void DisableSelection()
    {
        if (handIcon != null) handIcon.enabled = false;
        centerDotImage.enabled = false;
        Interaction_Info_UI.SetActive(false);
        selectedObject = null;
    }

    public void EnableSelection()
    {
        enabled = true;
        handIcon.enabled = true;
        centerDotImage.enabled = true;
    }
} // This bracket CLOSES the SelectionManager class