using UnityEngine;

public class LadderInteraction : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            Character character = collision.GetComponent<Character>();
            if (character != null)
            {
                character.StartClimbing();
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            Character character = collision.GetComponent<Character>();
            if (character != null)
            {
                character.StopClimbing();
            }
        }
    }
}
