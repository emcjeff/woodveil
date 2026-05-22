using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class LoadingScreenOverlay : MonoBehaviour
{
    private static LoadingScreenOverlay instance;
    public static LoadingScreenOverlay Instance => instance;

    [Header("References")]
    [Tooltip("Drag the full-screen black image panel child object here.")]
    [SerializeField] private GameObject visualPanel;

    [Header("Settings")]
    [Tooltip("Minimum time the loading screen stays visible (in seconds) before hiding.")]
    [SerializeField] private float loadingScreenDuration = 2.5f;

    private Coroutine hideRoutine;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);

        if (visualPanel != null) visualPanel.SetActive(false);
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "wodbeyl")
        {
            Debug.Log("[LoadingScreenOverlay] 'wodbeyl' level loaded. Processing safe transition handshakes.");

            // Clean up old coroutines instantly on load sequence completion
            if (hideRoutine != null)
            {
                StopCoroutine(hideRoutine);
            }
            hideRoutine = StartCoroutine(WaitAndHideRoutine());
        }
    }

    public void Show()
    {
        if (hideRoutine != null)
        {
            StopCoroutine(hideRoutine);
            hideRoutine = null;
        }

        if (visualPanel != null)
        {
            visualPanel.SetActive(true);
        }
    }

    public void HideWithDelay()
    {
        if (hideRoutine != null)
        {
            StopCoroutine(hideRoutine);
        }
        hideRoutine = StartCoroutine(WaitAndHideRoutine());
    }

    public void InstantHide()
    {
        if (hideRoutine != null)
        {
            StopCoroutine(hideRoutine);
            hideRoutine = null;
        }

        if (visualPanel != null)
        {
            visualPanel.SetActive(false);
        }
    }

    private IEnumerator WaitAndHideRoutine()
    {
        // 1. First clear structural state tracks instantly so Awake updates read it correctly
        MainMenu.isRetrying = false;
        MainMenu.isLongReturning = false;

        // 2. Wait out the visual presentation layout delay safely
        yield return new WaitForSecondsRealtime(loadingScreenDuration);

        if (visualPanel != null)
        {
            visualPanel.SetActive(false);
        }

        Debug.Log("[LoadingScreenOverlay] Fade-out complete. Control configurations released.");
        hideRoutine = null;
    }
}