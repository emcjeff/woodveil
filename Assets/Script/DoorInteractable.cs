using UnityEngine;

public class DoorInteractable : MonoBehaviour
{
    private Animator _animator;
    private bool _isOpen = false;

    // Optional: Add a cooldown to prevent "spam clicking" breaking the animation
    private float lastToggleTime;
    public float toggleCooldown = 0.5f;

    void Awake()
    {
        // Try to find animator on this object or its children
        _animator = GetComponentInChildren<Animator>();

        if (_animator == null)
        {
            Debug.LogError($"No Animator found on {gameObject.name} or its children!");
        }
    }

    public void ToogleDoor()
    {
        // Prevent clicking too fast
        if (Time.time < lastToggleTime + toggleCooldown) return;

        _isOpen = !_isOpen;
        lastToggleTime = Time.time;

        if (_animator != null)
        {
            // This triggers the transition in your Animator Controller
            _animator.SetBool("isOpen", _isOpen);
            Debug.Log("Door " + (_isOpen ? "Opening" : "Closing"));
        }
    }
}