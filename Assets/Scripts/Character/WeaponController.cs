using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class WeaponController : MonoBehaviour
{
    #region Variables:
    private Character character;
    [SerializeField] private Transform shootingPoint; // Point from which to attack/shoot
    public int shotsRemaining; // Track remaining shots

    private SpriteRenderer shootingPointSpriteRenderer;
    private AttackController attackController;

    public Weapon currentWeapon;
    public bool hasWeapon = false;
    private float shotCooldown = 0f;
    private Quaternion originalRotation;

    private Animator animator; // Reference to the Animator
    #endregion

    #region Unity Methods:
    private void Awake()
    {
        character = FindFirstObjectByType<Character>();

        if (character == null)
        {
            Debug.LogError("Character not found in the scene.");
        }
        if (character != null)
        {
            animator = character.GetComponentInChildren<Animator>(); // Get the Animator from the Character
            if (animator == null)
            {
                Debug.LogError("Animator not found in Character or its children.");
            }
        }

        attackController = GetComponent<AttackController>();

        if (attackController == null)
        {
            Debug.LogError("AttackController component not found on the GameObject.");
        }

        shootingPointSpriteRenderer = shootingPoint.GetComponent<SpriteRenderer>();
        if (shootingPoint != null)
        {
            shootingPointSpriteRenderer = shootingPoint.GetComponent<SpriteRenderer>();
            originalRotation = shootingPoint.localRotation;
        }

        else
        {
            Debug.LogError("Shooting Point is not assigned in the WeaponController.");
        }

    }

    private void Update()
    {
        // Ensure necessary components are available before proceeding
        if (character == null || attackController == null || shootingPoint == null)
        {
            Debug.LogWarning("WeaponController dependencies are not fully initialized.");
            return;
        }


        if (hasWeapon || currentWeapon != null)
        {
            HandleAttackInput();
            shotCooldown -= Time.deltaTime;

            if (hasWeapon && currentWeapon != null)
            {
                Collider2D[] enemiesInRange = Physics2D.OverlapCircleAll(transform.position, currentWeapon.range, attackController.whatIsEnemy);
                if (enemiesInRange.Length > 0)
                {
                    Collider2D nearestEnemy = GetNearestEnemyInFront(enemiesInRange);
                    if (nearestEnemy != null)
                    {
                        AimAtTarget(nearestEnemy.transform.position);
                    }
                    else
                    {
                        ResetAiming();
                    }
                }
                else
                {
                    ResetAiming();
                }
            }
            else
            {
                ResetAiming();
            }
        }
    }
    #endregion

    #region Private Methods:
    private void HandleAttackInput()
    {
        if (character == null)
        {
            Debug.LogWarning("Character or InputManager is not initialized.");
            return;
        }

        if (InputManager.Attack && !character.isJumping)
        {
            PerformMeleeAttack();
        }

        if (InputManager.RangedAttack && !character.isJumping && shotCooldown <= 0f)
        {

                HandleRangedAttack();
            if (hasWeapon && currentWeapon != null)
            {
                shotCooldown = currentWeapon.attackSpeed;
            }
            else
            {
                Debug.Log("Currently without weapon");
                hasWeapon = false;
                currentWeapon = null; // Ensure currentWeapon is null
                PerformMeleeAttack();
            }
        }
    }

    private void EquipWeapon(Weapon weapon)
    {
        currentWeapon = weapon;
        if (weapon == null)
        {
            // Reset to melee attack settings
            shootingPointSpriteRenderer.sprite = null;
            hasWeapon = false;
            PlayerStats.Instance.SetHasWeapon(false);
        }
        else
        {
            // Equip the ranged weapon
            shootingPointSpriteRenderer.sprite = weapon.weaponSprite;
            hasWeapon = true;
            PlayerStats.Instance.SetHasWeapon(true);
            shotsRemaining = weapon.shots; // Initialize remaining shots
        }

        if (UIManager.Instance != null)
        {
            UIManager.Instance.UpdateUI(); // Update the UI to show the new weapon
        }

        if (animator != null)
        {
            animator.SetBool("hasWeapon", hasWeapon); // Update the animator
        }
    }

    private void HandleRangedAttack()
    {
        if (currentWeapon == null)
        {
            Debug.Log("Currently without weapon");
            return;
        }

        Collider2D[] enemiesInRange = Physics2D.OverlapCircleAll(transform.position, currentWeapon.range, attackController.whatIsEnemy);
        if (enemiesInRange.Length > 0)
        {
            Collider2D nearestEnemy = GetNearestEnemyInFront(enemiesInRange);
            if (nearestEnemy != null)
            {
                PerformRangedAttack(nearestEnemy.transform.position);
            }
        }
    }

    private void AimAtTarget(Vector3 targetPosition)
    {
        if (shootingPoint == null) return;

        Vector2 direction = (targetPosition - shootingPoint.position).normalized;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        if (!character.isFacingRight)
        {
            angle = Mathf.Atan2(direction.y, -direction.x) * Mathf.Rad2Deg; // Invert x direction if facing left
        }

        // Ensure the angle stays between -90 and +90 degrees
        angle = Mathf.Clamp(angle, -90, 90);

        shootingPoint.localRotation = Quaternion.Euler(new Vector3(0, 0, angle));
    }

    private void ResetAiming()
    {
        if (shootingPoint != null)
        {
            shootingPoint.localRotation = originalRotation;
        }
    }

    private Collider2D GetNearestEnemyInFront(Collider2D[] enemies)
    {
        Collider2D nearestEnemy = null;
        float nearestDistance = float.MaxValue;

        foreach (var enemy in enemies)
        {

            float distance = Vector2.Distance(shootingPoint.position, enemy.transform.position);
            Vector2 directionToEnemy = enemy.transform.position - shootingPoint.position;

            // Check if the enemy is in front of the player
            if ((character.isFacingRight && directionToEnemy.x > 0) || (!character.isFacingRight && directionToEnemy.x < 0))
            {
                if (distance < nearestDistance)
                {
                    nearestEnemy = enemy;
                    nearestDistance = distance;
                }
            }
        }

        return nearestEnemy;
    }

    private void PerformMeleeAttack()
    {
        if (attackController != null)
        {
            attackController.Hit();  // Calls the appropriate attack logic in AttackController
            Debug.Log("Performing Melee Attack");
        }
        else
        {
            Debug.LogWarning("AttackController is not initialized.");
        }
    }

    private void PerformRangedAttack(Vector2 enemyPosition)
    {
        if (hasWeapon && currentWeapon != null)
        {
            if (shotsRemaining > 0)
            {
                shotsRemaining--; // Consume a shot
                Vector2 direction = (enemyPosition - (Vector2)shootingPoint.position).normalized;
                Shoot(direction, enemyPosition);

                if (shotsRemaining <= 0)
                {
                    RemoveWeapon(); // Remove weapon when out of shots
                }
            }
            else
            {
                PerformMeleeAttack();
            }
        }
    }

    private void Shoot(Vector2 direction, Vector2 targetPosition)
    {
        if (hasWeapon && currentWeapon != null && currentWeapon.projectilePrefab != null)
        {
            // Instantiate the projectile at the shootingPoint position with no rotation
            GameObject projectile = Instantiate(currentWeapon.projectilePrefab, shootingPoint.position, Quaternion.identity);

            // Get the Projectile script from the projectile
            Projectile projectileScript = projectile.GetComponent<Projectile>();

            if (projectileScript != null)
            {
                // Set the projectile parameters
                projectileScript.Initialize(targetPosition, currentWeapon.projectileForce, currentWeapon.damage, attackController.whatIsEnemy, currentWeapon.projectileForce);
            }

            if (animator != null)
            {
                animator.SetBool("isShooting", true); // Update the animator
            }

            // Reset isShooting after a short delay to allow for animation
            StartCoroutine(ResetShootingState());

            UIManager.Instance.UpdateUI(); // Update the UI to show no weapon
        }
    }

    private IEnumerator ResetShootingState()
    {
        yield return new WaitForSeconds(1f);
        if (animator != null)
        {
            animator.SetBool("isShooting", false); // Reset the animator
        }
    }

    #endregion

    #region Public Methods:
    public void HandleAttack()
    {
        if (hasWeapon && currentWeapon != null && shotsRemaining > 0)
        {
            HandleRangedAttack();
        }
        else
        {
            PerformMeleeAttack();
        }
    }
    public void CollectWeapon(Weapon newWeapon)
    {
        EquipWeapon(newWeapon);
    }

    public void RemoveWeapon()
    {
        currentWeapon = null;

        hasWeapon = false;
        PlayerStats.Instance.SetHasWeapon(false);

        if (shootingPointSpriteRenderer != null)
        {
            shootingPointSpriteRenderer.sprite = null;
        }

        if (UIManager.Instance != null)
        {
            UIManager.Instance.UpdateUI(); // Update the UI to show no weapon
        }

        if (animator != null)
        {
            animator.SetBool("hasWeapon", hasWeapon); // Update the animator
        }

        Debug.Log("Weapon removed after all shots are used.");
    }
    #endregion
}