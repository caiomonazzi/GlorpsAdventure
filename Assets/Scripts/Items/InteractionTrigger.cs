using UnityEngine;

public class InteractionTrigger : MonoBehaviour
{
    private InteractionCanvas canvas;

    [HideInInspector] public bool inTrigger; // Tracking trigger status

    private void Start()
    {
        canvas = GetComponentInChildren<InteractionCanvas>(true);
    }

    private void OnTriggerEnter2D(Collider2D collision) // if player ENTER in trigger
    {
        if (collision.gameObject.CompareTag("Player")) // if it's the player
        {
            inTrigger = true;
            canvas.gameObject.SetActive(true); // UI enable
        }
    }

    private void OnTriggerExit2D(Collider2D collision) // if player EXIT in trigger
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            inTrigger = false;
            canvas.gameObject.SetActive(false); // UI disable
        }
    }

}
