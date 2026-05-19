using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using UnityEngine.UI;

public class BookManager : MonoBehaviour
{
    public static BookManager Instance { get; private set; }

    [Header("Global Save State")]
    public bool[] completedObjectives = new bool[10];

    [Tooltip("Tracks which physical pages are owned. Index 0 = Page 1, Index 1 = Page 2, etc.")]
    public List<bool> unlockedPages = new List<bool>();

    [Header("UI Objects")]
    public GameObject bookUI;
    public GameObject BookPrompt;

    [Header("One-Time Spawn Paper")]
    [SerializeField] private GameObject introMissionPaper;

    [Header("Mission Customization")]
    [SerializeField] private List<string> crossOutLineNames = new List<string> { "Line1", "Line2", "Line3", "Line4", "Line5", "Line6", "Line7", "Line8", "Line9", "Line10" };

    [Header("Quest Trackers (Slimes)")]
    [SerializeField] private int slimeTier1Required = 3;
    [SerializeField] private int slimeTier2Required = 10;
    private static int currentSlimeKills = 0;

    [Header("Quest Trackers (Spiders)")]
    [SerializeField] private int spiderRequiredKills = 10;
    private static int currentSpiderKills = 0;

    [Header("Editor Testing Toggles")]
    [SerializeField] private bool enableTestToggles = false;
    [SerializeField] private bool[] testObjectivesState = new bool[10];

    [HideInInspector] public bool isBookOpen = false;
    [HideInInspector] public bool hasBook = false;

    private List<GameObject> objectiveCrossOutLines = new List<GameObject>();
    private bool isIntroPaperActive = false;
    private static bool hasShownIntroPaper = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Setup base page status safely at the earliest lifecycle frame
        InitializeBasePages();
    }

    private void InitializeBasePages()
    {
        if (unlockedPages == null || unlockedPages.Count == 0)
        {
            unlockedPages = new List<bool>();
            // Pre-populate slots. Let's give it 10 slots by default to match objectives
            for (int i = 0; i < 10; i++)
            {
                unlockedPages.Add(i == 0); // Index 0 (Page 1) is unlocked out of the box!
            }
        }
    }

    private void OnEnable() { SceneManager.sceneLoaded += OnSceneLoaded; }
    private void OnDisable() { SceneManager.sceneLoaded -= OnSceneLoaded; }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        CloseBook();

        if (scene.name == "wodbeyl" && !hasShownIntroPaper)
        {
            Invoke("ShowIntroMissionPaper", 0.1f);
        }

        if (scene.name == "Cave")
        {
            CompleteObjective(8);
            Debug.Log("Quest Complete: Entered the spooky Cave!");
        }
    }

    void Update()
    {
        if (enableTestToggles && Application.isPlaying)
        {
            for (int i = 0; i < completedObjectives.Length; i++)
            {
                completedObjectives[i] = testObjectivesState[i];
            }
            FindAndRefreshLines();
        }

        if (hasBook && !isBookOpen && !InventorySystem.Instance.isOpen && !CraftingSystem.Instance.isOpen)
        {
            BookPrompt.SetActive(true);
        }
        else
        {
            BookPrompt.SetActive(false);
        }

        if (Input.GetKeyDown(KeyCode.E) && hasBook && !isIntroPaperActive)
        {
            if (isBookOpen) CloseBook();
            else OpenBook();
        }
    }

    public void ShowIntroMissionPaper()
    {
        if (introMissionPaper == null) return;
        hasShownIntroPaper = true;
        isIntroPaperActive = true;
        bookUI.SetActive(false);
        introMissionPaper.SetActive(true);

        Button closeButton = introMissionPaper.GetComponentInChildren<Button>();
        if (closeButton != null)
        {
            closeButton.onClick.RemoveAllListeners();
            closeButton.onClick.AddListener(CloseIntroMissionPaper);
        }
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        Time.timeScale = 0f;
    }

    public void CloseIntroMissionPaper()
    {
        if (introMissionPaper != null) introMissionPaper.SetActive(false);
        isIntroPaperActive = false;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        Time.timeScale = 1f;
    }

    public void CollectBook()
    {
        hasBook = true;
        CompleteObjective(1);
    }

    public void RegisterSlimeKill(string enemyName)
    {
        if (enableTestToggles) return;
        if (enemyName.Contains("SlimeGreen") && !completedObjectives[4])
        {
            CompleteObjective(5);
        }
        currentSlimeKills++;
        if (currentSlimeKills >= slimeTier1Required && !completedObjectives[2]) CompleteObjective(3);
        if (currentSlimeKills >= slimeTier2Required && !completedObjectives[3]) CompleteObjective(4);
    }

    public void RegisterSpiderKill(string enemyName)
    {
        if (enableTestToggles) return;
        currentSpiderKills++;
        if (currentSpiderKills >= spiderRequiredKills && !completedObjectives[8]) CompleteObjective(9);
    }

    public void RegisterSpiderBossKill()
    {
        if (enableTestToggles) return;
        if (!completedObjectives[9]) CompleteObjective(10);
    }

    public void OpenBook()
    {
        bookUI.SetActive(true);
        isBookOpen = true;

        // Tell the visual layout rendering module to draw itself using our data
        book pageScript = bookUI.GetComponentInChildren<book>(true);
        if (pageScript != null)
        {
            pageScript.SyncAndRenderLayout();
        }

        FindAndRefreshLines();
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (SelectionManager.Instance != null) SelectionManager.Instance.DisableSelection();
    }

    public void CloseBook()
    {
        book pageScript = bookUI.GetComponentInChildren<book>(true);
        if (pageScript != null) pageScript.ResetState();

        bookUI.SetActive(false);
        isBookOpen = false;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (SelectionManager.Instance != null) SelectionManager.Instance.EnableSelection();
    }

    public void CompleteObjective(int objectiveNumber)
    {
        if (enableTestToggles) return;
        int index = objectiveNumber - 1;
        if (index >= 0 && index < completedObjectives.Length)
        {
            completedObjectives[index] = true;
            FindAndRefreshLines();

            ProgressionGate[] gates = FindObjectsByType<ProgressionGate>(FindObjectsSortMode.None);
            foreach (ProgressionGate gate in gates) gate.EvaluateGate();
        }
    }

    public bool IsObjectiveComplete(int objectiveLineNumber)
    {
        int targetIndex = objectiveLineNumber - 1;
        if (targetIndex >= 0 && targetIndex < completedObjectives.Length) return completedObjectives[targetIndex];
        return false;
    }

    // FIXED: Reads data directly from our safe local data array instance layout seamlessly!
    public bool IsPageUnlocked(int pageIndex)
    {
        InitializeBasePages();
        if (pageIndex >= 0 && pageIndex < unlockedPages.Count)
        {
            return unlockedPages[pageIndex];
        }
        return false;
    }

    public void UnlockPageGlobal(int pageIndex)
    {
        InitializeBasePages();
        if (pageIndex >= 0 && pageIndex < unlockedPages.Count)
        {
            unlockedPages[pageIndex] = true;
            Debug.Log($"[Global Book System] Page Index {pageIndex} marked unlocked!");

            // If the UI happens to be active, force recalculate it visually
            book pageScript = bookUI.GetComponentInChildren<book>(true);
            if (pageScript != null) pageScript.SyncAndRenderLayout();

            // Notify world items to check their states right now
            ProgressionGate[] gates = FindObjectsByType<ProgressionGate>(FindObjectsSortMode.None);
            foreach (ProgressionGate gate in gates) gate.EvaluateGate();
        }
    }

    private void FindAndRefreshLines()
    {
        Transform[] allChildren = bookUI.GetComponentsInChildren<Transform>(true);
        objectiveCrossOutLines.Clear();
        Dictionary<string, GameObject> foundLines = new Dictionary<string, GameObject>();

        foreach (Transform child in allChildren)
        {
            if (crossOutLineNames.Contains(child.name)) foundLines[child.name] = child.gameObject;
        }
        foreach (string lineName in crossOutLineNames)
        {
            if (foundLines.ContainsKey(lineName)) objectiveCrossOutLines.Add(foundLines[lineName]);
        }
        for (int i = 0; i < objectiveCrossOutLines.Count; i++)
        {
            if (objectiveCrossOutLines[i] != null) objectiveCrossOutLines[i].SetActive(completedObjectives[i]);
        }
        CheckMasterRewardUnlock();
    }

    private void CheckMasterRewardUnlock()
    {
        if (BowController.Instance != null)
        {
            BowController.Instance.isDoubleShotUnlocked = (completedObjectives[6] && completedObjectives[7] && completedObjectives[8]);
        }
    }
}