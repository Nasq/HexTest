using UnityEngine;
using System.Collections;
using System.Xml.Serialization;

/// <summary>
/// A board to hold hex tiles.
/// </summary>
public class HexBoard{
    private TileList _tileList;

    [SerializeField]
    public int _boardWidth;
    [SerializeField]
    public int _boardHeight;

    public int boardWidth { get { return _boardWidth; } }
    public int boardHeight { get { return _boardHeight; } }

    public float tileWidth = 0;
    public float tileHeight = 0;

    public HexTile[] tiles;

    public Point2[] spawnPoints = new Point2[] { 
        new Point2(0, 0) };

    /// <summary>Create new Hex Board.</summary>
    /// <param name="width">Width of the board.</param>
    /// <param name="height">Height of the board.</param>
	public HexBoard (int width, int height, TileList tileList) {
        _boardWidth = width;
        _boardHeight = height;
        _tileList = tileList;
        tiles = new HexTile[_boardWidth * _boardHeight];
	}

    // XML Deserialization requires a constructor with no params.
    public HexBoard() : this( 5, 5, null) { }


    public void Initialize(TileList tileList) {
        _tileList = tileList;
        for (int i = 0; i < tiles.Length; i++) {
            if (tiles[i] != null)
                tiles[i].SetTileType(tiles[i].tileType, CalcHexPosition(tiles[i].boardPosition), _tileList);
        }

    }


    public void BuildBoard() {
        SetAllTiles(HexTileType.Flat);
    }


    public HexTile GetTile(Point2 pos) {
        return tiles[pos.x + (pos.y * _boardWidth)];
    }
    public HexTile GetTile(int x, int y) { return GetTile(new Point2(x, y)); }


    public void SetTile(Point2 pos, HexTile tile) {
        tiles[pos.x + (pos.y * _boardWidth)] = tile;
    }
    public void SetTile(int x, int y, HexTile tile) { SetTile(new Point2(x, y), tile); }

    /// <summary>Initialise Hexagon width and height.</summary>
    /// <param name="hexagon">Prefab Hexagon for size reference.</param>
    public void SetSizes(GameObject hexagon) {
        //renderer component attached to the Hex object is used to get the current width and height
        tileWidth = hexagon.renderer.bounds.size.x;
        tileHeight = hexagon.renderer.bounds.size.z;
    }


    /// <summary>
    /// Calculate the position of the first tile, with (0, 0, 0) as the center of the board.
    /// </summary>
    /// <returns>Position of the first tile, with (0, 0, 0) being the center of the grid.</returns>
    private Vector3 CalcStartPosition() {
        float boardX = -((tileWidth * _boardWidth) / 2f);
        float boardY = (((tileHeight / 2f) * 1.5f) * _boardHeight) / 2f - (tileHeight / 2);
        if (tileHeight % 0 != 0) boardY += tileHeight / 8f; // if its an odd number of rows, add 1/8th to center it out.

        return new Vector3(boardX, 0, boardY);
    }


    /// <summary>
    /// Calculate world position of a hex tile given its position in the level array.
    /// </summary>
    /// <param name="gridPos">Board Position.</param>
    /// <returns>Position in world space.</returns>
    public Vector3 CalcHexPosition(Point2 gridPos) {
        //Position of the first hex tile
        Vector3 startPos = CalcStartPosition();

        //Every second row is offset by half of the tile width
        float offset = 0; 
        if (gridPos.y % 2 != 0) offset = tileWidth * 0.5f;
        float x = startPos.x + offset + (gridPos.x * tileWidth);

        //Every new line is offset in z direction by 3/4 of the hexagon height
        float z = startPos.z - gridPos.y * (tileHeight * 0.75f);

        return new Vector3(x, 0, z);
    }


    /// <summary>
    /// Set all tiles to one type.
    /// </summary>
    /// <param name="type">Tile type</param>
    public void SetAllTiles(HexTileType type){
        for(int x = 0; x < _boardWidth; x++){
            for(int y = 0; y < _boardHeight; y++){
                if (GetTile(x, y) != null) GetTile(x, y).Destroy();
                SetTile(x, y, new HexTile(x, y));
                GetTile(x, y).SetTileType(type, CalcHexPosition(new Point2(x, y)), _tileList);
            }
        }
    }


    /// <summary>
    /// Destroy all tiles in the board. 
    /// TODO: This is going to be changed to a object pool implimentation.
    /// </summary>
    public void Destroy() {
        foreach(HexTile t in tiles){
            t.Destroy();
        }
    }
}


/// <summary>
/// Hexagon Tiles to be placed on a Hex Board.
/// </summary>
public class HexTile {

    public Point2 boardPosition;
    public HexTileType tileType;
    public bool isWalkable = true;

    // DONT serialize this (probably cant be anyway).
    [System.Xml.Serialization.XmlIgnoreAttribute]
    public GameObject tileObject;
    
    public HexTile(Point2 pos) {
        boardPosition = pos;
    }

    //Constructor Overload
    public HexTile(int x, int y) : this(new Point2(x, y)) { }
    // XML Deserialization requires a constructor with no params.
    public HexTile() : this(new Point2(0, 0)) { }


    /// <summary>Set the type of this Tile</summary>
    /// <param name="type">Type of Tile.</param>
    /// <param name="objectPos">Position of Tile in world space.</param>
    /// <param name="tileList">List of Tile Prefabs.</param>
    public void SetTileType(HexTileType type, Vector3 objectPos, TileList tileList) {
        switch (type) {
            case HexTileType.Flat:
                SetTileObject(tileList.hexagonFlat, objectPos, true);
                break;
            case HexTileType.FlatSand:
                SetTileObject(tileList.hexagonFlatSand, objectPos, true);
                break;
            case HexTileType.Hill:
                SetTileObject(tileList.hexagonHill, objectPos, false);
                break;
            default:
                tileObject = null; isWalkable = false;
                break;
        }
    }
    
    public void SetTileObject(GameObject hexGO, Vector3 objectPos, bool walkable) {
        tileObject = (GameObject)GameObject.Instantiate(hexGO, objectPos, hexGO.transform.rotation);
        //GameObjectManager.instance.QueueInstantiate(tileObject, hexGO, objectPos, hexGO.transform.rotation);
        //tileObject.name = hexGO.name;
        tileObject.GetComponent<TilePos>().arrayPos = boardPosition;
        isWalkable = walkable;
    }

    public void Destroy() {
        if (tileObject != null) GameObject.Destroy(tileObject);
        //if(tileObject != null)GameObjectManager.instance.QueueDestroy(tileObject);
    }

}


public enum HexTileType {
    Empty = 0,
    Flat = 1,
    Hill = 2,
    FlatSand = 3
}