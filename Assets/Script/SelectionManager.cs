using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SelectionManager : MonoBehaviour
{
    public static SelectionManager Instance { get; set; }

    public bool onTarget;

    public GameObject selectedObject;

    public GameObject Interaction_Info_UI;
    TextMeshProUGUI interaction_text;

    public Image centerDotImage;
    public Image handIcon;

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

    void Update()
    {
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
                onTarget = false;
                Interaction_Info_UI.SetActive(false);
            }
        }
        else
        {
            onTarget = false;
            Interaction_Info_UI.SetActive(false);
        }
    }
        public void DisableSelection()
{
    handIcon.enabled = false;
    if (handIcon != null)
    {
        handIcon.enabled = false;
    }
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
}