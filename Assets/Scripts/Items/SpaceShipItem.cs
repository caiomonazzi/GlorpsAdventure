using UnityEngine;

public class SpaceShipPieceItem : MonoBehaviour
{
    public SpaceShipPiece piece;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            PlayerStats.Instance.CollectPiece(piece);
            Destroy(gameObject);
        }
    }
}
