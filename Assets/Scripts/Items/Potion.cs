using UnityEngine;

public class Potion : Item
{
    public int potionAmount = 10; // Amount of health to add or subtract
    public bool isPoison = false; // Determines if the potion is poison
    public bool rotationOn = true; // Determines if the potion should rotate
    public float rotationSpeed = 50f; // Speed of the rotation effect
    public bool limitRotation = true; // Restrict rotation to specified angles
    public float maxRotationAngle = 30f; // Maximum rotation angle for restricted rotation
    public GameObject collectEffect; // Particle effect to play when the potion is collected
    public float effectDestroyDelay = 1f; // Time in seconds to wait before destroying the collectible effect
    private Vector3 initialPosition;
    private float rotationAngle = 0f;


    private void Start()
    {
        initialPosition = transform.position;
    }

    private void Update()
    {
        if (rotationOn)
        {
            RotatePotion();
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

    public override void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            PlayerStats playerStats = PlayerStats.Instance;
            if (playerStats != null)
            {
                int finalAmount = isPoison ? -potionAmount : potionAmount;

                // Check if player can pick up a health potion
                if (!isPoison && playerStats.HP.current >= playerStats.HP.max)
                {
                    Debug.Log("Player is already at max health. Cannot pick up the health potion.");
                    return;
                }
                ApplyEffect();
                PlaySound();

                Destroy(gameObject);
            }
            else
            {
                Debug.LogError("PlayerStats instance not found.");
            }
        }
    }
    private void PlaySound()
    {
        AudioClip soundToPlay = isPoison ? AudioManager.Instance.playerDamage : AudioManager.Instance.pickUpItems;
        AudioManager.Instance.Play(PlayerStats.Instance.audioSource, soundToPlay, false);
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
