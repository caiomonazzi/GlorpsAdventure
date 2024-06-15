﻿using UnityEngine;

public class AttackController : MonoBehaviour
{
    #region Variables
    private PlayerStats playerStats;

    [HideInInspector] public AudioSource audioSource;
    [HideInInspector] public DamageEffect damageEffect; //Damage effect

    [Header("Melee")] // The attackController is reserved for melee attacks, taking the attackPoint to deal the damage.
    public float meleeCooldown = 1f; // Cooldown for melee attacks
    public float nextMeleeTime = 0f; // Time when the next melee attack is allowed
    public int meleeDamage = 10; // Amount of damage.
    public float range = 1f; // Attack radius that takes as its center the attackPoint.
    public Transform attackPoint; // Point from which to attack.
    public LayerMask whatIsEnemy; // A mask determining what is enemy to the character.

    [Header("Ranged Weapon")] // All the ranged weapons will be on weaponController
    private WeaponController weaponController;

    [HideInInspector] public bool isAttacking = false;
    #endregion

    #region Unity Methods
    private void OnDrawGizmosSelected()
    {
        DrawAttackArea();
    }
    #endregion

    #region Public Methods



    public void Hit()
    {
        if (attackPoint == null) return;

        Collider2D[] enemiesToDamage = Physics2D.OverlapCircleAll(attackPoint.position, range, whatIsEnemy);
    }


    #endregion

    #region Private Methods
    private void DrawAttackArea()
    {
        if (attackPoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(attackPoint.position, range);
        }
    }

    #endregion
}
