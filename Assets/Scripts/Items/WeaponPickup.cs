using UnityEngine;

public class WeaponItem : Item
{
    public Weapon weapon; // Assign the weapon ScriptableObject in the Inspector


    public override void OnTriggerEnter2D(Collider2D collision)
{
        if (collision.CompareTag("Player"))
        {
            WeaponController weaponController = collision.GetComponent<WeaponController>();
            if (weaponController != null)
            {
                weaponController.CollectWeapon(weapon);
                onPickedUp += OnPickedUp;
                base.OnTriggerEnter2D(collision);
            }
        }
    }

    public void OnPickedUp()
    {
        WeaponController weaponController = PlayerStats.Instance.gameObject.GetComponent<WeaponController>();

        if (weaponController != null)
        {
            weaponController.CollectWeapon(weapon);
            UIManager.Instance.UpdateUI(); // Update the UI to show the new weapon
        }
        else
        {
            Debug.LogWarning("WeaponController not found on player.");
        }
    }
}
