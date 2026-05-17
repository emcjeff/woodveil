using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using UnityEngine.UI;

public class BookManager : MonoBehaviour
{
    public static BookManager Instance { get; private set; }

    [Header("UI Objects")]
    [Tooltip("The actual main Book UI panel that opens when pressing E")]
    public GameObject bookUI;
    [Tooltip("The 'Press E to Open Book' text/prompt object")]
    public GameObject BookPrompt;

    [Header("One-Time Spawn Paper")]
    [Tooltip("The temporary mission paper that shows up at spawn and vanishes forever when closed")]
    [SerializeField] private GameObject introMissionPaper;

    [Header("Mission Customization")]
    [Tooltip("Type the exact names of your 10 Cross-Out Line GameObjects inside the book pages")]
    [SerializeField] private List<string> crossOutLineNames = new List<string> { "Line1", "Line2", "Line3", "Line4", "Line5", "Line6", "Line7", "Line8", "Line9", "Line10" };

    [Header("Quest Trackers (Slimes)")]
    [SerializeField] private int slimeTier1Required = 3;  // For Objective 3
    [SerializeField] private int slimeTier2Required = 10; // For Objective 4
    private static int currentSlimeKills = 0;

    [Header("Quest Trackers (Spiders)")]
    [SerializeField] private int spiderRequiredKills = 10; // For Objective 9
    private static int currentSpiderKills = 0;

    [Header("Editor Testing Toggles")]
    [Tooltip("Check this to manually override objectives using the array below for testing weapons/rewards!")]
    [SerializeField] private bool enableTestToggles = false;
    [Tooltip("Elements 0-9 correspond to Objectives 1-10. Check them to force complete them in real-time.")]
    [SerializeField] private bool[] testObjectivesState = new bool[10];

    [HideInInspector] public bool isBookOpen = false;
    [HideInInspector] public bool hasBook = false;

    private List<GameObject> objectiveCrossOutLines = new List<GameObject>();
    private bool isIntroPaperActive = false;

    // Static array saves which objectives are finished across death/reloads during runtime (Size updated to 10)
    private static bool[] completedObjectives = new bool[10];
    private static bool hasShownIntroPaper = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        CloseBook();

        if (scene.name == "wodbeyl" && !hasShownIntroPaper)
        {
            Invoke("ShowIntroMissionPaper", 0.1f);
        }

        // --- LINE 8 COMPLETED: ENTER CAVE DETECTED ---
        if (scene.name == "Cave")
        {
            CompleteObjective(8); // Instantly crosses out Line8 upon entering the Cave scene!
            Debug.Log("Quest Complete: Entered the spooky Cave!");
        }
    }

    void Update()
    {
        // --- LIVE EDITOR TESTING LINK ---
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

    // --- ONE-TIME START PAPER LOGIC ---
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
        if (introMissionPaper != null)
        {
            introMissionPaper.SetActive(false);
        }

        isIntroPaperActive = false;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        Time.timeScale = 1f;
    }


    // --- JOURNAL SYSTEM FUNCTIONS ---
    public void CollectBook()
    {
        hasBook = true;
        CompleteObjective(1);
    }

    // --- PROGRESS TRACKER: SLIME KILLS ---
    public void RegisterSlimeKill(string enemyName)
    {
        if (enableTestToggles) return;

        // Handle Objective 5: Specific Mini-boss check (SlimeGreen)
        if (enemyName.Contains("SlimeGreen") && !completedObjectives[4])
        {
            CompleteObjective(5);
            Debug.Log("Quest Complete: SlimeGreen mini-boss defeated!");
        }

        currentSlimeKills++;
        Debug.Log($"Slime defeated ({enemyName})! Total Slime Progress: {currentSlimeKills}");

        if (currentSlimeKills >= slimeTier1Required && !completedObjectives[2])
        {
            CompleteObjective(3);
        }

        if (currentSlimeKills >= slimeTier2Required && !completedObjectives[3])
        {
            CompleteObjective(4);
        }
    }

    // --- PROGRESS TRACKER: SPIDER KILLS (LINE 9) ---
    public void RegisterSpiderKill(string enemyName)
    {
        if (enableTestToggles) return;

        currentSpiderKills++;
        Debug.Log($"Spider defeated ({enemyName})! Total Spider Progress: {currentSpiderKills}/{spiderRequiredKills}");

        if (currentSpiderKills >= spiderRequiredKills && !completedObjectives[8])
        {
            CompleteObjective(9);
            Debug.Log($"Quest Complete: Cleared out {spiderRequiredKills} Spiders!");
        }
    }

    // --- NEW! PROGRESS TRACKER: SPIDER BOSS KILL (LINE 10) ---
    public void RegisterSpiderBossKill()
    {
        if (enableTestToggles) return;

        if (!completedObjectives[9])
        {
            CompleteObjective(10); // Instantly crosses out Line10!
            Debug.Log("Quest Complete: The Mythic Spider Boss has been vanquished!");
        }
    }

    public void OpenBook()
    {
        bookUI.SetActive(true);
        isBookOpen = true;

        FindAndRefreshLines();

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (SelectionManager.Instance != null)
        {
            SelectionManager.Instance.DisableSelection();
        }
    }

    public void CloseBook()
    {
        book pageScript = bookUI.GetComponentInChildren<book>();
        if (pageScript != null)
        {
            pageScript.ResetState();
        }

        bookUI.SetActive(false);
        isBookOpen = false;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (SelectionManager.Instance != null)
        {
            SelectionManager.Instance.EnableSelection();
        }
    }

    public void CompleteObjective(int objectiveNumber)
    {
        if (enableTestToggles) return;

        int index = objectiveNumber - 1;
        if (index >= 0 && index < completedObjectives.Length)
        {
            completedObjectives[index] = true;
            FindAndRefreshLines();
            Debug.Log($"Book Objective {objectiveNumber} crossed out!");
        }
    }

    private void FindAndRefreshLines()
    {
        Transform[] allChildren = bookUI.GetComponentsInChildren<Transform>(true);
        objectiveCrossOutLines.Clear();

        Dictionary<string, GameObject> foundLines = new Dictionary<string, GameObject>();

        foreach (Transform child in allChildren)
        {
            if (crossOutLineNames.Contains(child.name))
            {
                foundLines[child.name] = child.gameObject;
            }
        }

        foreach (string lineName in crossOutLineNames)
        {
            if (foundLines.ContainsKey(lineName))
            {
                objectiveCrossOutLines.Add(foundLines[lineName]);
            }
        }

        for (int i = 0; i < objectiveCrossOutLines.Count; i++)
        {
            if (objectiveCrossOutLines[i] != null)
            {
                objectiveCrossOutLines[i].SetActive(completedObjectives[i]);
            }
        }

        CheckMasterRewardUnlock();
    }

    private void CheckMasterRewardUnlock()
    {
        bool allDone = true;

        for (int i = 0; i < completedObjectives.Length; i++)
        {
            if (!completedObjectives[i])
            {
                allDone = false;
                break;
            }
        }

        if (BowController.Instance != null)
        {
            BowController.Instance.isDoubleShotUnlocked = allDone;
        }
    }

    public bool IsPageUnlocked(int pageIndex)
    {
        if (bookUI == null) return false;

        book pageScript = bookUI.GetComponentInChildren<book>(true);
        if (pageScript != null && pageScript.unlockedPages != null)
        {
            if (pageIndex >= 0 && pageIndex < pageScript.unlockedPages.Count)
            {
                return pageScript.unlockedPages[pageIndex];
            }
        }
        return false;
    }
}