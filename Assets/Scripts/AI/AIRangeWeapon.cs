using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIRangeWeapon : RangeWeapon
{
    private Rigidbody2D rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
        }
        rb.gravityScale = 0;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
    }
    public void Initialize(Vector3 targetPosition, int damage, float speed, LayerMask targetLayer, float force)
    {
        this.damageRange = new DoubleFloat(damage, damage);
        this.targetLayer = targetLayer;
        this.moveSpeed = speed;

        Vector2 direction = (targetPosition - transform.position).normalized;
        rb.velocity = direction * speed;
        rb.AddForce(direction * force, ForceMode2D.Impulse);
    }


    public override void OnTriggerEnter2D(Collider2D collider)
    {
        base.OnTriggerEnter2D(collider);

        if (collider.gameObject.layer == LayerMask.NameToLayer("Player")) // If contact with player
        {
            Damage(PlayerStats.Instance); // Player damaged
        }
    }

    // Damage method
    void Damage(PlayerStats player)
    {
        player.TakingDamage(); // Player hp - 1 
        Destroying(); // Destroying gameobject
    }
}
