using UnityEngine;

public enum GamePieceType
{
    None = 0,
    Pawn = 1,
    Rook = 2,
    Knight = 3,
    Bishop = 4,
    Queen = 5,
    King = 6,
    AttackBoard = 7
}

public class GamePiece : MonoBehaviour
{
    public int team;
    public int currentX;
    public int currentY;
    public int currentZ;
    public GamePieceType type;

    private Vector3 desiredPosition;
    private Vector3 desiredScale;

}
