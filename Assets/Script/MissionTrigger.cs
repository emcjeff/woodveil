using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class MissionStartTrigger : MonoBehaviour
{
    [Header("UI Settings")]
    [Tooltip("The exact name of your Mission UI Panel GameObject")]
    [SerializeField] private string missionPanelName = "MissionPanel";

    [Header("Multi-Objective Names")]
    [Tooltip("Type the exact names of your 5 Cross-Out Line GameObjects as they appear in the hierarchy")]
    [SerializeField] private List<string> crossOutLineNames = new List<string> { "Line1", "Line2", "Line3", "Line4", "Line5" };

    [Header("Behavior")]
    [SerializeField] private bool pauseGame = true;

    [Header("Auto-Close Settings")]
    [Tooltip("Should the panel close automatically if the player doesn't click anything?")]
    [SerializeField] private bool useAutoCloseTimer = true;
    [Tooltip("Time in seconds before the panel forces itself shut.")]
    [SerializeField] private float autoCloseDelay = 10f;

    // CRITICAL FIX: Global static safety gate to stop multiple instances processing on the same scene load
    private static bool hasTriggeredInThisScene = false;

    private GameObject missionPanel;
    private List<GameObject> objectiveCrossOutLines = new List<GameObject>();

    private Coroutine initCoroutine;
    private Coroutine autoCloseCoroutine;

    private void Awake()
    {
        // Reset the safety gate whenever the script instantiates/starts clean
        hasTriggeredInThisScene = false;
    }

    private void Start()
    {
        initCoroutine = StartCoroutine(DelayedStartRoutine());
    }

    private IEnumerator DelayedStartRoutine()
    {
        yield return new WaitForEndOfFrame();

        // If another copy already processed this frame, turn this copy off immediately and exit
        if (hasTriggeredInThisScene)
        {
            Debug.Log($"[MissionStartTrigger] Duplicate script instance detected on '{gameObject.name}'. Deactivating copy to prevent double UI pops.");
            this.enabled = false;
            yield break;
        }

        FindUIElementsInScene();
        UpdateAllObjectiveVisuals();

        // Check if we arrived via a standard, fresh Main Menu click
        if (MainMenu.cameFromMenu)
        {
            Debug.Log("[MissionStartTrigger] Fresh entry context detected. Activating UI.");

            // LOCK THE GATE IN_FRAME SO DUPLICATES CANNOT ENTER
            hasTriggeredInThisScene = true;
            MainMenu.cameFromMenu = false;

            ShowMission();
        }
        else
        {
            // If it's a retry or standard map transition, suppress the pop-up to avoid double-freezing
            Debug.Log("[MissionStartTrigger] Retrying or continuous play detected. Suppressing popup entry layout.");

            hasTriggeredInThisScene = true; // Lock out duplicates even on a bypass

            if (missionPanel != null)
            {
                missionPanel.SetActive(false);
            }
            this.enabled = false;
        }

        initCoroutine = null;
    }

    private void FindUIElementsInScene()
    {
        Transform[] allTransforms = Resources.FindObjectsOfTypeAll<Transform>();
        objectiveCrossOutLines.Clear();
        Dictionary<string, GameObject> foundLines = new Dictionary<string, GameObject>();

        foreach (Transform t in allTransforms)
        {
            if (t.hideFlags == HideFlags.None)
            {
                if (t.name == missionPanelName) missionPanel = t.gameObject;
                if (crossOutLineNames.Contains(t.name)) foundLines[t.name] = t.gameObject;
            }
        }

        foreach (string lineName in crossOutLineNames)
        {
            if (foundLines.ContainsKey(lineName)) objectiveCrossOutLines.Add(foundLines[lineName]);
        }
    }

    public void ShowMission()
    {
        if (missionPanel == null) return;

        UpdateAllObjectiveVisuals();
        missionPanel.SetActive(true);

        Button acceptButton = missionPanel.GetComponentInChildren<Button>();
        if (acceptButton != null)
        {
            acceptButton.onClick.RemoveAllListeners();
            acceptButton.onClick.AddListener(CloseMission);
        }

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (pauseGame)
        {
            Time.timeScale = 0f;
        }

        if (useAutoCloseTimer)
        {
            if (autoCloseCoroutine != null) StopCoroutine(autoCloseCoroutine);
            autoCloseCoroutine = StartCoroutine(AutoCloseCountdownRoutine());
        }
    }

    private IEnumerator AutoCloseCountdownRoutine()
    {
        yield return new WaitForSecondsRealtime(autoCloseDelay);
        Debug.Log("[MissionStartTrigger] Auto-closing panel context via delay timeout.");
        CloseMission();
    }

    public void CloseMission()
    {
        if (initCoroutine != null)
        {
            StopCoroutine(initCoroutine);
            initCoroutine = null;
        }

        if (autoCloseCoroutine != null)
        {
            StopCoroutine(autoCloseCoroutine);
            autoCloseCoroutine = null;
        }

        if (missionPanel != null)
        {
            missionPanel.SetActive(false);
        }

        // Only resume time if another UI element (like BookManager) isn't actively forcing a pause
        bool isBookOpen = (BookManager.Instance != null && BookManager.Instance.isBookOpen);
        if (!isBookOpen)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            if (pauseGame) Time.timeScale = 1f;
        }

        this.enabled = false;
    }

    public void CompleteObjective(int objectiveNumber)
    {
        if (BookManager.Instance != null)
        {
            BookManager.Instance.CompleteObjective(objectiveNumber);
            FindUIElementsInScene();
            UpdateAllObjectiveVisuals();
        }
    }

    private void UpdateAllObjectiveVisuals()
    {
        if (BookManager.Instance == null) return;

        for (int i = 0; i < objectiveCrossOutLines.Count; i++)
        {
            if (objectiveCrossOutLines[i] != null)
            {
                bool isCompleted = BookManager.Instance.IsObjectiveComplete(i + 1);
                objectiveCrossOutLines[i].SetActive(isCompleted);
            }
        }
    }
}