using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public enum ChessPieceType
{
    None = 0,
    Pawn = 1,
    Rook = 2,
    Knight = 3,
    Bishop = 4,
    Queen = 5,
    King = 6,
}

public class ChessPieces : MonoBehaviour
{
    public int team;
    public int currentX;
    public int currentY;
    public int currentZ;
    public ChessPieceType type;

    private Vector3 desiredPosition;
    private Vector3 desiredScale;
    public List<Vector2Int> GetAvailableMoves(ref ChessPieces[,] board, int tileCountX, int tileCountY)
    {
        List<Vector2Int> r = new List<Vector2Int>();




        return r;
    }
    private void Update() {
        transform.position = Vector3.Lerp(transform.position, desiredPosition, Time.deltaTime * 10);
    }
    public virtual void SetPosition(Vector3 position, bool force = false)
    {
        desiredPosition = position;
        if(force)
            transform.position = desiredPosition;
    }

}
