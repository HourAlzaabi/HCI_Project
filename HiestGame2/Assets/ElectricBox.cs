using UnityEngine;
using System.Collections;

public class ElectricBox : MonoBehaviour
{
    public string requiredItem = "Wire Cutters";
    public LaserDoor linkedLaserDoor;
    public bool consumeItem = false;

    [Header("Visuals")]
    public GameObject normalVisual;
    public GameObject tamperedVisual;

    [Header("Effects")]
    public ParticleSystem cutParticles;
    public ParticleSystem cutBurstParticles;


    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip cutSound;
    public AudioClip sparkLoopSound;

    public float sparkMinDelay = 1.5f;
    public float sparkMaxDelay = 4f;

    private bool disabled = false;
    private bool playerInside = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInside = true;
            TryDisable();
        }
    }

    public void TryDisable()
    {
        if (disabled) return;

        if (InventorySystem.Instance.HasItem(requiredItem))
        {
            disabled = true;

            if (consumeItem)
                InventorySystem.Instance.RemoveItem(requiredItem);


            if (linkedLaserDoor != null)
 
                linkedLaserDoor.DisableDoor();
            

                if (normalVisual != null)
                normalVisual.SetActive(false);

            if (tamperedVisual != null)
                tamperedVisual.SetActive(true);

            if (cutParticles != null)
            {
                cutParticles.Play();
                cutBurstParticles.Play();
            }


            if (audioSource != null && cutSound != null)
                audioSource.PlayOneShot(cutSound);

            StartCoroutine(SparkSoundLoop());

            Debug.Log("Electric box disabled.");
        }
        else
        {
            Debug.Log("You need " + requiredItem);
        }
    }

    IEnumerator SparkSoundLoop()
    {
        while (disabled)
        {
            float delay = Random.Range(sparkMinDelay, sparkMaxDelay);
            yield return new WaitForSeconds(delay);

            if (audioSource != null && sparkLoopSound != null)
                audioSource.PlayOneShot(sparkLoopSound, 0.4f);
        }
    }
}