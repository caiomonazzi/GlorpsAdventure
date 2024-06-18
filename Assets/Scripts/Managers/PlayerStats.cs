using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerStats : MonoBehaviour
{
    #region Variables:
    public static PlayerStats Instance; // Singleton

    [HideInInspector] public AudioSource audioSource;
    [HideInInspector] public DamageEffect damageEffect; // Damage effect

    [Header("Variables")]
    public DoubleInt HP = new DoubleInt(3, 3);

    public Dictionary<int, bool> doorKeys = new Dictionary<int, bool>();

    [Header("Graphics")]
    [HideInInspector] public SpriteRenderer playerSprite; // Player sprite

    [Header("Parameters")]
    public float timeToDamage; // Time for pause between AI damage
    bool isDamaged;

    [Header("SpaceShip Pieces")]
    public List<SpaceShipPiece> collectedPieces = new List<SpaceShipPiece>();

    [Header("Health Controller")]
    [HideInInspector] public Character character;
    [HideInInspector] public bool isDead = false;
    [HideInInspector] public bool isHurting = false;

    private Animator animator;

    [Header("Weapon Controller")]
    public WeaponController weaponController;
    public bool hasWeapon;
    public int shotsRemaining;


    // Delegate to determine when to update the life bar.
    public delegate void TakeHealth(int amount);
    public TakeHealth HealthEvent;

    // Delegate to notify when the character dies.
    public delegate void CharacterDeath();
    public CharacterDeath OnDeath;

    // Delegate to notify when the character takes damage.
    public delegate void DamageTaken();
    public event DamageTaken OnDamageTaken;
    #endregion

    #region Private Methods:
    private void Awake()
    {
        // Singleton
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        weaponController = FindFirstObjectByType<WeaponController>();
        if (weaponController != null)
        {
            hasWeapon = weaponController.hasWeapon;
            shotsRemaining = weaponController.shotsRemaining;
        }

        if (ScenesManager.Instance.continueGame)
            SaveManager.Load();

        animator = GetComponentInChildren<Animator>();
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        InitializePlayerComponents();
    }

    private void InitializePlayerComponents()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            character = player.GetComponent<Character>();
            weaponController = player.GetComponent<WeaponController>();

            playerSprite = player.GetComponent<SpriteRenderer>();
            audioSource = player.GetComponent<AudioSource>();
            animator = player.GetComponentInChildren<Animator>();



            if (weaponController != null)
            {
                hasWeapon = weaponController.hasWeapon;
                shotsRemaining = weaponController.shotsRemaining;
            }

            Debug.Log("Player components initialized.");
        }
        else
        {
            Debug.LogWarning("Player not found in the scene.");
        }
    }
    #endregion

    #region Public Methods:
    // Taking damage method
    public void TakingDamage(int damageAmount = 1) // Default damage amount to 1
    {
        Debug.Log("TakingDamage called. isDamaged: " + isDamaged + ", character.isInvincible: ");
        if (!isDamaged)  // if player isn't damaged and not invincible
        {
            isDamaged = true; // block damage
            StartCoroutine(timeDamage()); // set timer to next damage

            HP.current -= damageAmount; // Apply the specified damage amount
            Debug.Log("Damage applied: " + damageAmount + ". Current HP: " + HP.current);

            UIManager.Instance.UpdateUI(); // Update UI
            StartCoroutine(damageEffect.Damage(playerSprite)); // Damage effect

            AudioManager.Instance.Play(audioSource, AudioManager.Instance.playerDamage, false); // play damage sound

            if (HP.current <= 0) // If hp < 0
            {
                Death(); // Lose 
            }
        }
    }

    public void Heal(int healAmount)
    {
        HP.current += healAmount;
        if (HP.current > HP.max)
        {
            HP.current = HP.max;
        }
        UIManager.Instance.UpdateUI();
    }

    IEnumerator timeDamage()
    {

        yield return new WaitForSeconds(timeToDamage); // Wait timeToDamage
        isDamaged = false; // can damage again
    }

    public void SetHasWeapon(bool value)
    {
        hasWeapon = value;
    }

    public void Hurt(bool h)
    {
        isHurting = h;

        if (h) animator.Play("Hurt");

        animator.SetBool("IsHurting", isHurting);
    }

    public void Death()
    {
        GameManager.Instance.GameOver(); // Game over in GameManager
        isDead = true;
        animator.SetBool("IsDead", isDead);
    }

    public void CollectPiece(SpaceShipPiece piece)
    {
        if (piece != null && !collectedPieces.Contains(piece))
        {
            collectedPieces.Add(piece);
            if (UIManager.Instance != null)
            {
                UIManager.Instance.UpdateSpaceShipPiecesUI();
            }
        }
    }
    #endregion
}