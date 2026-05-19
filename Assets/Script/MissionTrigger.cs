using System.Collections;
using System.Collections.Generic;
using UnityEngine;
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

    private GameObject missionPanel;
    private List<GameObject> objectiveCrossOutLines = new List<GameObject>();
    private static bool hasShownMission = false;

    private void Start()
    {
        // 1. Find the panel and lines inside the canvas hierarchy
        FindUIElementsInScene();

        // 2. Sync the lines directly from BookManager's global state data array
        UpdateAllObjectiveVisuals();

        // If the initial intro pop-up already happened, turn off this trigger box right away
        if (hasShownMission)
        {
            gameObject.SetActive(false);
        }
    }

    private void FindUIElementsInScene()
    {
        // Advanced search to search through ALL scenes, including DontDestroyOnLoad layouts
        Transform[] allTransforms = Resources.FindObjectsOfTypeAll<Transform>();

        // Clear old references to avoid double-stacking bugs
        objectiveCrossOutLines.Clear();

        // Temporary dictionary to hold lines as we find them so they stay in order
        Dictionary<string, GameObject> foundLines = new Dictionary<string, GameObject>();

        foreach (Transform t in allTransforms)
        {
            if (t.hideFlags == HideFlags.None)
            {
                // Find the main panel
                if (t.name == missionPanelName)
                {
                    missionPanel = t.gameObject;
                }

                // Check if this object matches one of our line names
                if (crossOutLineNames.Contains(t.name))
                {
                    foundLines[t.name] = t.gameObject;
                }
            }
        }

        // Reconstruct the lines list in the exact order you typed them in the Inspector
        foreach (string lineName in crossOutLineNames)
        {
            if (foundLines.ContainsKey(lineName))
            {
                objectiveCrossOutLines.Add(foundLines[lineName]);
            }
            else
            {
                Debug.LogWarning($"MissionStartTrigger: Missing line element named '{lineName}' in the UI hierarchy!");
            }
        }

        if (missionPanel == null)
        {
            Debug.LogWarning($"MissionStartTrigger: Could not find any GameObject named '{missionPanelName}' anywhere!");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !hasShownMission)
        {
            // Safeguard refresh in case the UI was loaded late
            FindUIElementsInScene();
            ShowMission();
        }
    }

    public void ShowMission()
    {
        if (missionPanel == null) return;

        hasShownMission = true;
        UpdateAllObjectiveVisuals();
        missionPanel.SetActive(true);

        // Auto link the Accept button
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
    }

    public void CloseMission()
    {
        if (missionPanel == null) FindUIElementsInScene();

        if (missionPanel != null)
        {
            missionPanel.SetActive(false);
        }

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (pauseGame)
        {
            Time.timeScale = 1f;
        }

        // Disable collider so player doesn't trip the popup again while running around wodbeyl
        Collider col = GetComponent<Collider>();
        if (col != null) col.enabled = false;
    }

    // FIXED: Routes completion through the centralized BookManager instance instead of a local static array
    public void CompleteObjective(int objectiveNumber)
    {
        if (BookManager.Instance != null)
        {
            // Update central state registry array
            BookManager.Instance.CompleteObjective(objectiveNumber);

            // Make sure we have local UI elements tracked down, then update the line strikes
            FindUIElementsInScene();
            UpdateAllObjectiveVisuals();
            Debug.Log($"MissionStartTrigger: Syncing Objective {objectiveNumber} with Global BookManager.");
        }
        else
        {
            Debug.LogError("MissionStartTrigger: Cannot complete objective because BookManager is missing from the level framework!");
        }
    }

    // FIXED: Reads verification data parameters entirely out of your global singleton memory storage asset
    private void UpdateAllObjectiveVisuals()
    {
        if (BookManager.Instance == null) return;

        for (int i = 0; i < objectiveCrossOutLines.Count; i++)
        {
            if (objectiveCrossOutLines[i] != null)
            {
                // Check objective index tracking via BookManager's central array matrix layout
                bool isCompleted = BookManager.Instance.IsObjectiveComplete(i + 1);
                objectiveCrossOutLines[i].SetActive(isCompleted);
            }
        }
    }
}