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
    public float shotCooldown = 0f;
    private Quaternion originalRotation;
    private Animator animator; // Reference to the Animator
    #endregion

    #region Unity Methods:
    private void Awake()
    {
        character = FindFirstObjectByType<Character>();
        attackController = character.GetComponent<AttackController>();

        if (character == null)
        {
            Debug.LogError("Character not found in the scene.");
        }
        if (character != null)
        {
            animator = character.GetComponentInChildren<Animator>();
            if (animator == null)
            {
                Debug.LogError("Animator not found in Character or its children.");
            }
        }

        if (attackController == null)
        {
            Debug.LogError("AttackController component not found on the GameObject.");
        }

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
        if (character == null || attackController == null || shootingPoint == null)
        {
            Debug.LogWarning("WeaponController dependencies are not fully initialized.");
            return;
        }

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
    }
    #endregion

    #region Private Methods:


    private void EquipWeapon(Weapon weapon)
    {
        currentWeapon = weapon;
        if (weapon == null)
        {
            shootingPointSpriteRenderer.sprite = null;
            hasWeapon = false;
            PlayerStats.Instance.SetHasWeapon(false);
        }
        else
        {
            shootingPointSpriteRenderer.sprite = weapon.weaponSprite;
            hasWeapon = true;
            PlayerStats.Instance.SetHasWeapon(true);
            shotsRemaining = weapon.shots;
        }

        if (UIManager.Instance != null)
        {
            UIManager.Instance.UpdateUI();
        }

        if (animator != null)
        {
            animator.SetBool("hasWeapon", hasWeapon);
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

    public Collider2D GetNearestEnemyInFront(Collider2D[] enemies)
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

    public void PerformRangedAttack(Vector2 enemyPosition)
    {
        if (hasWeapon && currentWeapon != null && shotCooldown <= 0f)
        {
            if (shotsRemaining > 0)
            {
                shotsRemaining--;
                Vector2 direction = (enemyPosition - (Vector2)shootingPoint.position).normalized;
                Shoot(direction, enemyPosition);

                if (shotsRemaining <= 0)
                {
                    RemoveWeapon();
                }
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

            character.isShooting = true; // Set character's isShooting to true

            // Reset isShooting after a short delay to allow for animation
            StartCoroutine(ResetShootingState());

            UIManager.Instance.UpdateUI(); // Update the UI to show no weapon
        }
    }

    private IEnumerator ResetShootingState()
    {
        yield return new WaitForSeconds(shotCooldown);
        if (animator != null)
        {
            animator.SetBool("isShooting", false);
        }

        character.isShooting = false;
    }


    #endregion

    #region Public Methods:
    public void HandleAttack()
    {
        if (hasWeapon && currentWeapon != null && shotsRemaining > 0)
        {
            character.HandleRangedAttack();
        }
    }

    public void CollectWeapon(Weapon newWeapon)
    {
        if (newWeapon != null)
        {
            if (currentWeapon != null && newWeapon.weaponName == currentWeapon.weaponName)
            {
                // Add bullets to the current weapon
                shotsRemaining += newWeapon.shots;
                UIManager.Instance.UpdateUI(); // Update the UI to reflect the added bullets
            }
            else
            {
                EquipWeapon(newWeapon);
            }
        }
        else
        {
            Debug.LogWarning("Trying to collect null weapon.");
        }
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
            UIManager.Instance.UpdateUI();
        }

        if (animator != null)
        {
            animator.SetBool("hasWeapon", hasWeapon);
        }

        Debug.Log("Weapon removed after all shots are used.");
    }
    #endregion
}