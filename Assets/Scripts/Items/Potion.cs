using UnityEngine;

public class Potion : Item
{
    public int potionAmount = 10; // Amount of health to add or subtract
    public bool isPoison = false; // Determines if the potion is poison

    public bool rotationOn = true; // Determines if the potion should rotate
    public float rotationSpeed = 50f; // Speed of the rotation effect
    public bool limitRotation = true; // Restrict rotation to specified angles
    public float maxRotationAngle = 30f; // Maximum rotation angle for restricted rotation
    public bool wobbleOn = false; // Determines if the potion should wobble
    public float wobbleSpeed = 1f; // Speed of the wobble effect
    public float wobbleAmplitude = 0.5f; // Amplitude of the wobble effect
    public AudioClip collectSound; // Sound to play when the potion is collected
    public GameObject collectEffect; // Particle effect to play when the potion is collected
    public float effectDestroyDelay = 1f; // Time in seconds to wait before destroying the collectible effect

    private Vector3 initialPosition;
    private float rotationAngle = 0f;
    private AudioSource audioSource;

    private void Start()
    {
        initialPosition = transform.position;
        audioSource = gameObject.AddComponent<AudioSource>();
    }

    private void Update()
    {
        if (rotationOn)
        {
            RotatePotion();
        }

        if (wobbleOn)
        {
            WobblePotion();
        }
    }

    private void RotatePotion()
    {
        if (limitRotation)
        {
            rotationAngle += rotationSpeed * Time.deltaTime;
            float clampedAngle = Mathf.PingPong(rotationAngle, maxRotationAngle * 2) - maxRotationAngle;
            transform.rotation = Quaternion.Euler(0, 0, clampedAngle);
        }
        else
        {
            transform.Rotate(0, 0, rotationSpeed * Time.deltaTime);
        }
    }

    private void WobblePotion()
    {
        float wobble = Mathf.Sin(Time.time * wobbleSpeed) * wobbleAmplitude;
        transform.position = initialPosition + new Vector3(0, wobble, 0);
    }

    public override void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            ApplyEffect();

            if (collectSound != null)
            {
                audioSource.PlayOneShot(collectSound);
            }

            if (collectEffect != null)
            {
                GameObject effect = Instantiate(collectEffect, transform.position, Quaternion.identity);
                Destroy(effect, effectDestroyDelay);
            }

            Destroy(gameObject);
        }
    }

    private void ApplyEffect()
    {
        PlayerStats playerStats = PlayerStats.Instance;
        if (playerStats != null)
        {
            int finalAmount = isPoison ? -potionAmount : potionAmount;
            if (finalAmount < 0)
            {
                playerStats.TakingDamage(-finalAmount); // Apply the specified damage amount
            }
            else
            {
                playerStats.Heal(finalAmount); // Apply the specified healing amount
            }
        }
        else
        {
            Debug.LogError("PlayerStats instance not found.");
        }
    }
}
