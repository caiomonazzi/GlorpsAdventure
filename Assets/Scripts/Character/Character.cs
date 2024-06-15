using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

#region Rigibody2DParameters struct:
[System.Serializable]
public struct Rigidbody2DParameters
{
    public RigidbodyType2D bodyType;
    public PhysicsMaterial2D material;
    public bool simulated;
    public bool useAutoMass;
    public float mass;
    public float linearDrag;
    public float angularDrag;
    public float gravityScale;
    public CollisionDetectionMode2D collisionDetectionMode;
    public RigidbodyInterpolation2D interpolation;
    public RigidbodyConstraints2D constraints;
    public bool isKinematic;
}
#endregion


public class Character : MonoBehaviour
{
    
    #region Variables

    [HideInInspector] private WeaponController weaponController;
    [HideInInspector] public AttackController attackController;

    // Components
    PlayerStats playerStats;
    private Animator animator;
    public GameObject playerSprite;
    public SpriteRenderer playerSpriteRenderer;

    [Header("Movement")]
    private bool isClimbing = false;
    private bool isOnRope = false;
    public float climbSpeed = 3f;
    private float verticalMove = 0f;

    [SerializeField] public float runSpeed = 20f;
    [SerializeField] public float walkSpeed = 10f;
    public float originalRunSpeed;
    public float originalWalkSpeed;

    [SerializeField] private float staminaTime = 100f;
    [SerializeField] private bool doubleJump = true;

    [Header("Transforms")]
    private int jumps = 0;
    private int maxJumps = 1;
    private float horizontalMove = 0f;

    public bool isFacingRight = true;
    public bool isRunning = false;
    public bool isJumping = false;
    public bool isCrouching = false;
    public bool isAttacking = false;
    public bool IsSwinging = false;

    private int currentDirection = 0;

    private float lastRunKeyPressTime = 0f;
    private bool pressedRunFirstTime = false;
    private float staminaRunningTime = 0f;
    private const float doubleKeyPressDelay = .25f;

    [SerializeField] private float jumpForce = 350f; // Amount of force added when the player jumps.
    [Range(0, 1)][SerializeField] private float crouchSpeed = .36f; // Amount of maxSpeed applied to crouching movement. 1 = 100%
    [Range(0, .3f)][SerializeField] private float movementSmoothing = .1f; // How much to smooth out the movement

    [SerializeField] private bool airControl = false; // Whether or not a player can steer while jumping;
    [SerializeField] private LayerMask whatIsGround; // A mask determining what is ground to the character
    [SerializeField] private Transform groundCheck; // A position marking where to check if the player is grounded.

    [SerializeField] private Collider2D crouchDisableCollider; // A collider that will be disabled when crouching

    private Rigidbody2DParameters originalRigidbody2DParameters;
    private Rigidbody2D m_Rigidbody2D;

    private float delayGroundCheck = 0.25f;
    private const float groundedRadius = .2f; // Radius of the overlap circle to determine if grounded
    private bool grounded; // Whether or not the player is grounded.

    private Vector3 velocity = Vector3.zero;
    private float timeBeforeGroundCheck = 0f;


    private Color originalColor; // To store the original color of the sprite 
    [SerializeField] private Color invincibleColor = Color.yellow;

    #endregion


    #region Events
    [Header("Events")]
    [Space]
    public UnityEvent OnLandEvent;
    #endregion


    #region Unity Methods
    private void Start()
    {
        playerStats = PlayerStats.Instance;
        m_Rigidbody2D = GetComponent<Rigidbody2D>();
        animator = GetComponentInChildren<Animator>();

        PlayerStats.Instance.audioSource = GetComponent<AudioSource>();


        originalColor = playerSpriteRenderer.color; // Store the original color of the sprite

        SaveManager.Save(); // Save level state


    }

    private void Awake()
    {

        attackController = GetComponent<AttackController>();
        weaponController = GetComponent<WeaponController>();

        animator = GetComponent<Animator>();

        m_Rigidbody2D = GetComponent<Rigidbody2D>();
        StoreRigidbody2DParameters();
        originalRunSpeed = runSpeed;
        originalWalkSpeed = walkSpeed;

        if (OnLandEvent == null)
            OnLandEvent = new UnityEvent();
        if (doubleJump) maxJumps = 2;

    }

    private void Update()
    {
        Inputs(); // Check inputs
        if (!grounded)
        {
            timeBeforeGroundCheck -= Time.deltaTime;
        }
        // Check for ladder climbing input
        if (isClimbing)
        {
            if (Input.GetKey(KeyCode.W))
            {
                verticalMove = 1f;
            }
            else if (Input.GetKey(KeyCode.S))
            {
                verticalMove = -1f;
            }
            else
            {
                verticalMove = 0f;
            }
        }

    }

    private void FixedUpdate()
    {
        // If attacking or dead, do not move.
        if (attackController.isAttacking || playerStats.isDead)
        {
            Movement(0);
            return;
        }
        else
        {
            Movement(horizontalMove * Time.fixedDeltaTime);
        }
        if (timeBeforeGroundCheck > 0f) return;

        bool wasGrounded = grounded;
        grounded = false;

        Collider2D[] colliders = Physics2D.OverlapCircleAll(groundCheck.position, groundedRadius, whatIsGround);
        for (int i = 0; i < colliders.Length; i++)
        {
            if (colliders[i].gameObject != gameObject)
            {
                grounded = true;
                if (!wasGrounded)
                    OnLandEvent.Invoke();
            }
        }

        if (isClimbing)
        {
            Climb(verticalMove * Time.fixedDeltaTime);
        }

        // Update animator with grounded state
        animator.SetBool("isJumping", !grounded);
        animator.SetBool("isCrouching", isCrouching);
    }
    #endregion


    #region Input Handlers
    // Inputs method
    void Inputs()
    {
        if (InputManager.Pause) // If player press pause button
        {
            UIManager.Instance.Pause(); // Show UI pause screen
        }

        horizontalMove = Input.GetAxisRaw("Horizontal");
        if (InputManager.MoveLeft || InputManager.MoveRight)
        {
            if (pressedRunFirstTime)
            {
                // A key was already pressed, checking for double press
                if (Time.time - lastRunKeyPressTime <= doubleKeyPressDelay)
                {
                    // Key was pressed twice in the desired delay
                    pressedRunFirstTime = false;
                    if (!isRunning)
                    {
                        isRunning = true;
                        staminaRunningTime = Time.time;

                        animator.SetBool("isRunning", true);
                    }
                }
            }
            else
            {
                // Key is pressed for the first time
                pressedRunFirstTime = true;
            }

            // Updating last time that the key was pressed
            lastRunKeyPressTime = Time.time;
        }
        else if (isRunning && (InputManager.MoveLeft || InputManager.MoveRight))
        {
            // A key was released while running, cancelling it
            isRunning = false;
            animator.SetBool("isRunning", false);
        }

        // Waiting for double key press but reached the delay, restarting the double press logic
        if (pressedRunFirstTime && Time.time - lastRunKeyPressTime > doubleKeyPressDelay)
        {
            pressedRunFirstTime = false;
        }

        // Checking to see if the player stamina timed out while running
        if (isRunning && ((Time.time - staminaRunningTime) > staminaTime))
        {
            // Stamina timeout, player is not running anymore
            isRunning = false;
            animator.SetBool("isRunning", false);
        }

        // Move the char at the selected speed
        float speed = isRunning ? runSpeed : walkSpeed;
        horizontalMove = horizontalMove * speed;

        // Set isWalking and isRunning based on speed
        if (Mathf.Abs(horizontalMove) > 0 && Mathf.Abs(speed) < runSpeed)
        {
            animator.SetBool("isWalking", true);
            animator.SetBool("isRunning", false);
        }
        else if (Mathf.Abs(horizontalMove) >= runSpeed)
        {
            animator.SetBool("isWalking", false);
            animator.SetBool("isRunning", true);
        }
        else
        {
            animator.SetBool("isWalking", false);
            animator.SetBool("isRunning", false);
        }

        // Check and change attackpoint direction if needed
        HandleFlip(horizontalMove);

        // if pause disable and game
        if (!UIManager.Instance.isPause && GameManager.Instance.isGame)
        {
            if (InputManager.Attack && Time.time >= attackController.nextMeleeTime) // If player press attack button
            {
                Attack();
                attackController.nextMeleeTime = Time.time + attackController.meleeCooldown;
            }


            if (!isClimbing)
            {
                if (InputManager.Jump) // If player press jump button
                {
                    Jump(true);
                }

                if (InputManager.Crouch) // If player press crouch button
                {
                    Crouch(true);
                }
                else
                {
                    Crouch(false);
                }
            }
        }
    }

    #endregion


    #region Public Methods

    public void RestoreRigidbody2DParameters()
    {
        m_Rigidbody2D.bodyType = originalRigidbody2DParameters.bodyType;
        m_Rigidbody2D.sharedMaterial = originalRigidbody2DParameters.material;
        m_Rigidbody2D.simulated = originalRigidbody2DParameters.simulated;
        m_Rigidbody2D.useAutoMass = originalRigidbody2DParameters.useAutoMass;
        m_Rigidbody2D.mass = originalRigidbody2DParameters.mass;
        m_Rigidbody2D.drag = originalRigidbody2DParameters.linearDrag;
        m_Rigidbody2D.angularDrag = originalRigidbody2DParameters.angularDrag;
        m_Rigidbody2D.gravityScale = originalRigidbody2DParameters.gravityScale;
        m_Rigidbody2D.collisionDetectionMode = originalRigidbody2DParameters.collisionDetectionMode;
        m_Rigidbody2D.interpolation = originalRigidbody2DParameters.interpolation;
        m_Rigidbody2D.constraints = originalRigidbody2DParameters.constraints;
        m_Rigidbody2D.isKinematic = originalRigidbody2DParameters.isKinematic;
    }

    public void Jump(bool j)
    {
        if (playerStats.isDead) return;

        // Ensure maxJumps is set according to doubleJump setting
        maxJumps = doubleJump ? 2 : 1;

        // If attempting to jump and haven't reached max jumps...
        if (j && jumps < maxJumps)
        {
            jumps++;

            // Add vertical force if it's not the first jump.
            if (jumps > 1)
            {
                Jumping();
            }
            else
            {
                animator.SetBool("isJumping", true);
                animator.Play("Jump");
            }
        }
        // If stopping the jump and currently jumping.
        else if (!j && isJumping)
        {
            jumps = 0;
            isJumping = false; // Ensure this is set to false
            animator.SetBool("isJumping", false);
        }

        isJumping = j;

        // Animator handles animations based on jump count.
        animator.SetInteger("Jumps", jumps);
    }

    public void Jumping()
    {
        grounded = false;
        // Add a vertical force to the player.
        m_Rigidbody2D.AddForce(new Vector2(0f, jumpForce));
        timeBeforeGroundCheck = delayGroundCheck;
    }

    public void Movement(float move)
    {
        if (playerStats.isDead) return;

        // Only control the player if grounded or airControl is turned on
        if (grounded || airControl)
        {
            HandleMovement(move);
        }

        // Determine animation based on movement speed
        if (Mathf.Abs(move) > 0.1f)
        {
            // If the absolute value of move is greater than 0.1, it means the player is moving
            // Use the absolute value because we only care about speed, not direction
            if (Mathf.Abs(move) > 1f)
            {
                // If the speed is greater than 1, play the run animation
                animator.Play("Run");
            }
            else
            {
                // If the speed is between 0.1 and 1, play the walk animation
                animator.Play("Walk");
            }
        }
        else
        {
            // If the player is not moving, play the idle animation
            animator.Play("Idle");
        }

        // If the player should jump...
        if (grounded && isJumping)
        {
            Jumping();
        }
    }

    // Crouch method
    public void Crouch(bool c)
    {
        isCrouching = c;
        animator.SetBool("isCrouching", isCrouching);

        if (isCrouching)
        {
            if (crouchDisableCollider != null)
                crouchDisableCollider.enabled = false;
        }
        else
        {
            if (crouchDisableCollider != null)
                crouchDisableCollider.enabled = true;
        }
    }

    public void Attack()
    {
        if (weaponController.hasWeapon && weaponController.currentWeapon != null && weaponController.shotsRemaining > 0)
        {
            weaponController.HandleAttack();
        }
        else
        {
            // Melee attack
            Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackController.attackPoint.position, attackController.range, attackController.whatIsEnemy);
            foreach (Collider2D enemy in hitEnemies)
            {
                enemy.GetComponent<AIStats>()?.TakingDamage(attackController.meleeDamage);
            }
        }

        animator.SetBool("isAttacking", true);
        StartCoroutine(ResetAttackState());
    }

    public void TakeDamage(int damageAmount = 1)
    {
        if (!playerStats.isInvincible)
        {
            playerStats.TakingDamage(damageAmount);
        }
    }

    private IEnumerator ResetAttackState()
    {
        yield return new WaitForSeconds(0.1f);
        animator.SetBool("isAttacking", false);
    }

    public void Flip()
    {
        if (playerStats.isDead) return;

        // Switch the way the player is labelled as facing.
        isFacingRight = !isFacingRight;

        // Multiply the player's x local scale by -1.
        Vector3 theScale = transform.localScale;
        theScale.x *= -1;
        transform.localScale = theScale;

        // Flip the attackPoint
        if (attackController.attackPoint != null)
        {
            // Multiply the attackPoint's x local scale by -1.
            Vector3 attackScale = attackController.attackPoint.localScale;
            attackScale.x *= -1;
            attackController.attackPoint.localScale = attackScale;
        }

    }

    public Rigidbody2DParameters GetOriginalRigidbody2DParameters()
    {
        return originalRigidbody2DParameters;
    }

    public void OnLanding()
    {
        // Restore jumps when touching the ground.
        Jump(false);
        isJumping = false; // Ensure jumping state is reset
        animator.SetBool("isJumping", isJumping); // Ensure animator stops jump animation
        // Trigger landing particle system
    }

    public void ActivateInvincibility(float duration)
    {
        StartCoroutine(InvincibilityCoroutine(duration));
    }

    public void StartClimbing()
    {
        isClimbing = true;
        m_Rigidbody2D.gravityScale = 0; // Disable gravity while climbing
        m_Rigidbody2D.velocity = new Vector2(m_Rigidbody2D.velocity.x, 0); // Stop vertical movement
    }

    public void StopClimbing()
    {
        isClimbing = false;
        m_Rigidbody2D.gravityScale = 1; // Re-enable gravity
    }
    #endregion


    #region Private Methods
    private void StoreRigidbody2DParameters()
    {
        originalRigidbody2DParameters.bodyType = m_Rigidbody2D.bodyType;
        originalRigidbody2DParameters.material = m_Rigidbody2D.sharedMaterial;
        originalRigidbody2DParameters.simulated = m_Rigidbody2D.simulated;
        originalRigidbody2DParameters.useAutoMass = m_Rigidbody2D.useAutoMass;
        originalRigidbody2DParameters.mass = m_Rigidbody2D.mass;
        originalRigidbody2DParameters.linearDrag = m_Rigidbody2D.drag;
        originalRigidbody2DParameters.angularDrag = m_Rigidbody2D.angularDrag;
        originalRigidbody2DParameters.gravityScale = m_Rigidbody2D.gravityScale;
        originalRigidbody2DParameters.collisionDetectionMode = m_Rigidbody2D.collisionDetectionMode;
        originalRigidbody2DParameters.interpolation = m_Rigidbody2D.interpolation;
        originalRigidbody2DParameters.constraints = m_Rigidbody2D.constraints;
        originalRigidbody2DParameters.isKinematic = m_Rigidbody2D.isKinematic;
    }

    private void HandleMovement(float move)
    {
        if (isCrouching)
        {
            move *= crouchSpeed;

            if (crouchDisableCollider != null)
                crouchDisableCollider.enabled = false;
        }
        else
        {
            if (crouchDisableCollider != null)
                crouchDisableCollider.enabled = true;
        }

        Vector3 targetVelocity = new Vector2(move * 10f, m_Rigidbody2D.velocity.y);
        m_Rigidbody2D.velocity = Vector3.SmoothDamp(m_Rigidbody2D.velocity, targetVelocity, ref velocity, movementSmoothing);
    }

    private void HandleFlip(float move)
    {
        if (move > 0 && !isFacingRight)
        {
            Flip();
        }
        else if (move < 0 && isFacingRight)
        {
            Flip();
        }
    }

    private void PlayParticleSystem(ParticleSystem particleSystem)
    {
        if (particleSystem != null)
        {
            particleSystem.Stop();
            particleSystem.Play();
        }
    }


    private void Climb(float move)
    {
        Vector3 targetVelocity = new Vector2(m_Rigidbody2D.velocity.x, move * climbSpeed);
        m_Rigidbody2D.velocity = Vector3.SmoothDamp(m_Rigidbody2D.velocity, targetVelocity, ref velocity, movementSmoothing);
    }

    private IEnumerator InvincibilityCoroutine(float duration)
    {
        playerStats.isInvincible = true;
        Color originalColor = playerSpriteRenderer.color;
        playerSpriteRenderer.color = invincibleColor; // Change to invincible color

        yield return new WaitForSeconds(duration);

        playerSpriteRenderer.color = originalColor; // Revert to original color
        playerStats.isInvincible = false;
        // Optionally stop the invincibility effect here
    }
    #endregion
}
