using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StoryIntro : MonoBehaviour
{
    [Header("UI Structure Links")]
    [SerializeField] private Button skipButtonComponent;

    [Header("Story Card Flow")]
    [Tooltip("Drag your 5 hierarchy Card Image GameObjects here sequentially")]
    [SerializeField] private List<GameObject> storyCards = new List<GameObject>();

    [Tooltip("How many seconds each card stays fully visible before the next one starts appearing")]
    [SerializeField] private float timePerCard = 3f;

    [Tooltip("How fast the card transitions from invisible to visible (Higher = Faster)")]
    [SerializeField] private float fadeSpeed = 2.5f;

    private PlayerMovement1 playerMovementScript;
    private MonoBehaviour cameraLookScript;

    private int currentCardIndex = 0;
    private float stateTimer = 0f;
    private bool isStoryFinished = false;
    private bool isInitialized = false;
    private bool isCurrentlyFading = false;

    private List<CanvasGroup> cardCanvasGroups = new List<CanvasGroup>();

    // This tracks if the intro already played during this active gameplay pass
    private static bool hasPlayedThisSession = false;

    // Public utility function so MainMenu can tell the intro to refresh when starting fresh!
    public static void ResetIntroPlaystate()
    {
        hasPlayedThisSession = false;
    }

    void Start()
    {
        // If the player already went through the intro panels on this run, bypass everything instantly!
        if (hasPlayedThisSession)
        {
            Debug.Log("[Story System] Intro sequence already viewed. Bypassing canvas layout rendering.");
            TogglePersistentPlayerControls(true);
            Destroy(gameObject);
            return;
        }

        // Initialize your cards exactly like before
        foreach (GameObject card in storyCards)
        {
            if (card != null)
            {
                CanvasGroup group = card.GetComponent<CanvasGroup>();
                if (group == null)
                {
                    group = card.AddComponent<CanvasGroup>();
                }
                cardCanvasGroups.Add(group);

                group.alpha = 0f;
                card.SetActive(true);
            }
        }
    }

    void Update()
    {
        if (!isInitialized)
        {
            InitializeSequence();
            return;
        }

        if (isStoryFinished || isCurrentlyFading) return;

        stateTimer += Time.unscaledDeltaTime;

        if (stateTimer >= timePerCard)
        {
            stateTimer = 0f;
            ShowNextComicPanel();
        }
    }

    private void InitializeSequence()
    {
        if (storyCards.Count == 0) return;

        isInitialized = true;

        TogglePersistentPlayerControls(false);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (skipButtonComponent != null)
        {
            skipButtonComponent.onClick.RemoveAllListeners();
            skipButtonComponent.onClick.AddListener(FinishStoryAndStartGame);
        }

        ShowNextComicPanel();
    }

    private void ShowNextComicPanel()
    {
        if (isStoryFinished) return;

        if (currentCardIndex < cardCanvasGroups.Count)
        {
            StartCoroutine(FadeInCard(cardCanvasGroups[currentCardIndex]));
            currentCardIndex++;
        }
        else
        {
            FinishStoryAndStartGame();
        }
    }

    private IEnumerator FadeInCard(CanvasGroup group)
    {
        isCurrentlyFading = true;
        float alpha = 0f;

        while (alpha < 1f)
        {
            alpha += Time.unscaledDeltaTime * fadeSpeed;
            if (group != null)
            {
                group.alpha = Mathf.Clamp01(alpha);
            }
            yield return null;
        }

        isCurrentlyFading = false;
    }

    private void TogglePersistentPlayerControls(bool state)
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerMovementScript = player.GetComponent<PlayerMovement1>();
            if (playerMovementScript != null) playerMovementScript.enabled = state;

            cameraLookScript = FindLookScript(player);
            if (cameraLookScript != null) cameraLookScript.enabled = state;
        }
    }

    private MonoBehaviour FindLookScript(GameObject player)
    {
        string[] commonLookNames = { "MouseLook", "PlayerLook", "CameraLook", "CameraController" };
        foreach (string name in commonLookNames)
        {
            System.Type t = System.Type.GetType(name);
            if (t != null)
            {
                MonoBehaviour script = player.GetComponent(t) as MonoBehaviour;
                if (script != null) return script;
            }
        }
        if (Camera.main != null)
        {
            MonoBehaviour[] allCameraScripts = Camera.main.gameObject.GetComponentsInChildren<MonoBehaviour>();
            foreach (MonoBehaviour script in allCameraScripts)
            {
                if (script != null)
                {
                    string typeName = script.GetType().Name;
                    if (typeName == "MouseLook" || typeName == "PlayerLook" || typeName == "CameraLook" || typeName == "CameraController") return script;
                }
            }
        }
        return null;
    }

    private void FinishStoryAndStartGame()
    {
        if (isStoryFinished) return;
        isStoryFinished = true;

        // Remember that it has run successfully for this gameplay session
        hasPlayedThisSession = true;

        StopAllCoroutines();
        TogglePersistentPlayerControls(true);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        Debug.Log("[Intro System] Ending cutscene canvas display wrapper.");
        Destroy(gameObject);
    }
}