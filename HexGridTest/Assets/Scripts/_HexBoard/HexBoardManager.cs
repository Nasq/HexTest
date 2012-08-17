using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;
using System.IO;
using System.Threading;

public class hexNode {
    public GameObject hexObject; // the hex prefab
    public bool isWalkable = true;
    public bool isOccupied = false;
    public int stepValue = 1;
}



public class HexBoardManager : MonoBehaviour {

    // Components
    private GameManager gameManager;
    private TileList tileList; // List of tile prefabs.

    // Line Renderer for Pathfinding
    public LineRenderer pathLine;

    //
    private XmlDocument myLevelSave;

    public bool debug = false;

    // My Hexagon Tile Board.
    public HexBoard myBoard;

    float pathTime = 0f;


    // Squares around a hex.
    private static Point2[] aroundEven = new Point2[]{
        new Point2(-1, -1), new Point2(0, -1),
        new Point2(-1, 0), new Point2(1, 0),
        new Point2(-1, 1), new Point2(0, 1) };

    private static Point2[] aroundOdd = new Point2[]{
        new Point2(0, -1), new Point2(1, -1),
        new Point2(-1, 0), new Point2(1, 0),
        new Point2(0, 1), new Point2(1, 1) };


    string lastBoardSaved {
        set { PlayerPrefs.SetString("lastBoardSaved", value); }
        get { return PlayerPrefs.GetString("lastBoardSaved"); }
    }

    void Awake() {
        tileList = GetComponent<TileList>();
        gameManager = GetComponent<GameManager>();
    }

    /// <summary>
    /// Set up and generate the Board.
    /// </summary>
    void Start() {
        float startemp = Time.realtimeSinceStartup;
            if (lastBoardSaved != null && lastBoardSaved.Length > 0) myBoard = LoadBoard(lastBoardSaved);
            if (myBoard == null) {
                newBoard(50, 50);
            } else {
                myBoard.Initialize(tileList);
            }
            Debug.Log("Start() over, " + (Time.realtimeSinceStartup - startemp) + "secs.");
    }



    void Update() {


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

    public void SetAllTiles(HexTileType type) {
        myBoard.SetAllTiles(type);
    }
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

    List<Vector3> thePath = new List<Vector3>();

    public void getPath(Point2 startPos, Point2 endPos) {

        // Run Different Pathfinding Solutions in here.
        float startTime = Time.realtimeSinceStartup;
        SearchNode search = FindPath(startPos, endPos);
        pathTime = Time.realtimeSinceStartup - startTime;

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

        pathLine.SetVertexCount(thePath.Count);
        for (int i = 0; i < thePath.Count; i++) {
            pathLine.SetPosition(i, thePath[i]);
            
        }

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

    //bool set_first = false;
    /*
    // A* Pathfinding with Manhatan Heuristics for Hexagons.
    void path(int start_x, int start_y, int end_x, int end_y) {
        // Check cases path is impossible from the start.
        var error=0;
        if(start_x == end_x && start_y == end_y) error=1;
        if(!hex_accessible(start_x, start_y)) error=1;
        if(!hex_accessible(end_x, end_y)) error=1;
        if(error==1) {
            Debug.Log("Path is impossible to create.");
            return;
        }

        int[] openlist = new int[gridWidth * gridHeight + 2];
        int[] openlist_x = new int[gridWidth];
        int[] openlist_y = new int[gridHeight];
        int[,] statelist = new int[gridWidth+1, gridHeight+1]; // current open or closed state
        int[,] openlist_g = new int[gridWidth+1, gridHeight+1];
        int[,] openlist_f = new int[gridWidth+1, gridHeight+1];
        int[,] openlist_h = new int[gridWidth+1, gridHeight+1];
        int[,] parent_x = new int[gridWidth+1, gridHeight+1];
        int[,] parent_y = new int[gridWidth+1, gridHeight+1];
        int[,] path = new int[gridWidth * gridHeight+2, 2];

        int select_x = 0;
        int select_y = 0;
        int node_x = 0;
        int node_y = 0;
        int counter = 1; // Openlist_ID counter
        int selected_id = 0; // Actual Openlist ID
			
        // Add start coordinates to openlist.
        openlist[1] = 1;
        openlist_x[1] = start_x;
        openlist_y[1] = start_y;
        openlist_f[start_x, start_y] = 0;
        openlist_h[start_x, start_y] = 0;
        openlist_g[start_x, start_y] = 0;
        statelist[start_x, start_y] = 1; 
			
        int lowest_found = 0;
        int lowest_x = 0;
        int lowest_y = 0;
        //int node_x = 0;
        // int node_y = 0;

        // Try to find the path until the target coordinate is found
        while (statelist[end_x, end_y] != 1) {
            set_first = true;
            // Find lowest F in openlist
            for (int i =0; i < openlist.Length; i++) {
                if(openlist[i] == 1){
                    select_x = openlist_x[i]; 
                    select_y = openlist_y[i]; 
                    if(set_first == true) {
                        lowest_found = openlist_f[select_x, select_y];
                        set_first = false;
                    }
                    if (openlist_f[select_x, select_y] <= lowest_found) {
                        lowest_found = openlist_f[select_x, select_y];
                        lowest_x = openlist_x[i];
                        lowest_y = openlist_y[i];
                        selected_id = i;
                    }
                }
            }
            if(set_first==true) {
                // open_list is empty
                Debug.Log("No possible route can be found.");
                return;
            }
            // add it lowest F as closed to the statelist and remove from openlist
            statelist[lowest_x, lowest_y] = 2;
            openlist[selected_id]= 0;
            // Add connected nodes to the openlist
            for(int i = 1; i < 7; i++) {
                // Run node update for 6 neighbouring tiles.
                switch(i){
                    case 1:
                        node_x = lowest_x-1;
                        node_y = lowest_y;						
                        break;
                    case 2:
                        node_x = lowest_x;
                        node_y = lowest_y-1;						
                        break;
                    case 3:
                        node_x = lowest_x+1;
                        node_y = lowest_y-1;						
                        break;
                    case 4:
                        node_x = lowest_x+1;
                        node_y = lowest_y;						
                        break;
                    case 5:
                        node_x = lowest_x;
                        node_y = lowest_y+1;
                        break;
                    case 6:
                        node_x = lowest_x-1;
                        node_y = lowest_y+1;
                        break;
                }

                if (hex_accessible(node_x,node_y)) {
                    if(statelist[node_x, node_y] == 1) {
                        if(openlist_g[node_x, node_y] < openlist_g[lowest_x, lowest_y]) {
                            parent_x[lowest_x, lowest_y] = node_x;
                            parent_y[lowest_x, lowest_y] = node_y;
                            openlist_g[lowest_x, lowest_y] = openlist_g[node_x, node_y] + 10;
                            openlist_f[lowest_x, lowest_y] = openlist_g[lowest_x, lowest_y] + openlist_h[lowest_x, lowest_y];
                        }
                    } else if (statelist[node_x, node_y] == 2) {
                        // its on closed list do nothing.
                    } else {
                        counter++;
                        // add to open list
                        Debug.Log("--->" + counter + " len: " + openlist_y.Length + " + " + tileList.Length);
                        openlist[counter] = 1;
                        openlist_x[counter] = node_x;
                        openlist_y[counter] = node_y;
                          
                        statelist[node_x, node_y] = 1;

                        // Set parent
                        parent_x[node_x, node_y] = lowest_x;
                        parent_y[node_x, node_y] = lowest_y;

                        // update H , G and F
                        var ydist = end_y - node_y;
                        if ( ydist < 0 ) ydist = ydist*-1;

                        var xdist = end_x - node_x;
                        if ( xdist < 0 ) xdist = xdist*-1;	
	
                        openlist_h[node_x, node_y] = hex_distance(node_x,node_y,end_x,end_y) * 10;
                        openlist_g[node_x, node_y] = openlist_g[lowest_x, lowest_y] + 10;
                        openlist_f[node_x, node_y] = openlist_g[node_x, node_y] + openlist_h[node_x, node_y];
                    }
                }
            }
        }
			
        // Get Path
        int temp_x=end_x;
        int temp_y=end_y;
        counter = 0;
        while(temp_x != start_x || temp_y != start_y) {
            counter++;
            path[counter, 1] = temp_x;
            path[counter, 2] = temp_y;
            temp_x = parent_x[path[counter, 1], path[counter, 2]];
            temp_y = parent_y[path[counter, 1], path[counter, 2]];
        }
        counter++;
        path[counter, 1] = start_x;
        path[counter, 2] = start_y;
			
        // Draw path.
        while(counter!=0) {
            //document.getElementById('hex_' + path[counter][1] + '_' + path[counter][2]).className = 'hex_blue';
          //  myPath[counter].position = new Vector3(path[counter, 1], 0.1f, path[counter, 2]);
            //counter--;
        }
    }		
    
    */
    //--------------------------------------------------------------------------------------------------
    //--------------------------------------------------------------------------------------------------
    //--------------------------------------------------------------------------------------------------

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
    //--------------------------------------------------------------------------------------------------
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
            GUI.Label(new Rect(20, 50, 100, 20), "PathFind: " + thePath.Count);
            GUI.Label(new Rect(20, 75, 1000, 20), "PathTime: " + pathTime + " ");
            GUI.Label(new Rect(20, 100, 1000, 20), "AppDataPath: " + Application.dataPath);
        }

    }

}