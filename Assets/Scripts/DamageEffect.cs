using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


[Serializable]
public class DamageEffect
{
    //Parameters:
    public Color hitColor, normalColor;

    public IEnumerator Damage(SpriteRenderer sprite)
    {
        Color originalColor = sprite.color;
        Color fadeColor = originalColor;
        fadeColor.a = 0.5f; // Example alpha change to 50%

        sprite.color = fadeColor;

        yield return new WaitForSeconds(0.1f); // Duration of the effect

        // Ensure the alpha value is fully reset to the original alpha
        originalColor.a = 1f;
        sprite.color = originalColor;
    }
}
