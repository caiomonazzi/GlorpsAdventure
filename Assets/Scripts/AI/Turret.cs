using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Turret : AICombat
{
    #region Variables:
    [Header("Turret Settings")]
    public GameObject projectilePrefab; // The projectile that the turret will fire
    public Transform firePoint; // The point from where the projectile is fired
    public Transform turretSprite; // The sprite object to rotate

    public float rotationSpeed = 5f; // Speed of rotation to face the target
    private Transform target; // Current target the turret is aiming at
    private float nextFireTime = 0f; // Timer to control the fire rate

    private AIController aiController; // Reference to the AIController component
    private AudioSource audioSource; // AudioSource component for playing sounds
    #endregion

    #region Unity Methods:
    private void Start()
    {
        aiController = GetComponentInParent<AIController>(); // Ensure it gets the AIController from the parent
        if (aiController == null)
        {
            Debug.LogError("AIController not found on the parent object.");
            return;
        }

        // Ensure there is an AudioSource component
        audioSource = GetComponentInParent<AudioSource>();

        InvokeRepeating(nameof(FindTarget), 0f, 0.5f); // Check for targets every 0.5 seconds
    }

    private void Update()
    {
        if (target != null)
        {
            RotateTowardsTarget();
            if (Time.time >= nextFireTime)
            {
                AttackByRate();
            }
        }
    }

    #endregion

    #region Private Methods:
    private void FindTarget()
    {
        if (aiController == null) return; // Ensure aiController is not null before using it

        Collider2D[] targetsInRange = Physics2D.OverlapCircleAll(transform.position, aiController.detectionRange, targetLayer);
        float closestDistance = aiController.detectionRange;
        target = null;

        foreach (Collider2D potentialTarget in targetsInRange)
        {
            float distanceToTarget = Vector2.Distance(transform.position, potentialTarget.transform.position);
            if (distanceToTarget < closestDistance)
            {
                closestDistance = distanceToTarget;
                target = potentialTarget.transform;
            }
        }

        if (target != null)
        {
           //  Debug.Log($"Target found at position: {target.position}");
        }
    }

    private void RotateTowardsTarget()
    {
        if (target == null) return;

        Vector3 direction = (target.position - transform.position).normalized;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        Quaternion targetRotation = Quaternion.Euler(new Vector3(0, 0, angle));
        turretSprite.rotation = Quaternion.Slerp(turretSprite.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        // Debug.Log($"Turret is aiming at target position: {target.position}");
    }

    private void AttackByRate()
    {
        if (timeBetweenShots <= 0)
        {
            RangeAttack(projectilePrefab, target); // Spawn weapon
            timeBetweenShots = startTimeBetweenShots; // Reset time to start again
        }
        else
        {
            timeBetweenShots -= Time.deltaTime; // Countdown the time
        }
    }
    #endregion
}
