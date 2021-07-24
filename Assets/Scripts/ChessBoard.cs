using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.MagicLeap;
using UnityEngine.UI;
using TMPro;

public enum ChessBoardType 
{
    None = 0,
    SmallLeft = 1,
    SmallRight = 2,
    Large = 3
}

public class ChessBoard : MonoBehaviour
{  

  
    private ChessPieces[,,] chessPieces;
    private ChessPieces[,] chessPiece;
    private ChessPieces currentlyDragging;
    private List<ChessPieces> deadWhites = new List<ChessPieces>();
    private List<ChessPieces> deadBlacks = new List<ChessPieces>();    
    public ChessBoardType boardType;

    [Header("Prefabs and Materials")]
    [SerializeField] private GameObject[] prefabs;
    [SerializeField] private Material[] teamMaterials;
    
    [Header("Art Stuff")]
    [SerializeField] private Material tileMaterial;
    [SerializeField] private float dragOffset = 1.0f;
    [SerializeField] private float tileSize = 1.0f;
    [SerializeField] private float yOffset = 0.2f;
    [SerializeField] private Vector3 boardCenter = Vector3.zero;
    private MLInput.Controller controller;
    //LOGIC
    [SerializeField] private int TILE_COUNT_X = 4;
    [SerializeField] private int TILE_COUNT_Y = 4;
    private int TILE_COUNT_Z = 1;
    private GameObject[,] basetiles;
    private Camera currentCamera;
    private List<Vector2Int> availableMoves = new List<Vector2Int>();
    private Vector3Int currentHover;
    private Vector3 bounds;
    private GameObject[,,] tiles;
    public int team;
    bool pressed = false;


    //Functions Begin
    void Start() {
        controller = MLInput.GetController(MLInput.Hand.Right);
    }
    private void Awake() 
    {
        GenerateLargeBoards(tileSize, TILE_COUNT_X, TILE_COUNT_Y);
        SpawnAllPieces();
        PositionAllPieces();
    }
    private void Update() {
        if(!currentCamera) 
        {
         currentCamera = Camera.main;
         return;
        }
        
        CheckTrigger();
        RaycastHit info;
        Ray ray = currentCamera.ScreenPointToRay(controller.Position);

        if(Physics.Raycast(ray, out info, 100, LayerMask.GetMask("Tile")))
        {
            Vector3Int hitPosition = LookupTilesIndex(info.transform.gameObject);
  
            if(currentHover == -Vector3Int.one) {
                // first time hovering
                currentHover = hitPosition;
                tiles[hitPosition.x, hitPosition.y, hitPosition.z].layer = LayerMask.NameToLayer("Hover");
            }
           
            if(currentHover != hitPosition) {
                // first time hovering
                tiles[currentHover.x, currentHover.y, currentHover.z].layer = LayerMask.NameToLayer("Tile");
                currentHover = hitPosition;
                tiles[hitPosition.x, hitPosition.y, hitPosition.z].layer = LayerMask.NameToLayer("Hover");
            }

            if(pressed == true) {
                if(chessPieces[hitPosition.x, hitPosition.y, hitPosition.z] != null) {
                    if(true) {
                        currentlyDragging = chessPieces[hitPosition.x, hitPosition.y, hitPosition.z];
                        availableMoves = currentlyDragging.GetAvailableMoves(ref chessPiece, TILE_COUNT_X, TILE_COUNT_Y);
                        HighlightTiles();
                    } 
                }   
            } else if (currentlyDragging != null && pressed == false) {
               
               //z?
                Vector3Int previousPosition = new Vector3Int(currentlyDragging.currentX, currentlyDragging.currentY, currentlyDragging.currentZ);
                bool validMove = MoveTo(currentlyDragging, hitPosition.x, hitPosition.y, hitPosition.z);

                if(!validMove) {
                    currentlyDragging.SetPosition(GetTileCenter(previousPosition.x, previousPosition.y));
                    currentlyDragging = null;
                } else {
                    currentlyDragging = null;
                }
            }
        
        } else {
            if(currentHover != -Vector3Int.one) {
                currentHover = -Vector3Int.one;
            }

            if(currentlyDragging && !pressed) {
                currentlyDragging.SetPosition(GetTileCenter(currentlyDragging.currentX, currentlyDragging.currentY));
                currentlyDragging = null;
            }
        }

        if(currentlyDragging) {
            Plane horizonPlane = new Plane(Vector3.up,Vector3.up * yOffset);
            float distance = 0.0f;
            if(horizonPlane.Raycast(ray, out distance)) {
                currentlyDragging.SetPosition(ray.GetPoint(distance) + Vector3.up * dragOffset);
            }
        }
    }

    public void MovePieces()
    { 
        RaycastHit info;
        Ray ray = currentCamera.ScreenPointToRay(controller.Position);

        if(Physics.Raycast(ray, out info, 100, LayerMask.GetMask("Tile")))
        {
            Vector3Int hitPosition = LookupTilesIndex(info.transform.gameObject);
            
            if(chessPieces[hitPosition.x, hitPosition.y, hitPosition.z] != null) {
                    //is it our turn?
                if(true) {
                    currentlyDragging = chessPieces[hitPosition.x, hitPosition.y, hitPosition.z];
                    Debug.Log(currentlyDragging);
                }
            }
        }
    }
  
    void CheckTrigger() {
        if (Input.GetMouseButtonDown(0) || controller.TriggerValue > 0.2f) {
            //pressed
            pressed = true;
        } else if(Input.GetMouseButtonUp(0) ||controller.TriggerValue < 0.2f) {
            //not pressed
            pressed = false;
        }
    }
    private void GenerateLargeBoards(float tileSize, int tileCountX, int tileCountY)
    {
       yOffset += transform.position.y;
       bounds = new Vector3((tileCountX /2) * tileSize, 0, (tileCountY /2) * tileSize) + boardCenter;
       
        basetiles = new GameObject[tileCountX, tileCountY];
        for(int x = 0; x < tileCountX; x++) {
            for(int y = 0; y < tileCountY; y++) {
                    basetiles[x,y] = GenerateSingleTile(tileSize, x, y);
                    
            }
        }
    }
    private bool MoveTo(ChessPieces cp, int x, int y, int z) {

        Vector3Int previousPosition = new Vector3Int(cp.currentX, cp.currentY, cp.currentZ);
        
        if(chessPieces[x,y,z] != null) {
            ChessPieces ocp = chessPieces[x,y,z];
            if(cp.team == ocp.team) {   
                return false;
            }

            if(ocp.team == 0) {
                deadWhites.Add(ocp);
                ocp.SetPosition(new Vector3(4*tileSize, yOffset, -1 * tileSize) - bounds + new Vector3(tileSize/2, 0, tileSize/2) + (Vector3.forward * 0.3f) * deadWhites.Count);

            } else {
                deadBlacks.Add(ocp);
                ocp.SetPosition(new Vector3(-1*tileSize, yOffset, 4 * tileSize) - bounds + new Vector3(tileSize/2, 0, tileSize/2) + (Vector3.back * 0.3f) * deadBlacks.Count);
            }
        }
        chessPieces[x,y,z] = cp;
        chessPieces[previousPosition.x, previousPosition.y, previousPosition.z] = null;

        PositionSinglePiece(x,y,z);
        return true;
    }
    
    private Vector2Int LookupTileIndex(GameObject hitInfo) 
    {
        for(int x = 0; x < TILE_COUNT_X; x++) {
            for(int y = 0; y < TILE_COUNT_Y; y++) {
                    if(basetiles[x,y] == hitInfo) {
                    return new Vector2Int(x, y);
                }
                
            }
        }
        return -Vector2Int.one;
    }
    private Vector3Int LookupTilesIndex(GameObject hitInfo) 
    {
        for(int x = 0; x < TILE_COUNT_X; x++) {
            for(int y = 0; y < TILE_COUNT_Y; y++) {
                for(int z = 0; z < TILE_COUNT_Z; z++) {

                    if(tiles[x,y,z] == hitInfo) {
                    return new Vector3Int(x, y, z);
                }
                
            }
        }
        }
        return -Vector3Int.one;
    }
    private GameObject GenerateSingleTile(float tileSize, int x, int y)
    {
        GameObject tileObject = new GameObject(string.Format("X:{0}, Y:{1}", x, y));
        tileObject.transform.parent = transform;

        Mesh mesh = new Mesh();
        tileObject.AddComponent<MeshFilter>().mesh = mesh;
        tileObject.AddComponent<MeshRenderer>().material = tileMaterial;

        Vector3[] vertices = new Vector3[4];
        vertices[0] = new Vector3(x * tileSize, yOffset, y * tileSize) - bounds;
        vertices[1] = new Vector3(x * tileSize, yOffset, (y+1) * tileSize) - bounds;
        vertices[2] = new Vector3((x+1) * tileSize, yOffset, y * tileSize) - bounds;
        vertices[3] = new Vector3((x+1) * tileSize, yOffset, (y+1) * tileSize) - bounds;

        int[] trig = new int[] {0, 1, 2, 1, 3, 2};


        mesh.vertices = vertices;
        mesh.triangles = trig;
        mesh.RecalculateNormals();


        tileObject.layer = LayerMask.NameToLayer("Tile");
        tileObject.AddComponent<BoxCollider>();


        return tileObject;
    
    }
    private void PositionAllPieces() 
    {
        for(int x=0; x < TILE_COUNT_X; x++) 
            for(int y=0; y < TILE_COUNT_Y; y++)
                        for(int z=0; z < TILE_COUNT_Z; z++)
                if(chessPieces[x,y,z] != null) 
                    PositionSinglePiece(x,y,z,true);
    }

    private void PositionSinglePiece(int x, int y, int z, bool force = false)
    {
        chessPieces[x,y,z].currentX = x;
        chessPieces[x,y,z].currentY = y;
        chessPieces[x,y,z].currentZ = z;
        chessPieces[x,y,z].SetPosition(GetTileCenter(x,y), force);
    }
    

    private void HighlightTiles() {
        for(int i = 0; i < availableMoves.Count; i++) {
            tiles[availableMoves[i].x, availableMoves[i].y, 0].layer = LayerMask.NameToLayer("Highlight"); 
        }
    }

    private void RemoveHighlightTiles() {
        for(int i = 0; i < availableMoves.Count; i++) {
            tiles[availableMoves[i].x, availableMoves[i].y, 0].layer = LayerMask.NameToLayer("Tile"); 
        }
        availableMoves.Clear();
    }

    private Vector3 GetTileCenter(int x, int y) {
        return new Vector3(x * tileSize, yOffset, y * tileSize) - bounds + new Vector3(tileSize/2,0,tileSize/2);
    }
    private void SpawnAllPieces()
    {
        chessPieces = new ChessPieces[TILE_COUNT_X, TILE_COUNT_Y, TILE_COUNT_Z];
        
        if(boardType != ChessBoardType.None) {
            if(team == 0) {
                if(boardType == ChessBoardType.Large) {
                    //White Team
                    chessPieces[0,0,0] = SpawnSinglePiece(ChessPieceType.Bishop, 0);
                    chessPieces[1,0,0] = SpawnSinglePiece(ChessPieceType.King, 0);
                    chessPieces[2,0,0] = SpawnSinglePiece(ChessPieceType.Queen, 0);
                    chessPieces[3,0,0] = SpawnSinglePiece(ChessPieceType.Bishop, 0);
                    
                    for(int i = 0; i < TILE_COUNT_X; i++) {
                        chessPieces[i,1,0] = SpawnSinglePiece(ChessPieceType.Pawn, 0);
                    }
                }

                if(boardType == ChessBoardType.SmallLeft) {
                    //White Team
                    chessPieces[0,0,0] = SpawnSinglePiece(ChessPieceType.Rook, 0);
                    chessPieces[1,0,0] = SpawnSinglePiece(ChessPieceType.Knight, 0);
                    chessPieces[0,1,0] = SpawnSinglePiece(ChessPieceType.Pawn, 0);
                    chessPieces[1,1,0] = SpawnSinglePiece(ChessPieceType.Pawn, 0);
                }

                if(boardType == ChessBoardType.SmallRight) {
                    //White Team
                    chessPieces[1,0,0] = SpawnSinglePiece(ChessPieceType.Knight, 0);
                    chessPieces[0,0,0] = SpawnSinglePiece(ChessPieceType.Rook, 0);
                    chessPieces[0,1,0] = SpawnSinglePiece(ChessPieceType.Pawn, 0);
                    chessPieces[1,1,0] = SpawnSinglePiece(ChessPieceType.Pawn, 0);
                }
            } else if(team == 1) {

                if(boardType == ChessBoardType.Large) {
                    //White Team
                    
                    chessPieces[0,3,0] = SpawnSinglePiece(ChessPieceType.Bishop, 1);
                    chessPieces[1,3,0] = SpawnSinglePiece(ChessPieceType.King, 1);
                    chessPieces[2,3,0] = SpawnSinglePiece(ChessPieceType.Queen, 1);
                    chessPieces[3,3,0] = SpawnSinglePiece(ChessPieceType.Bishop, 1);
                    
                    for(int i = 0; i < TILE_COUNT_X; i++) {
                        chessPieces[i,2,0] = SpawnSinglePiece(ChessPieceType.Pawn, 1);
                    }
                }

                if(boardType == ChessBoardType.SmallLeft) {
                    //White Team
                    chessPieces[1,1,0] = SpawnSinglePiece(ChessPieceType.Rook, 1);
                    chessPieces[1,0,0] = SpawnSinglePiece(ChessPieceType.Knight, 1);
                    chessPieces[0,1,0] = SpawnSinglePiece(ChessPieceType.Pawn, 1);
                    chessPieces[0,0,0] = SpawnSinglePiece(ChessPieceType.Pawn, 1);
                }

                if(boardType == ChessBoardType.SmallRight) {
                    //White Team
                    chessPieces[1,0,0] = SpawnSinglePiece(ChessPieceType.Knight, 1);
                    chessPieces[0,0,0] = SpawnSinglePiece(ChessPieceType.Rook, 1);
                    chessPieces[0,1,0] = SpawnSinglePiece(ChessPieceType.Pawn, 1);
                    chessPieces[1,1,0] = SpawnSinglePiece(ChessPieceType.Pawn, 1);
                }
            }
        }
    }
    private ChessPieces SpawnSinglePiece(ChessPieceType type, int team) 
    {
        ChessPieces cp = Instantiate(prefabs[(int) type -1], transform).GetComponent<ChessPieces>();
        cp.type = type;
        cp.team = team;

        if(boardType == ChessBoardType.Large) {
            cp.transform.localScale = new Vector3(tileSize * 600, tileSize * 600, tileSize * 600);
        } else if (boardType == ChessBoardType.SmallLeft || boardType == ChessBoardType.SmallRight) {
            cp.transform.localScale = new Vector3(tileSize/2.5f, tileSize/2.5f, tileSize/2.5f);
        }
        cp.GetComponent<MeshRenderer>().material = teamMaterials[team];

        return cp;
    }
}


