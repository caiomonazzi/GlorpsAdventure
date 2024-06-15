using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIStats : MonoBehaviour
{
    #region Variables and Events:
    // Cached components
    AIController aiController;
    AICanvas aICanvas;
    SpriteRenderer aiSprite;
    AudioSource audioSource;

    // Event
    public delegate void DeathAction(); // AI Death Event
    public event DeathAction onDeath;

    [HideInInspector] public DamageEffect damageEffect; // Visual damage effect

    [Header("Settings")]
    public DoubleFloat HP = new DoubleFloat(100, 100); // DoubleFloat(currentHP, maxHP)

    #endregion

    #region Methods:
    private void Start()
    {
        aiSprite = GetComponentInChildren<SpriteRenderer>();
        aICanvas = GetComponentInChildren<AICanvas>();
        aiController = GetComponent<AIController>();
        audioSource = GetComponent<AudioSource>();
    }

    // Caused by taking damage
    public void TakingDamage(float damage)
    {
        aiController.isAttacked = true; // Sends AI that it was attacked

        HP.current -= damage; // Reduce health by damage amount
        Debug.Log($"Enemy took damage: {damage}, current HP: {HP.current}"); // Log damage and current HP

        aICanvas.UpdateUI(); // Update AI UI (HP bar)

        AudioManager.Instance.Play(audioSource, AudioManager.Instance.aiDamage, false); // Play damage sound

        StartCoroutine(damageEffect.Damage(aiSprite)); // Start damage effect

        if (HP.current <= 0) // If health is zero or below
        {
            Debug.Log("Enemy is dead.");
            Death();
        }
    }

    void Death()
    {
        if (onDeath != null)
            onDeath(); // Trigger death event

        Debug.Log("Destroying enemy GameObject."); // Log destruction
        Destroy(gameObject); // Destroy the AI GameObject
    }
    #endregion
}
