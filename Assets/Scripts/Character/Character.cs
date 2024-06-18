using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.TextCore.Text;

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

[System.Serializable]
public enum CharacterState
{
    Idle,
    Walking,
    Running,
    Jumping,
    Crouching,
    Attacking,
    Shooting,
    Climbing,
    Falling
}

public class Character : MonoBehaviour
{
    #region Variables
    [HideInInspector] private WeaponController weaponController;
    [HideInInspector] public AttackController attackController;
    [SerializeField] private float fallDamageMultiplier = 1.0f;
    private float lastYPosition; // Track previous position for fall detection
    private float highestYPosition; // Track the highest position during a jump or fall
    private float fallStartTime; // Track the start time of the fall

    private PlayerStats playerStats;
    [SerializeField] public CharacterState currentState;
    private Animator animator;
    public GameObject playerSprite;
    public SpriteRenderer playerSpriteRenderer;

    [Header("Movement")]
    [SerializeField] public float runSpeed = 20f;
    [SerializeField] public float walkSpeed = 10f;
    public float climbSpeed = 3f;
    private float verticalMove = 0f;
    [HideInInspector] public float originalRunSpeed;
    [HideInInspector] public float originalWalkSpeed;

    [SerializeField] private float staminaTime = 100f;
    [SerializeField] private bool doubleJump = true;

    [Header("Transforms")]
    private int jumps = 0;
    private int maxJumps = 1;
    private float horizontalMove = 0f;

    [HideInInspector] public bool isFacingRight = true;
    private int currentDirection = 0;
    public bool isWalking = false;
    public bool isRunning = false;
    public bool isJumping = false;
    public bool isCrouching = false;
    public bool isClimbing = false;
    public bool isAttacking = false;
    public bool isShooting = false;
    public bool isFalling = false;

    private float lastRunKeyPressTime = 0f;
    private bool pressedRunFirstTime = false;
    private float staminaRunningTime = 0f;
    private const float doubleKeyPressDelay = .25f;

    [SerializeField] private float jumpForce = 350f;
    [Range(0, 1)][SerializeField] private float crouchSpeed = .36f;
    [Range(0, .3f)][SerializeField] private float movementSmoothing = .1f;

    [SerializeField] private bool airControl = false;
    [SerializeField] private LayerMask whatIsGround;
    [SerializeField] private Transform groundCheck;

    [SerializeField] private Collider2D crouchDisableCollider;

    private Rigidbody2DParameters originalRigidbody2DParameters;
    private Rigidbody2D m_Rigidbody2D;

    private float delayGroundCheck = 0.25f;
    private const float groundedRadius = .2f;
    private bool grounded;

    private Vector3 velocity = Vector3.zero;
    private float timeBeforeGroundCheck = 0f;

    private Color originalColor;

    private Camera cam;

    #endregion


    #region Events
    [Header("Events")]
    [Space]
    public UnityEvent OnLandEvent;
    #endregion


    #region Unity Methods
    private void Start()
    {
        InitializeComponents();
        lastYPosition = transform.position.y; // Initialize last known Y position
        highestYPosition = transform.position.y; // Initialize highest known Y position
        SaveManager.Save(); // Save level state
    }

    private void InitializeComponents()
    {
        playerStats = PlayerStats.Instance;
        m_Rigidbody2D = GetComponent<Rigidbody2D>();
        animator = GetComponentInChildren<Animator>();
        PlayerStats.Instance.audioSource = GetComponent<AudioSource>();
        originalColor = playerSpriteRenderer.color; // Store the original color of the sprite
        StoreRigidbody2DParameters();
        originalRunSpeed = runSpeed;
        originalWalkSpeed = walkSpeed;
        if (OnLandEvent == null) OnLandEvent = new UnityEvent();
        if (doubleJump) maxJumps = 2;
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
        HandleInputs();
        if (!grounded)
        {
            timeBeforeGroundCheck -= Time.deltaTime;
        }

        if (isClimbing)
        {
            HandleClimbingInput();
        }

        UpdateState();
    }

    private void FixedUpdate()
    {

        if (attackController.isAttacking || playerStats.isDead)
        {
            Movement(0);
            return;
        }

        Movement(horizontalMove * Time.fixedDeltaTime);
        if (timeBeforeGroundCheck > 0f) return;

        CheckGroundedStatus();
        CheckForFallDamage();

        if (isClimbing)
        {
            Climb(verticalMove * Time.fixedDeltaTime);
        }

        // Update animator with grounded state
        animator.SetBool("isJumping", !grounded);
        animator.SetBool("isCrouching", isCrouching);
    }

    private void CheckForFallDamage()
    {
        // Thresholds
        float fallSpeedThreshold = -5f; // Speed at which fall damage starts
        float minimumFallDistance = 3f; // Minimum distance for fall damage

        if (!grounded && m_Rigidbody2D.velocity.y < fallSpeedThreshold)
        {
            // Update the highest Y position reached during the fall
            if (transform.position.y > highestYPosition)
            {
                highestYPosition = transform.position.y;
            }

            if (!isClimbing && !isJumping)
            {
                if (!isFalling)
                {
                    isFalling = true; // Character is falling
                    fallStartTime = Time.time; // Record the start time of the fall
                    Debug.Log("Character started falling at time: " + fallStartTime);
                }
            }
        }
        else if (grounded && isFalling)
        {
            // Calculate fall distance
            float fallDistance = highestYPosition - transform.position.y;
            Debug.Log("Fall Distance: " + fallDistance);

            // Calculate fall duration
            float fallDuration = Time.time - fallStartTime;
            Debug.Log("Fall Duration: " + fallDuration);

            // Only apply fall damage if fall distance exceeds minimum threshold
            if (fallDistance > minimumFallDistance)
            {
                // Apply fall damage based on duration and distance
                float fallDamage = (fallDistance - minimumFallDistance) * fallDamageMultiplier;
                if (fallDamage > 0)
                {
                    ApplyFallDamage(fallDamage);
                }
            }

            // Reset highestYPosition and isFalling after landing
            highestYPosition = transform.position.y;
            isFalling = false;
            Debug.Log("Character has landed");
        }

        lastYPosition = transform.position.y; // Update last known Y position
    }

    private void ApplyFallDamage(float damage)
    {
        int roundedDamage = Mathf.RoundToInt(damage); // Round float damage to nearest integer

        // Apply damage logic here, e.g., reduce health
        playerStats.TakingDamage(roundedDamage);

        // Example: Debug log for now
        Debug.Log("Applied fall damage: " + roundedDamage);
    }
    #endregion


    #region Input Handlers
    // Inputs method
    void HandleInputs()
    {
        if (InputManager.Pause) 
        {
            UIManager.Instance.Pause(); 
        }

        horizontalMove = Input.GetAxisRaw("Horizontal");

        HandleRunning(); 
        HandleWalking(); 
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

    public void HandleAttackInput()
    {
        StartCoroutine(HandleAttackRoutine());
    }

    private IEnumerator HandleAttackRoutine()
    {
        // Set attacking state and change color to red
        isAttacking = true;
        playerSpriteRenderer.color = Color.red;
        attackController.isAttacking = true;

        // Handle ranged attack if the weapon is available and cooldown is complete
        if (weaponController.hasWeapon && weaponController.currentWeapon != null && weaponController.shotCooldown <= 0f)
        {
            HandleRangedAttack();
            weaponController.shotCooldown = weaponController.currentWeapon.attackSpeed;
        }
        else
        {
            // Perform melee attack
            attackController.Hit();
        }

        // Wait for a short duration for visual feedback
        yield return new WaitForSeconds(0.1f);

        // Revert the color back to the original color and reset attacking state
        playerSpriteRenderer.color = originalColor;
        isAttacking = false;
        attackController.isAttacking = false;

        // Ensure melee attack cooldown is respected
        yield return new WaitForSeconds(attackController.meleeCooldown);
        animator.SetBool("isAttacking", false);
    }


    public void HandleRangedAttack()
    {
        if (weaponController.currentWeapon == null)
        {
            Debug.Log("Currently without weapon");
            return;
        }

        Collider2D[] enemiesInRange = Physics2D.OverlapCircleAll(transform.position, weaponController.currentWeapon.range, attackController.whatIsEnemy);
        if (enemiesInRange.Length > 0)
        {
            Collider2D nearestEnemy = weaponController.GetNearestEnemyInFront(enemiesInRange);
            if (nearestEnemy != null)
            {
                weaponController.PerformRangedAttack(nearestEnemy.transform.position);
            }
        }
    }

    private void HandleWalking()
    {
        // Determine if the player is walking or idle based on horizontal input
        if (!isRunning)
        {
            isWalking = Mathf.Abs(horizontalMove) > 0.1f;
        }

        // Set isWalking and isRunning based on speed
        float speed = isRunning ? runSpeed : walkSpeed;
        horizontalMove = horizontalMove * speed;

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
    }

    private void HandleRunning()
    {
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
    }

    private void HandleClimbingInput()
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

        if (crouchDisableCollider != null)
        {
            crouchDisableCollider.enabled = !isCrouching;
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

    private IEnumerator ResetAttackState()
    {
        yield return new WaitForSeconds(attackController.meleeCooldown);
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

    public void OnLanding()
    {
        // Restore jumps when touching the ground.
        Jump(false);
        isJumping = false; // Ensure jumping state is reset
        animator.SetBool("isJumping", isJumping); // Ensure animator stops jump animation
        // Trigger landing particle system
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

    private void Climb(float move)
    {
        Vector3 targetVelocity = new Vector2(m_Rigidbody2D.velocity.x, move * climbSpeed);
        m_Rigidbody2D.velocity = Vector3.SmoothDamp(m_Rigidbody2D.velocity, targetVelocity, ref velocity, movementSmoothing);
    }


    private void UpdateState()
    {
        if (attackController.isAttacking)
        {
            ChangeState(CharacterState.Attacking);
        }
        else if (isClimbing)
        {
            ChangeState(CharacterState.Climbing);
        }
        else if (isFalling)
        {
            ChangeState(CharacterState.Falling);
        }
        else if (isJumping)
        {
            ChangeState(CharacterState.Jumping);
        }
        else if (isShooting)
        {
            ChangeState(CharacterState.Shooting);
        }
        else if (isRunning)
        {
            ChangeState(CharacterState.Running);
        }
        else if (isWalking)
        {
            ChangeState(CharacterState.Walking);
        }
        else if (isCrouching)
        {
            ChangeState(CharacterState.Crouching);
        }
        else
        {
            ChangeState(CharacterState.Idle);
        }
    }

    private void ChangeState(CharacterState newState)
    {
        if (currentState != newState)
        {
            //Debug.Log($"State changed from {currentState} to {newState}");
            currentState = newState;
            animator.SetInteger("State", (int)newState); // Update animator
        }
    }


    private void CheckGroundedStatus()
    {
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
    }
    #endregion
}
