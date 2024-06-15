using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class AIProjectile : MonoBehaviour
{
    private Rigidbody2D rb;

    [Header("Projectile Settings")]
    public float damageAmount = 10f;
    public float force = 5f;
    public float moveSpeed = 10f;
    public LayerMask targetLayer;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
        }
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
    }

    public void Initialize(Vector3 targetPosition, float moveSpeed, float damageAmount, LayerMask targetLayer, float force)
    {
        this.moveSpeed = moveSpeed;
        this.damageAmount = damageAmount;
        this.targetLayer = targetLayer;

        Vector2 direction = (targetPosition - transform.position).normalized;
        rb.velocity = direction * moveSpeed;
        rb.AddForce(direction * force, ForceMode2D.Impulse);

        // Debug information for projectile initialization
        Debug.Log($"Projectile initialized with direction: {direction}, velocity: {rb.velocity}, target position: {targetPosition}");
    }

    private void FixedUpdate()
    {
        Move();
    }

    protected void Move()
    {
        Vector2 direction = rb.velocity.normalized;
        rb.velocity = direction * moveSpeed;
    }

    private void OnTriggerEnter2D(Collider2D collider)
    {
        if (((1 << collider.gameObject.layer) & targetLayer) != 0)
        {
            PlayerStats playerStats = collider.gameObject.GetComponent<PlayerStats>();
            if (playerStats != null)
            {
                playerStats.TakingDamage((int)damageAmount); // Apply the specified damage amount
                Destroy(gameObject);
            }
        }
        else if (collider.gameObject.layer == LayerMask.NameToLayer("Obstacle"))
        {
            Destroy(gameObject);
        }
    }

    private void OnBecameInvisible()
    {
        Destroy(gameObject); // Destroy the projectile when it goes off-screen
    }
}
