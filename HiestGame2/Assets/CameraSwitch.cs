using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

[RequireComponent(typeof(Collider2D), typeof(AudioSource))]
public class CameraSwitch : MonoBehaviour
{
    [Header("Who can use the switch")]
    public string activatorTag = "Player";

    [Header("Hold key to DISABLE")]
    public Key interactKey = Key.E;

    [Header("References")]
    public GameObject cameraSpriteObject; // little camera sprite GO
    public GameObject cameraZoneObject;   // zone GO (trigger)

    [Header("Switch Sprite (ON = normal, OFF = flipped Y)")]
    public SpriteRenderer switchSpriteRenderer;
    public Sprite switchOnSprite; // Only need ON sprite now

    [Header("Camera Flicker")]
    public float flickerInterval = 0.12f;

    [Header("Sound Effects")]
    public AudioClip switchOnClip;
    public AudioClip switchOffClip;
    [Range(0f, 1f)] public float switchVolume = 1f;

    [Header("Start State")]
    public bool startOn = true;

    bool playerInZone = false;
    bool isOn = true;

    AudioSource audioSource;
    SpriteRenderer cameraSR;
    Coroutine flickerRoutine;

    void Reset()
    {
        GetComponent<Collider2D>().isTrigger = true;
    }

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.playOnAwake = false;

        if (cameraSpriteObject != null)
            cameraSR = cameraSpriteObject.GetComponent<SpriteRenderer>();
    }

    void Start()
    {
        ApplyState(startOn, playSound: false);
    }

    void Update()
    {
        bool shouldBeOn = true;

        if (playerInZone)
        {
            var kb = Keyboard.current;
            if (kb != null)
            {
                bool holding = kb[interactKey].isPressed;
                shouldBeOn = !holding;
            }
        }

        if (shouldBeOn != isOn)
            ApplyState(shouldBeOn, playSound: true);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag(activatorTag)) return;
        playerInZone = true;
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag(activatorTag)) return;
        playerInZone = false;

        if (!isOn)
            ApplyState(true, playSound: true);
    }

    void ApplyState(bool on, bool playSound)
    {
        isOn = on;

        // Zone active only when ON
        if (cameraZoneObject != null)
            cameraZoneObject.SetActive(on);

        // Camera sprite visible only when ON
        if (cameraSpriteObject != null)
        {
            cameraSpriteObject.SetActive(on);

            if (on) StartFlicker();
            else StopFlicker();
        }

        // Switch sprite logic (FLIP instead of swapping sprite)
        if (switchSpriteRenderer != null)
        {
            if (switchOnSprite != null)
                switchSpriteRenderer.sprite = switchOnSprite;

            // ON = normal
            // OFF = flipped vertically (Y axis)
            switchSpriteRenderer.flipY = !on;
        }

        // Sounds
        if (playSound)
        {
            if (on)
            {
                if (switchOnClip != null)
                    audioSource.PlayOneShot(switchOnClip, switchVolume);
            }
            else
            {
                if (switchOffClip != null)
                    audioSource.PlayOneShot(switchOffClip, switchVolume);
            }
        }
    }

    void StartFlicker()
    {
        StopFlicker();

        if (cameraSR == null && cameraSpriteObject != null)
            cameraSR = cameraSpriteObject.GetComponent<SpriteRenderer>();

        if (cameraSR == null) return;

        flickerRoutine = StartCoroutine(Flicker());
    }

    void StopFlicker()
    {
        if (flickerRoutine != null)
        {
            StopCoroutine(flickerRoutine);
            flickerRoutine = null;
        }

        if (cameraSR != null)
            cameraSR.enabled = true;
    }

    IEnumerator Flicker()
    {
        while (true)
        {
            cameraSR.enabled = !cameraSR.enabled;
            yield return new WaitForSeconds(flickerInterval);
        }
    }
}