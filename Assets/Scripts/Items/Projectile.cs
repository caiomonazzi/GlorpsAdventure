using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Projectile : RangeWeapon
{
    private Rigidbody2D rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
        }
        // rb.gravityScale = 0;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
    }

    public void Initialize(Vector3 targetPosition, float moveSpeed, float damage, LayerMask targetLayer, float force)
    {
        this.moveSpeed = moveSpeed;
        this.damageRange = new DoubleFloat(damage, damage);
        this.targetLayer = targetLayer;

        Vector2 direction = (targetPosition - transform.position).normalized;
        rb.velocity = direction * moveSpeed;
        rb.AddForce(direction * force, ForceMode2D.Impulse);
    }

    private void FixedUpdate()
    {
        Move();
    }

    //Move method override to use Rigidbody2D
    protected override void Move()
    {
        Vector2 direction = rb.velocity.normalized;
        rb.velocity = direction * moveSpeed;
    }

    public override void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider.gameObject.layer == LayerMask.NameToLayer("Enemy") && ((1 << collider.gameObject.layer) & targetLayer) != 0)
        {
            AIStats enemy = collider.gameObject.GetComponent<AIStats>();
            if (enemy != null)
            {
                Damage(enemy);
            }
        }
        else if (collider.gameObject.layer == LayerMask.NameToLayer("Obstacle"))
        {
            Destroying();
        }
    }

    void Damage(AIStats enemy)
    {
        enemy.TakingDamage(damageRange.RandomFloat());
        Destroying();
    }
}
