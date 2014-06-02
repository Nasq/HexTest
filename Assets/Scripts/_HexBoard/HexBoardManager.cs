using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;
using System.IO;
using System.Threading;

/// <summary>
/// Manage a HexBoard.
/// </summary>
public class HexBoardManager : MonoBehaviour {

    // Components.
    private GameManager gameManager; 
    private TileList tileList; // List of tile prefabs.

    // Prefabs & Objects.
    public LineRenderer pathLine;

    // XML Doc for saving.
    private XmlDocument myLevelSave;

    // My Hexagon Tile Board.
    public HexBoard myBoard;

    public bool debug = false;
    public float pathTime = 0.0f;
    public bool loadingBoard = false;

    // Squares around a hex.
    private static Point2[] aroundEven = new Point2[]{ new Point2(-1, -1), new Point2(0, -1), new Point2(-1, 0), new Point2(1, 0), new Point2(-1, 1), new Point2(0, 1) };
    private static Point2[] aroundOdd = new Point2[]{ new Point2(0, -1), new Point2(1, -1), new Point2(-1, 0), new Point2(1, 0), new Point2(0, 1), new Point2(1, 1) };


    // Queue of objects to be instantiated.
    private List<HexTile> buildQueue;

    // Queue of objects to be destroyed.
    private List<HexTile> destroyQueue;

    // Maximum time the loop can run.
    private float maxTime = 0.1f;


    /// <summary>The name of the last board saved by the player.</summary>
    public string lastBoardSaved {
        set { PlayerPrefs.SetString("lastBoardSaved", value); }
        get { return PlayerPrefs.GetString("lastBoardSaved"); }
    }


    /// <summary>Cache Components.</summary>
    void Awake() {
        tileList = GetComponent<TileList>();
        gameManager = GetComponent<GameManager>();
    }


    /// <summary>Set up and generate the Board.</summary>
    void Start() {
        float startemp = Time.realtimeSinceStartup;

        // Try loading the last board saved.
        if (lastBoardSaved != null && lastBoardSaved.Length > 0) 
            myBoard = LoadBoard(lastBoardSaved);

        // If no board was loaded, make a new one.
        // Else initialize the loaded board. TODO: Change to Build.
        if (myBoard == null) { newBoard(20, 20); } 
        else {  myBoard.Initialize(tileList); }

        Debug.Log("Start() over, " + (Time.realtimeSinceStartup - startemp) + "secs.");

        var tmp = Loom.Current;
    }


    public void LoadInitialize(string name) {
        DestroyBoard();
        myBoard = LoadBoard(name);
        myBoard.Initialize(tileList);
    }


    public void newBoard(int x, int y) {
        float temp = Time.realtimeSinceStartup;
        myBoard = new HexBoard(x, y, tileList);
        myBoard.SetSizes(tileList.hexagon);
        myBoard.BuildBoard();
        Debug.Log("New Board Created in " + (Time.realtimeSinceStartup - temp) + "secs. (" + x + ", " + y + ")");
    }

    public HexBoard LoadBoard(string boardName) {
#if UNITY_WEBPLAYER
        return null;
#endif
#if !UNITY_WEBPLAYER
        XmlSerializer serializer = new XmlSerializer(typeof(HexBoard));
        try {
            using (XmlReader reader = XmlReader.Create(Application.dataPath + "/data/" + boardName + ".xml")){
                HexBoard temp = (HexBoard) serializer.Deserialize(reader);
                gameManager.LoadInfo(temp.boardWidth, temp.boardHeight, boardName);
                return temp;
            }

            // Old code for keepsies
            /* myLevelSave.Load(Application.dataPath + "/data/" + boardName + ".xml");
            StringWriter writer = new StringWriter(myLevelSave.d);
            return (HexBoard)serializer.Deserialize(new StringWriter(myLevelSave.OuterXml));*/

        } catch (FileNotFoundException fnf) {
            Debug.Log("Load Error 404: " + fnf.FileName);
            return null;
        } catch {
            Debug.Log("Load Error 000: Dont Know.");
            return null;
        }
#endif
    }


    public void SaveBoard(string boardName) {
        XmlSerializer serializer = new XmlSerializer(typeof(HexBoard));
        string xml;
        using (StringWriter writer = new StringWriter()) {
            serializer.Serialize(writer, myBoard);
            xml = writer.ToString();
        }
        myLevelSave = new XmlDocument();
        myLevelSave.LoadXml(xml);
        myLevelSave.Save(Application.dataPath + "/data/" + boardName + ".xml");
        lastBoardSaved = boardName;
    }


    public void ToggleTile(Point2 pos) {
        if (myBoard.GetTile(pos).tileType == HexTileType.Flat) {
            // Switch to Hill
            myBoard.GetTile(pos).Destroy();
            myBoard.SetTile(pos, new HexTile(pos));
            myBoard.GetTile(pos).SetTileType(HexTileType.Hill, myBoard.CalcHexPosition(pos), tileList);

            //myLevel[pos.y][pos.x] = 2;
            //Destroy(tileList[pos.y][pos.x]);
            //createHex(hexagonHill, pos);
            //toggleEffect.transform.position = calcHexPosition(pos) + (Vector3.up * 0.4f);
            //toggleEffect.Play();

            GameObject particles = (GameObject)Instantiate(tileList.toggleEffect, myBoard.CalcHexPosition(pos) + (Vector3.up * 0.4f), Quaternion.identity);
            particles.GetComponent<ParticleSystem>().Play();
            Destroy(particles, 2);
        } else {
            //Switch to Flat
            myBoard.GetTile(pos).Destroy();
            myBoard.SetTile(pos, new HexTile(pos));
            myBoard.GetTile(pos).SetTileType(HexTileType.Flat, myBoard.CalcHexPosition(pos), tileList);

            GameObject particles = (GameObject)Instantiate(tileList.toggleEffect, myBoard.CalcHexPosition(pos) + (Vector3.up * 0.4f), Quaternion.identity);
            particles.GetComponent<ParticleSystem>().Play();
            Destroy(particles, 2);
        }
    }

    public void SetTile(Point2 pos, HexTileType h) {
        myBoard.GetTile(pos).Destroy();
        myBoard.SetTile(pos, new HexTile(pos));
        myBoard.GetTile(pos).SetTileType(h, myBoard.CalcHexPosition(pos), tileList);

        GameObject particles = (GameObject)Instantiate(tileList.toggleEffect, myBoard.CalcHexPosition(pos) + (Vector3.up * 0.4f), Quaternion.identity);
        particles.GetComponent<ParticleSystem>().Play();
        Destroy(particles, 2);
    }


    /// <summary>Set all tiles to one tile type.</summary>
    /// <param name="type">Type of tile to use.</param>
    public void SetAllTiles(HexTileType type) {
        myBoard.SetAllTiles(type);
    }


    /// <summary>Queue board for destruction.</summary>
    public void DestroyBoard() {
        if(myBoard != null) myBoard.Destroy();
    }


    //-------------------------------------------------------------------------------------
    // PATHFINDING
    //-------------------------------------------------------------------------------------

    SearchNode search;

    List<Point2> OpenList = new List<Point2>();
    List<Point2> ClosedList = new List<Point2>();
    List<Point2> myPath = new List<Point2>();



    public List<Vector3> getPath(Point2 startPos, Point2 endPos) {

        List<Vector3> thePath = new List<Vector3>();

        SearchNode search = FindPath(startPos, endPos);

        Vector3 temp;
        if (search != null) {
            thePath.Clear();
            while (search.next != null) {
                temp = myBoard.CalcHexPosition(search.position) + new Vector3(0, 0.01f, 0);
                thePath.Add(temp);
                myPath.Add(search.position);
                search = search.next;
            }
            myPath.Add(search.position);
            temp = myBoard.CalcHexPosition(search.position) + new Vector3(0, 0.01f, 0);
            thePath.Add(temp);
        } else {
            Debug.Log("No path fround from " + startPos.ToString() + " to " + endPos.ToString());
        }

        return thePath;
    }

    public void DrawPath(List<Vector3> path) {
        Debug.Log("Drawing Path");
        pathLine.SetVertexCount(path.Count);
        for (int i = 0; i < path.Count; i++) {
            pathLine.SetPosition(i, path[i]);

        }

    }


    public void getPathAsync(Point2 startPos, Point2 endPos) {

        float startTime = Time.realtimeSinceStartup;

        //Run the action on a new thread
        Loom.RunAsync(() => {

            var thePath = getPath(startPos, endPos);

            //Run some code on the main thread
            Loom.QueueOnMainThread(() => {

                DrawPath(thePath);
            });
 
        });

        pathTime = Time.realtimeSinceStartup - startTime;

    }

    // Is the hex availble to be moved on to?
    bool hex_accessible(Point2 pos) {
        if (pos.y >= myBoard.boardHeight || pos.y < 0
            || pos.x >= myBoard.boardWidth || pos.x < 0) {
            Debug.Log(pos.x + " * " + pos.y + " False.");
            return false;
        }
        if (myBoard.GetTile(pos) == null
            || !myBoard.GetTile(pos).isWalkable) {
            Debug.Log(pos.x + " * " + pos.y + " False.");
            return false;
        }

        Debug.Log(pos.x + " * " + pos.y + " true.");
        return true;
    }


    // Probably needs a good hex-oriented rework. Use for Manhattan heuristic.
    int hex_distance(int x1, int y1, int x2,int y2) {
			int dx = System.Math.Abs(x1-x2);
            int dy = System.Math.Abs(y2 - y1);
            return (int)System.Math.Sqrt((dx * dx) + (dy * dy));
			//dx = Math.abs(x1-x2);
			//dy = Math.abs(y1-y2);
			//return dx + dy;
    }
    

    public SearchNode FindPath(Point2 start, Point2 end) {
        //note we just flip start and end here so you don't have to.            
        return FindPathReversed(end, start);
    }
    

    /// <summary>
    /// Method that switfly finds the best path from start to end. Doesn't reverse outcome
    /// </summary>
    /// <returns>The end SearchNode where each .next is a step back</returns>
    private SearchNode FindPathReversed(Point2 start, Point2 end) {
        SearchNode startNode = new SearchNode(start, 0, 0, null);

        MinHeap openList = new MinHeap();
        openList.Add(startNode);

        int sx = myBoard.boardWidth;
        int sy = myBoard.boardHeight;
        bool[] brWorld = new bool[sx * sy];
        brWorld[start.x + (start.y) * sx] = true;

        Point2[] tilesAround;
        while (openList.HasNext()) {
            SearchNode current = openList.ExtractFirst();
            tilesAround = (current.position.y % 2 == 0)? aroundEven : aroundOdd;

            for (int i = 0; i < tilesAround.Length; i++) {
                Point2 tmpy = tilesAround[i];
                Point2 tmp = current.position + tmpy;
                if (tmp == end) {
                    //int cst = tileList[tmp.y][tmp.x].GetComponent<HexTile>().stepCost;
                    return new SearchNode(end, current.pathCost, current.cost, current);
                }


                int brWorldIdx = tmp.x + (tmp.y) * sx;

                if (hex_accessible(tmp) && brWorld[brWorldIdx] == false) {
                    brWorld[brWorldIdx] = true;
                    //int pathCost = current.pathCost + surr.Cost;
                    int pathCost = current.pathCost + 1;
                    int cost = pathCost + tmp.GetDistanceSquared(end);
                    SearchNode node = new SearchNode(tmp, cost, pathCost, current);
                    openList.Add(node);
                }
            }
        }
        
        
        return null; //no path found
    }


    //--------------------------------------------------------------------------------------------------
    //-GUI----------------------------------------------------------------------------------------------
    //--------------------------------------------------------------------------------------------------


    void OnGUI() {
        //string output = "";
       /* if (thePath != null && thePath.Count > 1) {
            for (int i = 0; i < thePath.Count - 1; i++) {
                //Debug.Log("DRAWING PATH");
                //Debug.DrawLine(thePath[i], thePath[i + 1], Color.blue, 2.0f);
                output += "" + myPath[i].ToString() + " to " + myPath[i+1].ToString() + " || ";
            }
        }*/

        if (debug) {
            // Debug Info
            GUI.Label(new Rect(20, 25, 1000, 20), "MousePos: " + Input.mousePosition.ToString());
            //GUI.Label(new Rect(20, 50, 100, 20), "PathFind: " + thePath.Count);
            GUI.Label(new Rect(20, 75, 1000, 20), "PathTime: " + pathTime + " ");
            GUI.Label(new Rect(20, 100, 1000, 20), "AppDataPath: " + Application.dataPath);
        }

    }

}