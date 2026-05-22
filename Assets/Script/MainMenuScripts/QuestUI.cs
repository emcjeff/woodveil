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
        if (BookManager.Instance == null)
        {
            if (trackerPanelContainer != null && trackerPanelContainer.activeSelf)
                trackerPanelContainer.SetActive(false);
            return;
        }

        if (trackerPanelContainer != null && !trackerPanelContainer.activeSelf)
        {
            trackerPanelContainer.SetActive(true);
        }

        RefreshTrackerDisplay();
    }

    private void RefreshTrackerDisplay()
    {
        if (questTrackerText == null) return;

        StringBuilder builder = new StringBuilder();
        builder.AppendLine("<color=#FFFFFF>CURRENT OBJECTIVES</color>");

        bool activeQuestsFound = false;
        int displayedCount = 0; // Tracks how many lines we have drawn on the HUD

        // Iterate through all quest groups defined in our database
        foreach (var mapping in questDatabase)
        {
            if (BookManager.Instance.IsPageUnlocked(mapping.pageIndex))
            {
                for (int i = 0; i < mapping.objectiveNumbers.Count; i++)
                {
                    // If we have already hit our 3-objective screen limit, stop adding lines entirely
                    if (displayedCount >= 3)
                    {
                        break;
                    }

                    int objectiveNum = mapping.objectiveNumbers[i];
                    bool isComplete = BookManager.Instance.IsObjectiveComplete(objectiveNum);
                    string rawDescription = mapping.objectiveDescriptions[i];

                    string finalDescription = InjectDynamicCounters(objectiveNum, rawDescription);

                    if (isComplete)
                    {
                        builder.AppendLine($"<s><color=#888888>• {finalDescription}</color></s>");
                    }
                    else
                    {
                        builder.AppendLine($"<color=#FFFFFF>• {finalDescription}</color>");
                    }

                    activeQuestsFound = true;
                    displayedCount++; // Increment count for each displayed objective
                }
            }

            // Break out of the outer loop too if the limit has been filled
            if (displayedCount >= 3)
            {
                break;
            }
        }

        if (!activeQuestsFound)
        {
            builder.AppendLine("<color=#AAAAAA>No active level entries found.</color>");
        }

        questTrackerText.text = builder.ToString();
    }

    private string InjectDynamicCounters(int objectiveNum, string description)
    {
        if (description.Contains("{slimes}"))
        {
            int currentKills = GetPrivateStaticInt(typeof(BookManager), "currentSlimeKills");
            int required = (objectiveNum == 3) ? 3 : 10;
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