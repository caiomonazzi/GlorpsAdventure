using UnityEngine;

[CreateAssetMenu(fileName = "New Weapon", menuName = "Weapon")]
public class Weapon : ScriptableObject
{
    public string weaponName;
    public int damage;
    public float attackSpeed; // Cooldown between shots in seconds
    public float range;
    public GameObject projectilePrefab; // Prefab for the weapon's projectile
    public Sprite weaponSprite; // Sprite for the weapon
    public float projectileForce; // Force applied to the projectile (moveSpeed)
    public int shots; // Number of shots the weapon has
    public int remainingBullets; // Remaining bullets for the weapon
}
