using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class AICombat : MonoBehaviour
{
    #region Variables:
    [HideInInspector] public AIStats aiStats;
    public GameObject rangeWeapon; // Prefab for the range weapon

    protected float timeBetweenShots; // Time between shots
    public float startTimeBetweenShots = 1f; // Start time between shots
    public float projectileSpeed = 10f; // Speed of the projectile
    public float projectileDamage = 10f; // Damage of the projectile
    public float projectileForce = 5f; // Force applied to the projectile
    public LayerMask targetLayer; // The layer on which the targets are
    #endregion

    #region Methods:
    private void Start()
    {
        aiStats = GetComponent<AIStats>();
    }

    // Virtual method for Range Attack, reconfigured in child classes
    public virtual void RangeAttack(GameObject rangeWeapon, Transform target)
    {
        if (rangeWeapon == null)
        {
            // If rangeWeapon is not assigned, perform a melee attack instead
            MeleeAttack(target.gameObject);
            return;
        }

        GameObject rangeShot = Instantiate(rangeWeapon); // Creates a weapon
        rangeShot.transform.position = transform.position; // Moves the weapon to its position

        // Calculate the angle and position where to shoot
        Vector2 dir = new Vector2(target.position.x - transform.position.x, target.position.y - transform.position.y);

        rangeShot.transform.up = dir;

        // Initialize the projectile with the correct parameters
        AIProjectile projectileScript = rangeShot.GetComponent<AIProjectile>();
        if (projectileScript != null)
        {
            projectileScript.Initialize(target.position, projectileSpeed, projectileDamage, targetLayer, projectileForce);
        }
    }


    //Virtual method for Melee Attack, reconfigured in child classes
    public virtual void MeleeAttack(GameObject target)
    {
        PlayerStats playerStats = PlayerStats.Instance;
        if (playerStats != null && !playerStats.isInvincible)
        {
            playerStats.TakingDamage();
        }
        else
        {
            Debug.Log("Player is invincible, no damage dealt.");
        }
    }
    #endregion
}