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

    public GameObject InteractionInfo;
    TextMeshProUGUI interaction_text;

    private void Start()
    {
        onTarget = false;
        interaction_text = InteractionInfo.GetComponent<TextMeshProUGUI>();
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
                InteractionInfo.SetActive(true);

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
                InteractionInfo.SetActive(false);
            }
        }
        else
        {
            onTarget = false;
            InteractionInfo.SetActive(false);
        }
    }
}