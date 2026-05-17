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
    [Tooltip("Type the exact names of your 5 Cross-Out Line GameObjects inside the book pages")]
    [SerializeField] private List<string> crossOutLineNames = new List<string> { "Line1", "Line2", "Line3", "Line4", "Line5" };

    [Header("Quest Trackers")]
    [SerializeField] private int slimesRequired = 10;
    private static int currentSlimeKills = 0; // Static so it survives player scene transitions/reloads

    [Tooltip("How many arrows the player must actively hold in their inventory for Objective 4")]
    [SerializeField] private int arrowsRequired = 100;

    [Header("Editor Testing Toggles")]
    [Tooltip("Check this to manually override objectives using the array below for testing weapons/rewards!")]
    [SerializeField] private bool enableTestToggles = false;
    [Tooltip("Elements 0-4 correspond to Objectives 1-5. Check them to force complete them in real-time.")]
    [SerializeField] private bool[] testObjectivesState = new bool[5];

    [HideInInspector] public bool isBookOpen = false;
    [HideInInspector] public bool hasBook = false;

    private List<GameObject> objectiveCrossOutLines = new List<GameObject>();
    private bool isIntroPaperActive = false;

    // Static array saves which objectives are finished across death/reloads during runtime
    private static bool[] completedObjectives = new bool[5];
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

    // --- 1. THE ONE-TIME START PAPER LOGIC ---
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


    // --- 2. THE ACTUAL BOOK LAYOUT LOGIC ---
    public void CollectBook()
    {
        hasBook = true;
        CompleteObjective(1);
    }

    // --- TRACK SLIME KILLS (Event-driven) ---
    public void RegisterSlimeKill()
    {
        if (enableTestToggles) return; // Skip normal progression calculations if testing
        if (completedObjectives[2]) return;

        currentSlimeKills++;
        Debug.Log($"Slime defeated! Progress: {currentSlimeKills}/{slimesRequired}");

        if (currentSlimeKills >= slimesRequired)
        {
            CompleteObjective(3);
            Debug.Log("Quest Complete: 10 Slimes defeated!");
        }
    }

    // --- DYNAMIC INVENTORY CHECK FOR ARROWS ---
    private void CheckInventoryObjectives()
    {
        if (enableTestToggles) return; // Skip inventory calculations if testing via inspector

        if (InventorySystem.Instance != null)
        {
            int currentArrowsInInventory = InventorySystem.Instance.GetTotalItemCount("ArrowUI");
            Debug.Log($"Checking book quest: Holding {currentArrowsInInventory}/{arrowsRequired} arrows.");

            if (currentArrowsInInventory >= arrowsRequired)
            {
                completedObjectives[3] = true;
            }
            else
            {
                completedObjectives[3] = false;
            }
        }
    }

    public void OpenBook()
    {
        CheckInventoryObjectives();

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
        if (enableTestToggles) return; // Ignore standard execution signals during test loops

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

        // Run checking framework to decide if Bow Controller gains the burst buff
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
            // Connect directly to the bow toggle flag dynamically
            BowController.Instance.isDoubleShotUnlocked = allDone;
        }
    }
}