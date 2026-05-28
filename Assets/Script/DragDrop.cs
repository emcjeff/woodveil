using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class DragDrop : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private Canvas canvas;

    public static GameObject itemBeingDragged;
    private Vector3 startPosition;
    private Transform startParent;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();

        // 1. Try to find canvas in parents
        canvas = GetComponentInParent<Canvas>();

        // 2. If not found (item might be spawned outside), find the main one in the scene
        if (canvas == null)
        {
            canvas = GameObject.FindFirstObjectByType<Canvas>();
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (canvas == null)
        {
            Debug.LogError("DragDrop: No Canvas found in the scene!");
            return;
        }

        Debug.Log("OnBeginDrag");
        canvasGroup.alpha = 0.6f;

        // This allows the raycast to hit the Slot behind the item
        canvasGroup.blocksRaycasts = false;

        startPosition = transform.position;
        startParent = transform.parent;

        // Move to canvas level so it draws on top of all other UI
        transform.SetParent(canvas.transform);
        itemBeingDragged = gameObject;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (canvas == null) return;

        // Moves the item 1:1 with the mouse by accounting for UI Scale
        rectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        itemBeingDragged = null;
        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true;

        // Safety check to prevent the NullReferenceException on line 53
        if (canvas == null) return;

        // Logic: If the item is still a child of the Canvas, it wasn't dropped in a Slot
        if (transform.parent == canvas.transform)
        {
            transform.SetParent(startParent);
            transform.position = startPosition;
            Debug.Log("Returned to start");
        }
        else
        {
            // If the Slot script (OnDrop) changed the parent, snap it to the center
            rectTransform.anchoredPosition = Vector2.zero;
            Debug.Log("Snapped to new slot");
        }
    }
}