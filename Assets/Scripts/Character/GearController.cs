/*using UnityEngine;
using System.Collections;

public class GearController : MonoBehaviour
{
    #region Variables
    private Character character;
    private Rigidbody2DParameters originalParams;
    private Coroutine gearCoroutine;
    public Gear currentGear;
    public AudioSource audioSource;
    public bool hasGear = false;
    public RuntimeAnimatorController baseGearController;
    public bool HasGear()
    {
        return hasGear;
    }
    public Gear GetCurrentGear()
    {
        return currentGear;
    }
    #endregion

    #region Unity Methods
    private void Awake()
    {
        character = GetComponent<Character>();
    }

    private void Start()
    {
        originalParams = character.GetOriginalRigidbody2DParameters();
    }
    #endregion

    #region Private Methods
    private void ApplyGearParameters(Gear gear)
    {
        currentGear = gear;
        Rigidbody2D rb = character.GetComponent<Rigidbody2D>();

        character.runSpeed = gear.runSpeed;
        character.walkSpeed = gear.walkSpeed;

        if (gear.changeMass)
        {
            rb.mass = gear.mass;
        }
        rb.gravityScale = gear.gravityScale;
        rb.drag = gear.drag;

        // Play loop sound if assigned
        if (gear.loopSound != null)
        {
            AudioManager.Instance.PlayMusic(gear.loopSound);
        }

        // Change the gear sprite and play animation if assigned
        if (character.gearHolder != null)
        {
            SpriteRenderer gearSpriteRenderer = character.gearHolder.GetComponent<SpriteRenderer>();
            Animator gearAnimator = character.gearHolder.GetComponent<Animator>();

            if (gearSpriteRenderer != null)
            {
                gearSpriteRenderer.sprite = gear.gearSprite;
               //  Debug.Log($"GearController: Assigned sprite {gear.gearSprite.name}");
            }

            if (gearAnimator != null)
            {
                if (gear.gearAnimation != null)
                {
                    // Debug.Log($"GearController: Playing gear animation {gear.gearAnimation.name}");

                    AnimatorOverrideController animatorOverrideController = new AnimatorOverrideController(baseGearController);
                    animatorOverrideController["BaseAnimation"] = gear.gearAnimation;
                    gearAnimator.runtimeAnimatorController = animatorOverrideController;
                    gearAnimator.Play("BaseAnimation");

                }
                else
                {
                    Debug.LogWarning("GearController: Gear animation is null, using sprite renderer only.");
                }
            }
            else
            {
                Debug.LogWarning("GearController: Gear animator is null.");
            }
        }
    }

    private IEnumerator RemoveGearAfterTime(float duration)
    {
        yield return new WaitForSeconds(duration);
        RemoveGear();
    }

    private void RemoveGear()
    {
        currentGear = null;
        Rigidbody2D rb = character.GetComponent<Rigidbody2D>();

        rb.bodyType = originalParams.bodyType;
        rb.sharedMaterial = originalParams.material;
        rb.simulated = originalParams.simulated;
        rb.useAutoMass = originalParams.useAutoMass;
        rb.mass = originalParams.mass;
        rb.drag = originalParams.linearDrag;
        rb.angularDrag = originalParams.angularDrag;
        rb.gravityScale = originalParams.gravityScale;
        rb.collisionDetectionMode = originalParams.collisionDetectionMode;
        rb.interpolation = originalParams.interpolation;
        rb.constraints = originalParams.constraints;

        character.runSpeed = character.originalRunSpeed;
        character.walkSpeed = character.originalWalkSpeed;

        // Reset the gear sprite and animation
        if (character.gearHolder != null)
        {
            SpriteRenderer gearSpriteRenderer = character.gearHolder.GetComponent<SpriteRenderer>();
            if (gearSpriteRenderer != null)
            {
                gearSpriteRenderer.sprite = null;
                Debug.Log("GearController: Reset gear sprite to null.");
            }

            // Reset gear animation
            ResetGearAnimation();
        }

        hasGear = false;
        PlayerStats.Instance.SetHasGear(false);
        UIManager.Instance.UpdateUI(); // Update the UI to remove the gear
    }

    private void ResetGearAnimation()
    {
        Animator gearAnimator = character.gearHolder.GetComponent<Animator>();
        if (gearAnimator != null)
        {
            Debug.Log("GearController: Resetting gear animation to base.");
            gearAnimator.runtimeAnimatorController = baseGearController;
            gearAnimator.Play("BaseAnimation");
        }
        else
        {
            Debug.LogWarning("GearController: Gear animator is null.");
        }
    }
    #endregion

    #region Public Methods
    public void CollectGear(Gear newGear)
    {
        if (gearCoroutine != null)
        {
            StopCoroutine(gearCoroutine);
        }

        hasGear = true;
        PlayerStats.Instance.SetHasGear(true);
        ApplyGearParameters(newGear);
        gearCoroutine = StartCoroutine(RemoveGearAfterTime(newGear.duration));
        UIManager.Instance.UpdateUI(); // Update the UI to show the new gear
    }

    #endregion
}
*/