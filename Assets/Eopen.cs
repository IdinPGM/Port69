using UnityEngine;
using UnityEngine.UI; // Required for Slider

public class ChestInteraction : MonoBehaviour
{
    [SerializeField] private AudioClip chestOpenSound; // Assign sound in Inspector
    [SerializeField] private Slider progressSlider; // Assign Slider in Inspector
    private AudioSource audioSource;
    private bool isPlayerInTrigger = false;
    private float holdTime = 0f;
    private bool isSoundPlayed = false;
    private const float requiredHoldTime = 2f; // 2 seconds

    void Start()
    {
        // Get or add AudioSource component
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        audioSource.clip = chestOpenSound;
        audioSource.playOnAwake = false;

        // Initialize Slider
        if (progressSlider != null)
        {
            progressSlider.minValue = 0f;
            progressSlider.maxValue = requiredHoldTime;
            progressSlider.value = 0f;
            progressSlider.gameObject.SetActive(false); // Hide Slider initially
        }
    }

    void Update()
    {
        if (isPlayerInTrigger && !isSoundPlayed)
        {
            if (Input.GetKey(KeyCode.E))
            {
                holdTime += Time.deltaTime;
                UpdateProgressBar();
                if (holdTime >= requiredHoldTime)
                {
                    PlayChestSound();
                    isSoundPlayed = true; // Prevent repeated plays
                    HideProgressBar();
                }
            }
            else
            {
                holdTime = 0f; // Reset if E is released
                UpdateProgressBar();
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player")) // Ensure GameObject has "Player" tag
        {
            isPlayerInTrigger = true;
            if (progressSlider != null && !isSoundPlayed)
            {
                progressSlider.gameObject.SetActive(true); // Show Slider when player enters
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInTrigger = false;
            holdTime = 0f; // Reset timer
            HideProgressBar();
        }
    }

    void PlayChestSound()
    {
        if (chestOpenSound != null)
        {
            audioSource.Play();
            Debug.Log("Chest opened!");
        }
    }

    void UpdateProgressBar()
    {
        if (progressSlider != null)
        {
            progressSlider.value = holdTime; // Update Slider value based on hold time
        }
    }

    void HideProgressBar()
    {
        if (progressSlider != null)
        {
            progressSlider.value = 0f;
            progressSlider.gameObject.SetActive(false); // Hide Slider
        }
    }

    // Public method to check if chest is opened
    public bool IsChestOpened()
    {
        return isSoundPlayed;
    }
}