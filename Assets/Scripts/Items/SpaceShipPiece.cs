using UnityEngine;

[CreateAssetMenu(fileName = "SpaceShipPiece", menuName = "/SpaceShipPiece", order = 1)]
public class SpaceShipPiece : ScriptableObject
{
    public string pieceName;
    public Quadrant quadrant;
}

public enum Quadrant
{
    TopLeft,
    TopRight,
    BottomLeft,
    BottomRight
}
