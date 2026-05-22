using UnityEngine;
using TMPro;
using System.Text;
using System.Collections.Generic;

public class QuestUI : MonoBehaviour
{
    [System.Serializable]
    public struct PageObjectiveMapping
    {
        public int pageIndex;
        [Tooltip("The objective numbers matching BookManager (e.g., 1 for CollectBook, 3 for 3 Slimes, etc.)")]
        public List<int> objectiveNumbers;
        [Tooltip("The clear text displayed on the screen for this mission.")]
        public List<string> objectiveDescriptions;
    }

    [Header("UI Components")]
    [SerializeField] private TMP_Text questTrackerText;
    [SerializeField] private GameObject trackerPanelContainer;

    [Header("Quest Data Settings")]
    [SerializeField] private List<PageObjectiveMapping> questDatabase = new List<PageObjectiveMapping>();
    [SerializeField] private string noObjectivesMessage = "Get a page to get new objectives";

    // Performance Optimization: Single string builder instance reused across updates
    private StringBuilder builder = new StringBuilder();

    private void Start()
    {
        if (trackerPanelContainer != null)
        {
            trackerPanelContainer.SetActive(true);
        }

        RefreshTrackerDisplay();
    }

    private void Update()
    {
        // Safety check if BookManager isn't present in the scene
        if (BookManager.Instance == null)
        {
            if (trackerPanelContainer != null && trackerPanelContainer.activeSelf)
                trackerPanelContainer.SetActive(false);
            return;
        }

        // Keep the panel active if BookManager exists
        if (trackerPanelContainer != null && !trackerPanelContainer.activeSelf)
        {
            trackerPanelContainer.SetActive(true);
        }

        RefreshTrackerDisplay();
    }

    public void RefreshTrackerDisplay()
    {
        if (questTrackerText == null) return;

        // Clear previous frame data without reallocating memory
        builder.Clear();
        builder.AppendLine("<color=#FFFFFF><b>CURRENT OBJECTIVES</b></color>");

        bool activeQuestsFound = false;
        int displayedCount = 0; 

        // Cycle through all structural mapping blocks configured in the inspector
        foreach (var mapping in questDatabase)
        {
            if (displayedCount >= 3) break;

            if (BookManager.Instance.IsPageUnlocked(mapping.pageIndex))
            {
                for (int i = 0; i < mapping.objectiveNumbers.Count; i++)
                {
                    if (displayedCount >= 3) break;

                    int objectiveNum = mapping.objectiveNumbers[i];
                    bool isComplete = BookManager.Instance.IsObjectiveComplete(objectiveNum);

                    // Skip rule: If finished, drop it out of the HUD cycle entirely
                    if (isComplete)
                    {
                        continue; 
                    }

                    string rawDescription = mapping.objectiveDescriptions[i];
                    string finalDescription = InjectDynamicCounters(objectiveNum, rawDescription);

                    // Formatted with fixed plain bullet points to replace bad character codes
                    builder.AppendLine($"<color=#000000>- {finalDescription}</color>");

                    activeQuestsFound = true;
                    displayedCount++; 
                }
            }
        }

        // If no incomplete active quests are found on unlocked pages, show the instruction prompt
        if (!activeQuestsFound)
        {
            builder.AppendLine($"<color=#000000>{noObjectivesMessage}</color>");
        }

        questTrackerText.text = builder.ToString();
    }

    private string InjectDynamicCounters(int objectiveNum, string description)
    {
        if (description.Contains("{slimes}"))
        {
            int currentKills = GetPrivateStaticInt(typeof(BookManager), "currentSlimeKills");
            
            // Evaluates target constraints based on custom database requirements
            int required = 3; 
            if (objectiveNum == 4) required = 10; // Maps your new "Kill 10 slime" target ID

            return description.Replace("{slimes}", $"{currentKills}/{required}");
        }

        if (description.Contains("{spiders}"))
        {
            int currentKills = GetPrivateStaticInt(typeof(BookManager), "currentSpiderKills");
            return description.Replace("{spiders}", $"{currentKills}/10");
        }

        return description;
    }

    private int GetPrivateStaticInt(System.Type type, string fieldName)
    {
        var field = type.GetField(fieldName, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
        if (field != null)
        {
            return (int)field.GetValue(null);
        }
        return 0;
    }
}